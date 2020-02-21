using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Job
{
    public interface IJob
    {
        Guid JobId { get; set; }
        IRequirement Requirement { get; set; }
        Task<IEnumerable<IJob>> Run();
    }
}
