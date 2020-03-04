using DistributedJobQueue.Job;
using DistributedJobQueue.Job.Wrappers;
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
        public static readonly JsonSerializerSettings sqlJsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            #if DEBUG
                    Formatting = Formatting.Indented,
                    DefaultValueHandling = DefaultValueHandling.Include
            #else
                    Formatting = Formatting.None,
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            #endif
        };

        public static (SqlJob, SqlRequirement[]) Sqlize(this IJob job)
        {
            while(job is IJobWrapper)
            {
                job = (job as IJobWrapper).BaseJob;
            }

            Guid jbId = job.JobId;
            return (
                new SqlJob(
                    jbId,
                    job.GetType().Name,
                    JsonConvert.SerializeObject(job, sqlJsonSerializerSettings)
                ),
                job.ReadRequirement.GetRequirementTags().Select(x =>
                    new SqlRequirement(
                        jbId,
                        x
                    )
                ).ToArray()
            );
        }

        public static bool TryDeSqlize(this SqlJob sql, out IJob ret)
        {
            string tn = sql.JobTypeName;
            Type t = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsInterface && typeof(IJob).IsAssignableFrom(p) && p.Name == tn)
                .FirstOrDefault();

            if (t == null)
            {
                ret = null;
                return false;
            }

            ret = JsonConvert.DeserializeObject(sql.JobJson, t, sqlJsonSerializerSettings) as IJob;

            if (ret == null)
            {
                ret = null;
                return false;
            }
            if (ret.JobId != sql.JobId)
            {
                ret = null;
                return false;
            }

            return true;
        }
    }
}
