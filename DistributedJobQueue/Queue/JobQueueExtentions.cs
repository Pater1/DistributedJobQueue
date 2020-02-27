using DistributedJobQueue.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedJobQueue.Queue
{
    public static class JobQueueExtentions
    {
        public static async Task<bool> TryEnqueueAllAsync(this IJobQueue queue, IEnumerable<IJob> jobs)
        {
            return !(await Task.WhenAll(
                jobs.Select(job => queue.TryEnqueueAsync(job))
            )).Where(x => !x).Any();
        }

        public static Task<bool> WaitForCompletionAsync(this IJobQueue queue, IJob job) => queue.WaitForCompletionAsync(job.JobId);
        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, IEnumerable<Guid> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(x)));
        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, IEnumerable<IJob> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(x)));
        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, params Guid[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(x)));
        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, params IJob[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(x)));
    }
}
