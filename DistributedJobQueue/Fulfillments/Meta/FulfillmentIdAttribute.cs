using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Fulfillments.Meta
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class FulfillmentIdAttribute : Attribute
    {
        public FulfillmentIdAttribute(string fulfillmentId)
        {
            FulfillmentId = fulfillmentId;
        }

        public string FulfillmentId { get; }
    }
}
