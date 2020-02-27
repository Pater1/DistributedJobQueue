using DistributedJobQueue.Fulfillments;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Requirements
{
    [System.Serializable]
    public class AnyRequirement : IEnumerableRequirement
    {
        public IEnumerable<IRequirement> SubRequirements { get; }

        public AnyRequirement()
        {
            SubRequirements = new List<IRequirement>();
        }
        public AnyRequirement(params IRequirement[] subRequirements)
        {
            SubRequirements = subRequirements.ToList();
        }
        public AnyRequirement(IEnumerable<IRequirement> subRequirements)
        {
            SubRequirements = subRequirements.ToList();
        }
        public async Task<bool> FulfilledByAsync(IEnumerable<IFulfillment> fulfillment)
        {
            return (await Task.WhenAll(SubRequirements.Select(x => x.FulfilledByAsync(fulfillment)))).Aggregate((a, b) => a || b);
        }
    }
}
