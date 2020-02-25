using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DistributedJobQueue.Queue.Sql
{
    public readonly struct SqlRequirement : ISelfFactory<SqlRequirement, IDataRecord>
    {
        public SqlRequirement(Guid jobId, string requirementTag)
        {
            JobId = jobId;
            RequirementTag = requirementTag;
        }

        public Guid JobId { get; }
        public string RequirementTag { get; }

        public SqlRequirement Build(IDataRecord source)
        {
            return new SqlRequirement(
                Guid.Parse(source.GetValue(source.GetOrdinal("JobId")).ToString()),
                source.GetValue(source.GetOrdinal("RequirementTag")).ToString()
            );
        }
    }
}
