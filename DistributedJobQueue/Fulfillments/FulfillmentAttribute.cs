using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Fulfillments
{
    public class FulfillmentAttribute: Attribute, IFulfillment
    {
        private IFulfillment BackingRequirement { get; }
        public FulfillmentAttribute(string requirementJson)
        {
            try
            {
                BackingRequirement = Newtonsoft.Json.JsonConvert.DeserializeObject<IFulfillment>(requirementJson);
            }
            catch
            {
                BackingRequirement = new BasicFulfillment(requirementJson, true);
            }
        }
        public FulfillmentAttribute(IFulfillment requirement)
        {
            BackingRequirement = requirement;
        }
    }
}
