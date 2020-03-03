using Dasync.Collections;
using DistributedJobQueue.Fulfillments;
using DistributedJobQueue.Job;
using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedJobQueue.Queue
{
    public class LocalJobQueue : IJobQueue
    {
        private List<(string[] reqTags, IJob job)> sudoQueue = new List<(string[] reqTags, IJob job)>();
        private SemaphoreSlim locker = new SemaphoreSlim(1, 1);
        private List<Guid> InProcess = new List<Guid>();
        public async Task<(bool, IJob)> TryDequeueAsync(IEnumerable<IFulfillment> fulfillments = null)
        {
            string[] fulTags = fulfillments.GetFulfillmentTags();
            IJob job = null;
            int index = -1;

            await locker.WaitAsync();

            foreach ((IJob jb, int i) jbi in sudoQueue.Select((x,i) => (x,i)).Where(x => x.Item1.reqTags.ContainsAll(fulTags)).Select(x => (x.Item1.job, x.Item2)))
            {
                if (await (jbi.jb.ReadRequirement ?? new NoRequirement()).FulfilledByAsync(fulfillments))
                {
                    job = jbi.jb;
                    index = jbi.i;
                    break;
                }
            }
            if(job != null)
            {
                sudoQueue.RemoveAt(index);
                InProcess.Add(job.JobId);
            }

            locker.Release();

            return (job != null, job);
        }

        public async Task<bool> TryEnqueueAsync(IJob job)
        {
            bool alreadyContains;

            if(job.JobId == default)
            {
                job.JobId = Guid.NewGuid();
            }

            await locker.WaitAsync();

            alreadyContains = sudoQueue.Select(x => x.job.JobId).Contains(job.JobId);
            if (!alreadyContains)
            {
                sudoQueue.Add(((job.ReadRequirement ?? new NoRequirement()).GetRequirementTags(), job));
            }

            locker.Release();

            return !alreadyContains;
        }

        public async Task<(bool, object)> WaitForReturnValueAsync(Guid awaitingJobId, Guid jobToAwaitId)
        {
            if (awaitingJobId == jobToAwaitId)
            {
                throw new ArgumentException($"A job cannot await itself. jobId: {awaitingJobId.ToString()}");
            }

            while (InProcess.Contains(jobToAwaitId))
            {
                await Task.Delay(10);
            }

            return (true, null);
        }
    }
}
