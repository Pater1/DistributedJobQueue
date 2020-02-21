using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Requirements
{
    public interface IRequirement
    {
        Task<bool> FullfillsAsync(IRequirement toComp);
    }
}
