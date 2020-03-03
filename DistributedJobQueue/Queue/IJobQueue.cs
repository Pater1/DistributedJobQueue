using DistributedJobQueue.Fulfillments;
using DistributedJobQueue.Job;
using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Queue
{
    public interface IJobQueue
    {
        Task<bool> TryEnqueueAsync(IJob job);
        Task<(bool, object)> WaitForReturnValueAsync(Guid awaitingJobId, Guid jobToAwaitId);
        Task<(bool, IJob)> TryDequeueAsync(IEnumerable<IFulfillment> requirementsFulfillable = null);
    }
}
