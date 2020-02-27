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
        public List<IRequirement> SubRequirements { get; }

        IEnumerable<IRequirement> IEnumerableRequirement.SubRequirements => SubRequirements;

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
        public Task<bool> FullfillsAsync(IRequirement toComp)
        {
            return this.CheckFulfills(toComp);
        }

        public bool Aggregate(bool a, bool b)
        {
            return a && b;
        }
    }
}
