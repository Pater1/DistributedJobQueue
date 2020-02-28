using DistributedJobQueue.Misc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Job
{
    public static class JobExtentions
    {
        public static Guid GenerateIdFromJsonHash(this IDeterministicIdJob detjob)
        {
            Guid cache = detjob.JobId;
            detjob.JobId = default;

            string json = JsonConvert.SerializeObject(detjob);
            detjob.JobId = cache;

            Guid ret = json.HashTo<Guid>();
            return ret;
        }
    }
}
