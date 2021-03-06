﻿using DistributedJobQueue.Client;
using DistributedJobQueue.Fulfillments;
using DistributedJobQueue.Job;
using DistributedJobQueue.Job.Wrappers;
using DistributedJobQueue.Requirements;
using Newtonsoft.Json;
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
        //private string InProcessTable { get; } = "JobsInProcess";
        private string UnfinishedJobs { get; } = "UnfinishedJobs";
        private List<Guid> InProcessLocal { get; } = new List<Guid>();

        public async Task<(bool, IJob)> TryDequeueAsync(IEnumerable<IFulfillment> fulfillments = null)
        {
            string[] reqTags = fulfillments.GetFulfillmentTags().Select(x => x.ToLower()).ToArray();

            Guid[] possibleJobs = 
                new Query<DbConnection>(ConnectionFactory, 
                    $"SELECT * FROM {RequirementsTable} WHERE RequirementTag IN ({reqTags.Select(x => $"'{x}'").Aggregate((a, b) => $"{a}, {b}")})"
                ).EnumerateAsItems<SqlRequirement>().Select(x => x.JobId).Distinct().ToArray();

            if (!possibleJobs.Any())
            {
                return (false, null);
            }

            Dictionary<Guid, SqlRequirement[]> allJobsReqs =
                new Query<DbConnection>(ConnectionFactory,
                    $"SELECT * FROM {RequirementsTable} WHERE JobId IN ({possibleJobs.Select(x => $"'{x.ToString()}'").Aggregate((a, b) => $"{a}, {b}")})"
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
                        $"SELECT * FROM AvaliableJobs WHERE JobId IN ({allJobsReqs.Select(x => $"'{x.Key.ToString()}'").Distinct().Aggregate((a, b) => $"{a}, {b}")})"
                    ).EnumerateAsItems<SqlJob>()
                    .Select(x =>
                    {
                        bool success = x.TryDeSqlize(out IJob jb);
                        return (success, jb, x);
                    })
                    .Where(x => x.success)
                    .Select(async x => 
                        (x.jb, x.x, await x.jb.ReadRequirement.FulfilledByAsync(fulfillments))
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
                        $"UPDATE {QueueTable} SET TimeStarted = NOW() WHERE JobId = '{jb.Item2.JobId.ToString()}';"
                        + Environment.NewLine +
                        $"UPDATE {QueueTable} SET Status = '{JobStatus.InProcess.Json()}' WHERE JobId = '{jb.Item2.JobId.ToString()}';"
                        + Environment.NewLine +
                        "COMMIT;"
                        ;

                    new Query<DbConnection>(ConnectionFactory,
                        q
                    ).ExecuteNonQuery();

                    InProcessLocal.Add(jb.Item1.JobId);
                    this.RegisterJobDispatch(jb.Item1);

                    return (true,
                        //TODO: factory pattern this to make more readable
                        new OnErrorJobWrapper(
                            new OnCompleteJobWrapper(
                                new HeartbeatJobWrapper(
                                    jb.Item1
                                , 
                                    TimeSpan.FromSeconds(1)
                                ,
                                    async () =>
                                        new Query<DbConnection>(ConnectionFactory,
                                            $"UPDATE {QueueTable} SET LastHeartbeat = NOW() WHERE JobId = '{jb.Item2.JobId.ToString()}';"
                                        ).ExecuteNonQuery()
                                )
                            , 
                                async () =>
                                {
                                    InProcessLocal.Remove(jb.Item1.JobId);
                                    this.UnregisterJobDispatch(jb.Item1);
                                    new Query<DbConnection>(ConnectionFactory,
                                        "START TRANSACTION;"
                                        + Environment.NewLine +
                                        $"UPDATE {QueueTable} SET TimeFinished = NOW() WHERE JobId = '{jb.Item2.JobId.ToString()}';"
                                        + Environment.NewLine +
                                        $"UPDATE {QueueTable} SET Status = '{JobStatus.Done.Json()}' WHERE JobId = '{jb.Item2.JobId.ToString()}';"
                                        + Environment.NewLine +
                                        "COMMIT;"
                                    ).ExecuteNonQuery();
                                }
                            )
                        ,
                            async (e) =>
                            {
                                InProcessLocal.Remove(jb.Item1.JobId);
                                this.UnregisterJobDispatch(jb.Item1);
                                new Query<DbConnection>(ConnectionFactory,
                                    "START TRANSACTION;"
                                    + Environment.NewLine +
                                    $"UPDATE {QueueTable} SET TimeFinished = NOW() WHERE JobId = '{jb.Item2.JobId.ToString()}';"
                                    + Environment.NewLine +
                                    $"UPDATE {QueueTable} SET Status = '{(JobStatus.Done | JobStatus.Error | JobStatus.WithReturnValue).Json()}' WHERE JobId = '{jb.Item2.JobId.ToString()}';"
                                    + Environment.NewLine +
                                    $"UPDATE {QueueTable} SET ReturnJson = '{JsonConvert.SerializeObject(e)}' WHERE JobId = '{jb.Item2.JobId.ToString()}';"
                                    + Environment.NewLine +
                                    $"UPDATE {QueueTable} SET ReturnType = '{e.GetType().Name}' WHERE JobId = '{jb.Item2.JobId.ToString()}';"
                                    + Environment.NewLine +
                                    "COMMIT;"
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
            if(job is IDeterministicIdJob)
            {
                try
                {
                    job.JobId = (job as IDeterministicIdJob).GenerateJobId();
                }
                catch
                {
                    job.JobId = (job as IDeterministicIdJob).GenerateIdFromJsonHash();
                }
            }
            if(job.JobId == default)
            {
                //TODO: check that guid not used? (may be a non-issue with GUID's 128-bits of randomness)
                job.JobId = Guid.NewGuid();
            }

            var jb = job.Sqlize();
            jb.Item1.TimeEnqueued = DateTime.UtcNow;

            if (!jb.Item2.Any())
            {
                //TODO: log that job had no requirements
                return Task.FromResult(false);
            }

            string query =
                        "START TRANSACTION;"
                        + Environment.NewLine +
                        $"INSERT INTO {QueueTable} (JobId, JobType, JobJson, TimeEnqueued, Status) VALUES ('{jb.Item1.JobId.ToString()}', '{jb.Item1.JobTypeName}', '{jb.Item1.JobJson}', NOW(), '{JobStatus.Enqueued.Json()}');"
                        + Environment.NewLine +
                        jb.Item2.Select(x => $"INSERT INTO {RequirementsTable} (JobId, RequirementTag) VALUES ('{x.JobId.ToString()}', '{x.RequirementTag}');")
                                .Aggregate((a, b) => $"{a}{Environment.NewLine}{b}")
                        + Environment.NewLine +
                        "COMMIT;"
                        ;

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
