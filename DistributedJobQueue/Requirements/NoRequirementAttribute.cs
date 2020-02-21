using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Requirements
{
    public class NoRequirementAttribute : RequirementAttribute
    {
        public NoRequirementAttribute() : base(new NoRequirement()) { }
    }
}
