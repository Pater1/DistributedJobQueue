using DistributedJobQueue.Fulfillments;
using DistributedJobQueue.Job;
using DistributedJobQueue.Queue;
using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedJobQueue.Client
{
    public class FireAndForgetJobClient : IJobQueueClient
    {
        public List<(IFulfillment req, string[] tags)> _fulfillments = new List<(IFulfillment req, string[] tags)>();

        public FireAndForgetJobClient() { }
        public FireAndForgetJobClient(IJobQueue jobQueue, IEnumerable<IFulfillment> fulfillments = null)
        {
            if (fulfillments != null)
            {
                _fulfillments = fulfillments.Select(x => (x, x.GetFulfillmentTags())).ToList();
            }
            Queue = jobQueue;
        }
        public bool RegisterFulfillment(IFulfillment fulfillment)
        {
            string[] tags = fulfillment.GetFulfillmentTags();
            lock (_fulfillments)
            {
                if (!_fulfillments.Any() || _fulfillments.Where(x => !tags.ContainsAll(x.tags)).Any())
                {
                    _fulfillments.Add((fulfillment, tags));
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<IFulfillment> Fulfillments => _fulfillments.Select(x => x.req);
        public IJobQueue Queue { get; set; }


        public int ActiveJobs { get; private set; } = 0;
        public async Task<bool> RunNextAsync()
        {
            (bool, Job.IJob) next = await Queue.TryDequeueAsync(Fulfillments);
            if (next.Item1)
            {
                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    ActiveJobs++;
                    try
                    {
                        IEnumerable<IJob> requeue = await next.Item2.Run();
                        await Queue.TryEnqueueAllAsync(requeue);
                    }
                    finally
                    {
                        ActiveJobs--;
                    }
                });
            }
            return next.Item1;
        }
    }
}
