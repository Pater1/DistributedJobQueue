using DistributedJobQueue.Requirements.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Threading.Tasks;

namespace DistributedJobQueue.Requirements
{
    public static class RequirementExtentions
    {
        private static Dictionary<Type, string[]> RequirementIdCache = new Dictionary<Type, string[]>();
        private static string[] fastEmpty = new string[0];
        public static async Task<bool> CheckFulfills(this IRequirement a, IRequirement b)
        {
            if (a == null || a is NoRequirement || a is NoRequirementAttribute)
            {
                return true;
            }
            if (b == null || b is NoRequirement || b is NoRequirementAttribute)
            {
                return false; //?
            }

            if (a is IEnumerableRequirement && b is IEnumerableRequirement)
            {
                (IRequirement a, IRequirement b, bool fullfilled)[] crossJoin = await Task.WhenAll(
                    (a as IEnumerableRequirement).SubRequirements.Cartesian((b as IEnumerableRequirement).SubRequirements, async (a,b) => (a, b, await a.CheckFulfills(b)))
                );

                return crossJoin.GroupBy(x => x.a)
                    .Select(x => x.Select(y => y.fullfilled).Aggregate((b as IEnumerableRequirement).Aggregate))
                    .Aggregate((a as IEnumerableRequirement).Aggregate);
            }
            else if(a is IEnumerableRequirement)
            {
                return (await Task.WhenAll(
                    (a as IEnumerableRequirement).SubRequirements.Select(async x => await x.CheckFulfills(b))
                )).Aggregate((a as IEnumerableRequirement).Aggregate);
            }
            else if(b is IEnumerableRequirement)
            {
                return (await Task.WhenAll(
                    (b as IEnumerableRequirement).SubRequirements.Select(async x => await a.CheckFulfills(x))
                )).Aggregate((b as IEnumerableRequirement).Aggregate);
            }
            else
            {
                return await a.FullfillsAsync(b);
            }
        }


        public static string[] GetRequirementTags(this IRequirement req)
        {
            if (req == null || req is NoRequirement || req is NoRequirementAttribute)
            {
                return fastEmpty;
            }

            if (req is BasicRequirement)
            {
                return new string[] { ((BasicRequirement)req).RequirementName };
            }

            if (req is IEnumerableRequirement)
            {
                return (req as IEnumerableRequirement).SubRequirements.SelectMany(x => x.GetRequirementTags()).Distinct().OrderBy(x => x).ToArray();
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
