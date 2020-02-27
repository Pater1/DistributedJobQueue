using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Job
{
    public struct ReturnValueJob<T> : IJob
    {
        public Guid JobId { get; set; }
        public Guid ReturnJobId { get; set; }
        public IRequirement Requirement { get; set; }
        public T ReturnValue { get; set; }

        public ReturnValueJob(Guid jobId, IRequirement requirement, T returnValue)
        {
            JobId = new Guid();
            ReturnJobId = jobId;
            Requirement = requirement == null? new NoRequirement(): requirement;
            ReturnValue = returnValue;
        }

        private static readonly IEnumerable<IJob> fastEmpty = new IJob[0];
        public Task<IEnumerable<IJob>> Run()
        {
            return Task.FromResult(fastEmpty);
        }
    }
}
