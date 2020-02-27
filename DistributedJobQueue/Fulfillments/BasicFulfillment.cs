using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Fulfillments
{
    public struct BasicFulfillment: IFulfillment
    {
        public string FulfillmentName { get; }
        public bool IgnoreCase { get; }
        public BasicFulfillment(string fulfillmentName, bool ignoreCase = true)
        {
            FulfillmentName = fulfillmentName;
            IgnoreCase = ignoreCase;
        }
    }
}