using DistributedJobQueue.Fulfillments;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Requirements
{
    public readonly struct NoRequirement : IRequirement
    {
        public Task<bool> FulfilledByAsync(IEnumerable<IFulfillment> fulfillment) => Task.FromResult(true);
    }
}
