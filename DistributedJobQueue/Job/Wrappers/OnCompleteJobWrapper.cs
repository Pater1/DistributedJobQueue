using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DistributedJobQueue.Job.Wrappers
{
    public class OnCompleteJobWrapper : IJobWrapper
    {
        public OnCompleteJobWrapper(IJob baseJob, Func<Task> onCompletion)
        {
            BaseJob = baseJob;
            OnCompletion = onCompletion;
        }

        public Guid JobId { get => BaseJob.JobId; set => BaseJob.JobId = value; }
        public IRequirement ReadRequirement { get => BaseJob.ReadRequirement; set => BaseJob.ReadRequirement = value; }

        public IJob BaseJob { get; set; }

        [JsonIgnore]
        public Func<Task> OnCompletion { get; set; }

        public async Task<IEnumerable<IJob>> Run()
        {
            var ret = await BaseJob.Run();

            await OnCompletion();

            return ret;
        }
    }
}
