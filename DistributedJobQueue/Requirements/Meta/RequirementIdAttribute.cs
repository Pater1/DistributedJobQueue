using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Requirements.Meta
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class RequirementIdAttribute: Attribute
    {
        public RequirementIdAttribute(string requirementId)
        {
            RequirementId = requirementId;
        }

        public string RequirementId { get; }
    }
}
