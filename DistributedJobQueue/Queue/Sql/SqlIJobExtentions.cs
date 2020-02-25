using DistributedJobQueue.Job;
using DistributedJobQueue.Requirements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DistributedJobQueue.Queue.Sql
{
    public static class SqlIJobExtentions
    {
        public static (SqlJob, SqlRequirement[]) Sqlize(this IJob job)
        {
            Guid jbId = job.JobId;
            return (
                new SqlJob(
                    jbId,
                    job.GetType().Name,
                    HttpUtility.UrlEncode(JsonConvert.SerializeObject(job))
                ),
                job.Requirement.GetRequirementTags().Select(x =>
                    new SqlRequirement(
                        jbId,
                        x
                    )
                ).ToArray()
            );
        }
    }
}
