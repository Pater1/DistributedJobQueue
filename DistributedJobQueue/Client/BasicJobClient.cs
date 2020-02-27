using DistributedJobQueue.Job;
using DistributedJobQueue.Queue;
using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Client
{
    public class BasicJobClient : IJobQueueClient
    {
        public List<(IRequirement req, string[] tags)> _fulfilledRequirements = new List<(IRequirement req, string[] tags)>();

        public BasicJobClient() { }
        public BasicJobClient(IJobQueue jobQueue, IEnumerable<IRequirement> fulfilledRequirements = null)
        {
            if(fulfilledRequirements != null)
            {
                _fulfilledRequirements = fulfilledRequirements.Select(x => (x, x.GetRequirementTags())).ToList();
            }
            Queue = jobQueue;
        }

        public IEnumerable<IRequirement> FulfilledRequirements => _fulfilledRequirements.Select(x => x.req);
        public IJobQueue Queue { get; set; }

        public bool RegisterFullfilledRequirement(IRequirement requirement)
        {
            string[] tags = requirement.GetRequirementTags();
            lock (_fulfilledRequirements)
            {
                if(!_fulfilledRequirements.Any() || _fulfilledRequirements.Where(x => !tags.ContainsAll(x.tags)).Any())
                {
                    _fulfilledRequirements.Add((requirement, tags));
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> RunNextAsync()
        {
            AnyRequirement any = new AnyRequirement(FulfilledRequirements);

            (bool, Job.IJob) next = await Queue.TryDequeueAsync(any);
            if (next.Item1)
            {
                IEnumerable<IJob> requeue = await next.Item2.Run();
                await Queue.TryEnqueueAllAsync(requeue);
            }
            return next.Item1;
        }
    }
}
