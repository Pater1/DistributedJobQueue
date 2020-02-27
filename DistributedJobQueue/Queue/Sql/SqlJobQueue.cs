using DistributedJobQueue.Client;
using DistributedJobQueue.Fulfillments;
using DistributedJobQueue.Job;
using DistributedJobQueue.Job.Wrappers;
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
        private string InProcessTable { get; } = "JobsInProcess";
        private string UnfinishedJobs { get; } = "UnfinishedJobs";
        private List<Guid> InProcessLocal { get; } = new List<Guid>();

        public async Task<(bool, IJob)> TryDequeueAsync(IEnumerable<IFulfillment> fulfillments = null)
        {
            string[] reqTags = fulfillments.GetFulfillmentTags().Select(x => x.ToLower()).ToArray();

            Guid[] possibleJobs = 
                new Query<DbConnection>(ConnectionFactory, 
                    $"SELECT JobId as JobId, RequirementTag as RequirementTag FROM {RequirementsTable} WHERE RequirementTag IN ({reqTags.Select(x => $"'{x}'").Aggregate((a, b) => $"{a}, {b}")})"
                ).EnumerateAsItems<SqlRequirement>().Select(x => x.JobId).Distinct().ToArray();

            if (!possibleJobs.Any())
            {
                return (false, null);
            }

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
                (await Task.WhenAll(
                    new Query<DbConnection>(ConnectionFactory,
                        $"SELECT JobId as JobId, JobType as JobType, JobJson as JobJson FROM {QueueTable} WHERE JobId IN ({allJobsReqs.Select(x => $"'{x.Key.ToString()}'").Distinct().Aggregate((a, b) => $"{a}, {b}")})"
                    ).EnumerateAsItems<SqlJob>()
                    .Select(x =>
                    {
                        bool success = x.TryDeSqlize(out IJob jb);
                        return (success, jb, x);
                    })
                    .Where(x => x.success)
                    .Select(async x => 
                        (x.jb, x.x, await x.jb.Requirement.FulfilledByAsync(fulfillments))
                    )
                ))
                .Where(x => x.Item3)                      
                .Select(x => (x.jb, x.x, allJobsReqs[x.x.JobId]));

            if (!jobs.Any())
            {
                return (false, null);
            }

            foreach(var jb in jobs)
            {
                try
                {
                    string q =
                        "START TRANSACTION;"
                        + Environment.NewLine +
                        $"DELETE FROM {QueueTable} WHERE JobId = '{jb.Item1.JobId.ToString()}';"
                        + Environment.NewLine +
                        $"DELETE FROM {RequirementsTable} WHERE JobId = '{jb.Item1.JobId.ToString()}';"
                        + Environment.NewLine +
                        $"INSERT INTO {InProcessTable}(JobId, JobType, LastHeartbeat, JobJson) VALUES ('{jb.Item2.JobId.ToString()}', '{jb.Item2.JobTypeName}', NOW(), '{jb.Item2.JobJson}');"
                        + Environment.NewLine +
                        "COMMIT;"
                        ;

                    new Query<DbConnection>(ConnectionFactory,
                        q
                    ).ExecuteNonQuery();

                    InProcessLocal.Add(jb.Item1.JobId);
                    this.RegisterJobDispatch(jb.Item1);

                    return (true,
                        new OnCompleteJobWrapper(
                            new HeartbeatJobWrapper(jb.Item1, TimeSpan.FromSeconds(1), async () =>
                                new Query<DbConnection>(ConnectionFactory,
                                    $"UPDATE {InProcessTable} SET LastHeartbeat = NOW() WHERE JobId = '{jb.Item2.JobId.ToString()}';"
                                ).ExecuteNonQuery()
                            )
                        , 
                            async () =>
                            {
                                InProcessLocal.Remove(jb.Item1.JobId);
                                this.UnregisterJobDispatch(jb.Item1);
                                new Query<DbConnection>(ConnectionFactory,
                                    $"DELETE FROM {InProcessTable} WHERE JobId = '{jb.Item1.JobId.ToString()}';"
                                ).ExecuteNonQuery();
                            }

                        )
                    );
                }
                catch(Exception e)
                {
                    new Query<DbConnection>(ConnectionFactory,
                        $"ROLLBACK;"
                    ).ExecuteNonQuery();
                }
            }

            return (false, null);
        }

        public Task<bool> TryEnqueueAsync(IJob job)
        {
            if(job.JobId == default)
            {
                //TODO: check that guid not used? (may be a non-issue with GUID's 128-bits of randomness)
                job.JobId = Guid.NewGuid();
            }

            var jb = job.Sqlize();

            if (!jb.Item2.Any())
            {
                //TODO: log that job had no requirements
                return Task.FromResult(false);
            }

            string query = $"INSERT INTO {QueueTable} (JobId, JobType, JobJson) VALUES ('{jb.Item1.JobId.ToString()}', '{jb.Item1.JobTypeName}', '{jb.Item1.JobJson}');"
                        + Environment.NewLine +
                        jb.Item2.Select(x => $"INSERT INTO {RequirementsTable} (JobId, RequirementTag) VALUES ('{x.JobId.ToString()}', '{x.RequirementTag}');")
                                .Aggregate((a, b) => $"{a}{Environment.NewLine}{b}");

            new Query<DbConnection>(ConnectionFactory, query).ExecuteNonQuery();
            return Task.FromResult(true);
        }

        public async Task<bool> WaitForCompletionAsync(Guid jobId)
        {
            //TODO: detect if job asks to wait on itself & error

            Query<DbConnection> q = new Query<DbConnection>(ConnectionFactory,
                $"SELECT COUNT(*) FROM {UnfinishedJobs} WHERE JobId = '{jobId.ToString()}';"
            );//.ExecuteScalar();

            while (InProcessLocal.Contains(jobId) || (int.TryParse(q.ExecuteScalar().ToString(), out int cnt) && cnt > 0))
            {
                await Task.Delay(10);
            }

            return true;
        }
    }
}
