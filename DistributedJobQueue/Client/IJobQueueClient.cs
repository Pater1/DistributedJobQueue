using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Client
{
    public interface IJobQueueClient
    {
        IEnumerable<IRequirement> FulfilledRequirements { get; }
        bool RegisterFullfilledRequirement(IRequirement requirement);
        Task<bool> RunNextAsync();
    }
}
