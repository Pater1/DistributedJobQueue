using DistributedJobQueue.Requirements.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedJobQueue.Requirements
{
    public static class RequirementExtentions
    {
        private static Dictionary<Type, string[]> RequirementIdCache = new Dictionary<Type, string[]>();
        private static string[] fastEmpty = new string[0];
        public static string[] GetRequirementTags(this IRequirement req)
        {
            if(req == null || req is NoRequirement || req is NoRequirementAttribute)
            {
                return fastEmpty;
            }

            if(req is BasicRequirement)
            {
                return new string[] { ((BasicRequirement)req).RequirementName };
            }
            
            if(req is IEnumerable<IRequirement>)
            {
                return ((IEnumerable<IRequirement>)req).SelectMany(x => x.GetRequirementTags()).Distinct().OrderBy(x => x).ToArray();
            }

            Type t = req.GetType();
            string[] val;
            if (RequirementIdCache.TryGetValue(t, out val))
            {
                return val;
            }

            lock (RequirementIdCache)
            {
                if (RequirementIdCache.TryGetValue(t, out val))
                {
                    return val;
                }

                val = t.GetCustomAttributes(true).Where(x => x is RequirementIdAttribute).Select(x => x as RequirementIdAttribute).Select(x => x.RequirementId).Distinct().OrderBy(x => x).ToArray();
                RequirementIdCache.Add(t, val);
            }

            return val;
        }
    }
}
