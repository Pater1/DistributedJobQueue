using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Requirements
{
    public interface IEnumerableRequirement : IRequirement
    {
        public bool Aggregate(bool a, bool b);
        public IEnumerable<IRequirement> SubRequirements { get; }
    }
}
