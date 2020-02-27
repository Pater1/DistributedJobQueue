using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Requirements
{
    public interface IEnumerableRequirement : IRequirement
    {
        public IEnumerable<IRequirement> SubRequirements { get; }
    }
}
