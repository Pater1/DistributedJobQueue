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

        public BasicJobClient(IJobQueue jobQueue, IEnumerable<IRequirement> requirements = null)
        {
            if(requirements != null)
            {
                _fulfilledRequirements = requirements.Select(x => (x, x.GetRequirementTags())).ToList();
            }
            JobQueue = jobQueue;
        }

        public IEnumerable<IRequirement> FulfilledRequirements => _fulfilledRequirements.Select(x => x.req);
        public IJobQueue JobQueue { get; }

        public bool RegisterFullfilledRequirement(IRequirement requirement)
        {
            string[] tags = requirement.GetRequirementTags();
            lock (_fulfilledRequirements)
            {
                if(_fulfilledRequirements.Where(x => !tags.ContainsAll(x.tags)).Any())
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

            (bool, Job.IJob) next = await JobQueue.TryDequeueAsync(any);
            if (next.Item1)
            {
                await JobQueue.TryEnqueueAllAsync(await next.Item2.Run());
            }
            return next.Item1;
        }
    }
}
