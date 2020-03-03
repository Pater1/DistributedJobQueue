using DistributedJobQueue.Client;
using DistributedJobQueue.Queue;
using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Job
{
    public struct SerialJob : IJob
    {
        public SerialJob(IRequirement requirement, IEnumerable<IJob> serJobs)
        {
            this.JobId = default;
            this.Requirement = requirement;
            this.SerJobs = serJobs;
        }

        public Guid JobId { get; set; }
        public IRequirement Requirement { get; set; }
        public IEnumerable<IJob> SerJobs { get; set; }

        public async Task<IEnumerable<IJob>> Run()
        {
            IJobQueue jobQueue = this.GetDispatchingQueue();

            foreach(IJob job in SerJobs)
            {
                if(job.JobId == default)
                {
                    job.JobId = Guid.NewGuid();
                }
                await jobQueue.TryEnqueueAsync(job);
                await jobQueue.WaitForCompletionAsync(this, job);
            }

            return new IJob[0];
        }
    }
}
