using Dasync.Collections;
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
        public async Task<(bool, IJob)> TryDequeueAsync(IRequirement requirementsFulfillable = null)
        {
            if (requirementsFulfillable == null) requirementsFulfillable = new NoRequirement();

            string[] reqTags = requirementsFulfillable.GetRequirementTags();
            IJob job = null;
            int index = -1;

            await locker.WaitAsync();

            foreach ((IJob jb, int i) jbi in sudoQueue.Select((x,i) => (x,i)).Where(x => x.Item1.reqTags.ContainsAll(reqTags)).Select(x => (x.Item1.job, x.Item2)))
            {
                if (await (jbi.jb.Requirement ?? new NoRequirement()).FullfillsAsync(requirementsFulfillable))
                {
                    job = jbi.jb;
                    index = jbi.i;
                    break;
                }
            }
            if(job != null)
            {
                sudoQueue.RemoveAt(index);
            }

            locker.Release();

            return (job != null, job);
        }

        public async Task<bool> TryEnqueueAsync(IJob job)
        {
            bool alreadyContains;

            await locker.WaitAsync();

            alreadyContains = sudoQueue.Select(x => x.job.JobId).Contains(job.JobId);
            if (!alreadyContains)
            {
                sudoQueue.Add(((job.Requirement ?? new NoRequirement()).GetRequirementTags(), job));
            }

            locker.Release();

            return !alreadyContains;
        }
    }
}
