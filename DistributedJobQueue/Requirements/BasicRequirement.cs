using DistributedJobQueue.Fulfillments;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Requirements
{
    [System.Serializable]
    public readonly struct BasicRequirement: IRequirement
    {
        public string RequirementName { get; }
        public bool IgnoreCase { get; }
        public BasicRequirement(string requirementName, bool ignoreCase = true)
        {
            RequirementName = requirementName;
            IgnoreCase = ignoreCase;
        }

        public Task<bool> FulfilledByAsync(IEnumerable<IFulfillment> fulfillment)
        {
            bool ret = false;
            foreach (IFulfillment ful in fulfillment)
            {
                if (ful is BasicFulfillment)
                {
                    BasicFulfillment basComp = (BasicFulfillment)ful;
                    if (IgnoreCase || basComp.IgnoreCase)
                    {
                        ret = RequirementName.ToLowerInvariant() == basComp.FulfillmentName.ToLowerInvariant();
                    }
                    else
                    {
                        ret = RequirementName == basComp.FulfillmentName;
                    }
                }
                if (ret)
                {
                    break;
                }
            }
            return Task.FromResult(ret);
        }

        //public Task<bool> FullfillsAsync(IRequirement toComp)
        //{
        //    bool ret = false;
        //    if(toComp is BasicRequirement)
        //    {
        //        BasicRequirement basComp = (BasicRequirement)toComp;
        //        if(IgnoreCase || basComp.IgnoreCase)
        //        {
        //            ret = RequirementName.ToLowerInvariant() == basComp.RequirementName.ToLowerInvariant();
        //        }
        //        else
        //        {
        //            ret = RequirementName == basComp.RequirementName;
        //        }
        //    }
        //    return Task.FromResult(ret);
        //}
    }
}
