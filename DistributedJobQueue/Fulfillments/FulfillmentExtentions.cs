using DistributedJobQueue.Fulfillments.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedJobQueue.Fulfillments
{
    public static class FulfillmentExtentions
    {
        private static Dictionary<Type, string[]> FulfillmentIdCache = new Dictionary<Type, string[]>();
        private static string[] fastEmpty = new string[0];
      
        public static string[] GetFulfillmentTags(this IFulfillment ful)
        {
            if (ful == null /*|| req is NoFulfillment || req is NoFulfillmentAttribute*/)
            {
                return fastEmpty;
            }

            if (ful is BasicFulfillment)
            {
                return new string[] { ((BasicFulfillment)ful).FulfillmentName };
            }

            if (ful is IEnumerableFulfillment)
            {
                return (ful as IEnumerableFulfillment).SubFulfillments.SelectMany(x => x.GetFulfillmentTags()).Distinct().OrderBy(x => x).ToArray();
            }

            Type t = ful.GetType();
            string[] val;
            if (FulfillmentIdCache.TryGetValue(t, out val))
            {
                return val;
            }

            lock (FulfillmentIdCache)
            {
                if (FulfillmentIdCache.TryGetValue(t, out val))
                {
                    return val;
                }

                val = t.GetCustomAttributes(true).Where(x => x is FulfillmentIdAttribute).Select(x => x as FulfillmentIdAttribute).Select(x => x.FulfillmentId).Distinct().OrderBy(x => x).ToArray();
                FulfillmentIdCache.Add(t, val);
            }

            return val;
        }

        public static string[] GetFulfillmentTags(this IEnumerable<IFulfillment> ful) => ful.SelectMany(x => x.GetFulfillmentTags()).Distinct().ToArray();
    }
}
