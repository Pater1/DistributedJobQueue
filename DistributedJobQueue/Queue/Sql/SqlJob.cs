using DistributedJobQueue.Job;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace DistributedJobQueue.Queue.Sql
{
    public readonly struct SqlJob: ISelfFactory<SqlJob, IDataRecord>
    {
        public Guid JobId { get; }
        public string JobTypeName { get; }
        public string JobJson { get; }

        public SqlJob(Guid jobId, string jobTypeName, string jobJson)
        {
            JobId = jobId;
            JobTypeName = jobTypeName;
            JobJson = jobJson;
        }

        public bool TryDeSqlize(out IJob ret)
        {
            string dec = HttpUtility.UrlDecode(JobJson);
            string tn = JobTypeName;
            Type t = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsInterface && typeof(IJob).IsAssignableFrom(p) && p.Name == tn)
                .FirstOrDefault();

            if(t == null)
            {
                ret = null;
                return false;
            }

            ret = JsonConvert.DeserializeObject(dec, t) as IJob;

            if (ret == null)
            {
                ret = null;
                return false;
            }
            if (ret.JobId != JobId)
            {
                ret = null;
                return false;
            }

            return true;
        }

        public SqlJob Build(IDataRecord source)
        {
            return new SqlJob(
                Guid.Parse(source.GetValue(source.GetOrdinal("JobId")).ToString()),
                source.GetValue(source.GetOrdinal("JobType")).ToString(),
                source.GetValue(source.GetOrdinal("JobJson")).ToString()
            );
        }
    }
}
