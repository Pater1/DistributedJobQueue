using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedJobQueue.Job
{
    public enum JobStatus: byte
    {
        Enqueued = 1 << 0,
        InProcess = 1 << 1,
        Done = 1 << 2,
        WithReturnValue = 1 << 3,
        //128
        Error = 1 << 7
    }

    public static class JobStatusExtentions
    {
        public static IEnumerable<JobStatus> SplitFlags(this JobStatus flags) => ((JobStatus[])Enum.GetValues(typeof(JobStatus))).Where(x => (x & flags) > 0);
        public static JobStatus MergeFlags(this IEnumerable<JobStatus> flags) => flags.Aggregate((a, b) => a | b);

        public static string Json(this JobStatus flags) => JsonConvert.SerializeObject(flags.SplitFlags().Select(x => x.ToString().ToLowerInvariant()).ToArray());
        public static JobStatus ParseJson(string json) => (JsonConvert.DeserializeObject(json, typeof(string[])) as string[]).Select(x => FindCaseInsensitiveValue(x)).Cast<JobStatus>().MergeFlags();

        public static JobStatus FindCaseInsensitiveValue(string json) => ((JobStatus[])Enum.GetValues(typeof(JobStatus))).Where(x => x.ToString().ToLowerInvariant() == json).FirstOrDefault();
    }
}
