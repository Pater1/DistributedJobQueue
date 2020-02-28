using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Job
{
    public interface IDeterministicIdJob: IJob
    {
        public Guid GenerateJobId();
    }
}
