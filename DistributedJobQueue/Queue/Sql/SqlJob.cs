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
    public struct SqlJob: ISelfFactory<SqlJob, IDataRecord>
    {
        public Guid JobId { get; }
        public string JobTypeName { get; }
        public string JobJson { get; }
        public DateTime TimeEnqueued { get; set; }
        public DateTime? LastHeartbeat { get; set; }
        public JobStatus Status { get; set; }
        public string StatusJson {
            get
            {
                return Status.Json();
            }
            set
            {
                Status = JobStatusExtentions.ParseJson(value);
            }
        }
        public string ReturnJson { get; set; }

        public SqlJob(Guid jobId, string jobTypeName, string jobJson): this()
        {
            JobId = jobId;
            JobTypeName = jobTypeName;
            JobJson = jobJson;
            Status = JobStatus.Enqueued;
        }

        public SqlJob Build(IDataRecord source)
        {
            SqlJob jb = new SqlJob(
                Guid.Parse(source.GetValue(source.GetOrdinal("JobId")).ToString()),
                source.GetValue(source.GetOrdinal("JobType")).ToString(),
                source.GetValue(source.GetOrdinal("JobJson")).ToString()
            );

            jb.TimeEnqueued = DateTime.Parse(source.GetValue(source.GetOrdinal("TimeEnqueued")).ToString());
            if(DateTime.TryParse(source.GetValue(source.GetOrdinal("LastHeartbeat"))?.ToString(), out DateTime dt))
            {
                jb.LastHeartbeat = dt;
            }
            else
            {
                jb.LastHeartbeat = null;
            }
            jb.StatusJson = source.GetValue(source.GetOrdinal("Status")).ToString();
            jb.ReturnJson = source.GetValue(source.GetOrdinal("ReturnJson")).ToString();

            return jb;
        }
    }
}
