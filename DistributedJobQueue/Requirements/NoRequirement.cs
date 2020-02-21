using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Requirements
{
    public readonly struct NoRequirement : IRequirement
    {
        public Task<bool> FullfillsAsync(IRequirement toComp) => Task.FromResult(true);
    }
}
