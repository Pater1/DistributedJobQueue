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
