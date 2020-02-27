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
    public class AllRequirement: IEnumerableRequirement
    {
        public IEnumerable<IRequirement> SubRequirements { get; }

        public AllRequirement()
        {
            SubRequirements = new List<IRequirement>();
        }
        public AllRequirement(params IRequirement[] subRequirements)
        {
            SubRequirements = subRequirements.ToList();
        }
        public AllRequirement(IEnumerable<IRequirement> subRequirements)
        {
            SubRequirements = subRequirements.ToList();
        }
        public async Task<bool> FulfilledByAsync(IEnumerable<IFulfillment> fulfillment)
        {
            return (await Task.WhenAll(SubRequirements.Select(x => x.FulfilledByAsync(fulfillment)))).Aggregate((a,b) => a && b);
        }
    }
}
