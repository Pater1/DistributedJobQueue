using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Requirements
{
    [System.Serializable]
    public readonly struct AnyRequirement : IRequirement, IEnumerable<IRequirement>
    {
        private IEnumerable<IRequirement> SubRequirements { get; }
        public AnyRequirement(IEnumerable<IRequirement> subRequirements)
        {
            SubRequirements = subRequirements;
        }
        public async Task<bool> FullfillsAsync(IRequirement toComp)
        {
            foreach(IRequirement requirement in SubRequirements)
            {
                if(await requirement.FullfillsAsync(toComp))
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerator<IRequirement> GetEnumerator()
        {
            return SubRequirements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SubRequirements.GetEnumerator();
        }
    }
}
