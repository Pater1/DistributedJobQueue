using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedJobQueue.Queue.Sql
{
    public interface ISelfFactory<TSelf, TSource>
    {
        public TSelf Build(TSource source);
    }
}
