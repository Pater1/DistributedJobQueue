using DistributedJobQueue.Fulfillments;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Requirements
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class RequirementAttribute: Attribute, IRequirement
    {
        private IRequirement BackingRequirement { get; }
        public RequirementAttribute(string requirementJson)
        {
            try
            {
                BackingRequirement = Newtonsoft.Json.JsonConvert.DeserializeObject<IRequirement>(requirementJson);
            }
            catch
            {
                BackingRequirement = new BasicRequirement(requirementJson, true);
            }
        }
        public RequirementAttribute(IRequirement requirement)
        {
            BackingRequirement = requirement;
        }

        public Task<bool> FulfilledByAsync(IEnumerable<IFulfillment> fulfillment) => BackingRequirement.FulfilledByAsync(fulfillment);
    }
}
