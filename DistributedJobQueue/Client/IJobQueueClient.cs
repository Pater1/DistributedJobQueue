using DistributedJobQueue.Fulfillments;
using DistributedJobQueue.Queue;
using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Client
{
    public interface IJobQueueClient
    {
        IJobQueue Queue { get; set; }
        //IEnumerable<IRequirement> Requirements { get; }
        //bool RegisterRequirement(IRequirement requirement);
        IEnumerable<IFulfillment> Fulfillments { get; }
        bool RegisterFulfillment(IFulfillment requirement);
        Task<bool> RunNextAsync();
    }
}
