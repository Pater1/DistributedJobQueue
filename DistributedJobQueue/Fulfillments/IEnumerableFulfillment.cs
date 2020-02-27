using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Fulfillments
{
    public interface IEnumerableFulfillment: IFulfillment
    {
        public IEnumerable<IFulfillment> SubFulfillments { get; }
    }
}
