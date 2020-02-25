using DistributedJobQueue.Job;
using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Queue.Sql
{
    public class SqlJobQueue : IJobQueue
    {
        public SqlJobQueue(Func<DbConnection> connectionFactory)
        {
            ConnectionFactory = connectionFactory;
        }
        private Func<DbConnection> ConnectionFactory { get; }
        private string QueueTable { get; } = "JobQueue";
        private string RequirementsTable { get; } = "JobRequirements";

        public async Task<(bool, IJob)> TryDequeueAsync(IRequirement requirementsFulfillable = null)
        {
            string[] reqTags = requirementsFulfillable.GetRequirementTags().Select(x => x.ToLower()).ToArray();

            Guid[] possibleJobs = 
                new Query<DbConnection>(ConnectionFactory, 
                    $"SELECT JobId as JobId, RequirementTag as RequirementTag FROM {RequirementsTable} WHERE RequirementTag IN ({reqTags.Select(x => $"'{x}'").Aggregate((a, b) => $"{a}, {b}")})"
                ).EnumerateAsItems<SqlRequirement>().Select(x => x.JobId).Distinct().ToArray();

            Dictionary<Guid, SqlRequirement[]> allJobsReqs =
                new Query<DbConnection>(ConnectionFactory,
                    $"SELECT JobId as JobId, RequirementTag as RequirementTag FROM {RequirementsTable} WHERE JobId IN ({possibleJobs.Select(x => $"'{x.ToString()}'").Aggregate((a, b) => $"{a}, {b}")})"
                ).EnumerateAsItems<SqlRequirement>()
                .GroupBy(x => x.JobId)
                .Where(x => x.Select(y => y.RequirementTag.ToLower()).Except(reqTags).Count() <= 0)
                .ToDictionary(x => x.Key, x => x.ToArray());

            if (!allJobsReqs.Any())
            {
                return (false, null);
            }

            IEnumerable<(IJob, SqlJob, SqlRequirement[])> jobs =
                new Query<DbConnection>(ConnectionFactory,
                    $"SELECT JobId as JobId, JobType as JobType, JobJson as JobJson FROM {QueueTable} WHERE JobId IN ({allJobsReqs.Select(x => $"'{x.Key.ToString()}'").Distinct().Aggregate((a, b) => $"{a}, {b}")})"
                ).EnumerateAsItems<SqlJob>()
                .Select(x =>
                {
                    bool success = x.TryDeSqlize(out IJob jb);
                    return (success, jb, x);
                })
                .Where(x => x.success)
                .Select(x => (x.jb, x.x))
                .Where(x => requirementsFulfillable.FullfillsAsync(x.jb.Requirement).Result)//TODO: proper async to avoid deadlock
                .Select(x => (x.jb, x.x, allJobsReqs[x.x.JobId]));

            if (!jobs.Any())
            {
                return (false, null);
            }

            foreach(var jb in jobs)
            {
                try
                {
                    new Query<DbConnection>(ConnectionFactory,
                        $"DELETE FROM {QueueTable} WHERE JobId = '{jb.Item1.JobId.ToString()}';"
                        + Environment.NewLine +
                        $"DELETE FROM {RequirementsTable} WHERE JobId = '{jb.Item1.JobId.ToString()}';"
                    ).ExecuteNonQuery();

                    return (true, jb.Item1);
                }
                catch
                {
                    new Query<DbConnection>(ConnectionFactory,
                        $"INSERT INTO {QueueTable} (JobId, JobType, JobJson) VALUES ('{jb.Item2.JobId.ToString()}', '{jb.Item2.JobTypeName}', '{jb.Item2.JobJson}');"
                        + Environment.NewLine +
                        jb.Item3.Select(x => $"INSERT INTO {RequirementsTable} (JobId, RequirementTag) VALUES ('{x.JobId.ToString()}', '{x.RequirementTag}');")
                        .Aggregate((a,b) => $"{a}{Environment.NewLine},{b}")
                    ).ExecuteNonQuery();
                }
            }

            return (false, null);
        }

        public Task<bool> TryEnqueueAsync(IJob job)
        {
            var jb = job.Sqlize();
            new Query<DbConnection>(ConnectionFactory,
                        $"INSERT INTO {QueueTable} (JobId, JobType, JobJson) VALUES ('{jb.Item1.JobId.ToString()}', '{jb.Item1.JobTypeName}', '{jb.Item1.JobJson}');"
                        + Environment.NewLine +
                        jb.Item2.Select(x => $"INSERT INTO {RequirementsTable} (JobId, RequirementTag) VALUES ('{x.JobId.ToString()}', '{x.RequirementTag}');")
                        .Aggregate((a, b) => $"{a}{Environment.NewLine},{b}")
                    ).ExecuteNonQuery();
            return Task.FromResult(true);
        }
    }
}
