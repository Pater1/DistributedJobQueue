using DistributedJobQueue.Client;
using DistributedJobQueue.Queue;
using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Job
{
    public struct ParallelJob : IJob
    {
        public ParallelJob(IEnumerable<IJob> parJobs)
        {
            this.JobId = default;
            this.ReadRequirement = new NoRequirement();
            this.ParJobs = parJobs;
        }
        public ParallelJob(params IJob[] parJobs)
        {
            this.JobId = default;
            this.ReadRequirement = new NoRequirement();
            this.ParJobs = parJobs;
        }

        public Guid JobId { get; set; }
        public IRequirement ReadRequirement { get; set; }
        public IEnumerable<IJob> ParJobs { get; set; }

        public async Task<IEnumerable<IJob>> Run()
        {
            Guid thId = this.JobId;
            IJobQueue jobQueue = this.GetDispatchingQueue();

            await Task.WhenAll(ParJobs.Select(x =>
                {
                    IJob job = x;
                    if (job.JobId == default)
                    {
                        job.JobId = Guid.NewGuid();
                    }
                    return job;
                })
                .Select(async x =>
                {
                    await jobQueue.TryEnqueueAsync(x);
                    await jobQueue.WaitForCompletionAsync(thId, x);
                })
            );

            return new IJob[0];
        }
    }
}
