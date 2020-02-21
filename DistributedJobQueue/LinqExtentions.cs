using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedJobQueue
{
    public static class LinqExtentions
    {
        public static bool ContainsAll<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            foreach(T t in a)
            {
                if (!b.Contains(t))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
