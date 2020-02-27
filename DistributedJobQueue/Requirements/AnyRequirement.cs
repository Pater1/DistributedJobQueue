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
        public List<IRequirement> SubRequirements { get; }

        IEnumerable<IRequirement> IEnumerableRequirement.SubRequirements => SubRequirements;

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
        public Task<bool> FullfillsAsync(IRequirement toComp)
        {
            return this.CheckFulfills(toComp);
        }

        public bool Aggregate(bool a, bool b)
        {
            return a || b;
        }
    }
}
