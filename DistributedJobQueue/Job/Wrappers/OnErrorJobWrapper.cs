using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DistributedJobQueue.Job.Wrappers
{
    class OnErrorJobWrapper : IJobWrapper
    {
        public OnErrorJobWrapper(IJob baseJob, Func<Exception, Task> onError, bool rethrow = false)
        {
            BaseJob = baseJob;
            OnError = onError;
            Rethrow = rethrow;
        }

        public Guid JobId { get => BaseJob.JobId; set => BaseJob.JobId = value; }
        public IRequirement Requirement { get => BaseJob.Requirement; set => BaseJob.Requirement = value; }

        public IJob BaseJob { get; set; }
        public bool Rethrow { get; set; }

        [JsonIgnore]
        public Func<Exception, Task> OnError { get; set; }

        public async Task<IEnumerable<IJob>> Run()
        {
            try
            {
                var ret = await BaseJob.Run();
                return ret;
            }
            catch (Exception e)
            {
                await OnError(e);
                if (Rethrow)
                {
                    throw e;
                }
                else
                {
                    return new IJob[0];
                }
            }
        }
    }
}
