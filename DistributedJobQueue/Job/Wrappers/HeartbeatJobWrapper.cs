using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedJobQueue.Job.Wrappers
{
    public class HeartbeatJobWrapper : IJobWrapper
    {
        public HeartbeatJobWrapper(IJob baseJob, TimeSpan heartbeatInterval, Func<Task> onHeartbeat)
        {
            BaseJob = baseJob;
            HeartbeatInterval = heartbeatInterval;
            OnHeartbeat = onHeartbeat;
        }

        public Guid JobId { get => BaseJob.JobId; set => BaseJob.JobId = value; }
        public IRequirement ReadRequirement { get => BaseJob.ReadRequirement; set => BaseJob.ReadRequirement = value; }
        
        public IJob BaseJob { get; set; }


        public TimeSpan HeartbeatInterval { get; set; }
        [JsonIgnore]
        public Func<Task> OnHeartbeat { get; set; }

        public async Task<IEnumerable<IJob>> Run()
        {
            bool done = false;
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                while (!done)
                {
                    await OnHeartbeat();
                    await Task.Delay(HeartbeatInterval);
                }
            });

            var ret = await BaseJob.Run();

            done = true;

            return ret;
        }
    }
}
