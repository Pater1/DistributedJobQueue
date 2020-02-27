using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Job.Wrappers
{
    public interface IJobWrapper: IJob
    {
        public IJob BaseJob { get; }
    }
}
