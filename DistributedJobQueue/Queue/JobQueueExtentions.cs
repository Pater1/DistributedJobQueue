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

        public static async Task<bool> EnqueueAndWaitForCompletionAsync(this IJobQueue queue, IJob jobWaiting, IJob jobToWaitFor)
        {
            if(jobToWaitFor.JobId == default)
            {
                jobToWaitFor.JobId = Guid.NewGuid();
            }
            return await queue.TryEnqueueAsync(jobToWaitFor) 
                && await queue.WaitForCompletionAsync(jobWaiting.JobId, jobToWaitFor.JobId);
        }

        public static Task<bool> WaitForCompletionAsync(this IJobQueue queue, IJob jobWaiting, IJob jobToWaitFor) => queue.WaitForCompletionAsync(jobWaiting.JobId, jobToWaitFor.JobId);
        public static Task<bool> WaitForCompletionAsync(this IJobQueue queue, IJob jobWaiting, Guid jobToWaitForId) => queue.WaitForCompletionAsync(jobWaiting.JobId, jobToWaitForId);
        public static Task<bool> WaitForCompletionAsync(this IJobQueue queue, Guid jobWaitingId, IJob jobToWaitFor) => queue.WaitForCompletionAsync(jobWaitingId, jobToWaitFor.JobId);

        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, IJob jobWaiting, IEnumerable<IJob> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(jobWaiting, x)));
        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, IJob jobWaiting, IEnumerable<Guid> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(jobWaiting.JobId, x)));
        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, Guid jobWaitingId, IEnumerable<IJob> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(jobWaitingId, x.JobId)));
        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, Guid jobWaitingId, IEnumerable<Guid> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(jobWaitingId, x)));

        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, IJob jobWaiting, params IJob[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(jobWaiting, x)));
        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, IJob jobWaiting, params Guid[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(jobWaiting, x)));
        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, Guid jobWaitingId, params IJob[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(jobWaitingId, x)));
        public static Task<bool[]> WaitForCompletionAsync(this IJobQueue queue, Guid jobWaitingId, params Guid[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForCompletionAsync(jobWaitingId, x)));

        public static async Task<bool> WaitForCompletionAsync(this IJobQueue queue, Guid awaitingJobId, Guid jobToAwaitId)
        {
            (bool, object) returnValue = await queue.WaitForReturnValueAsync(awaitingJobId, jobToAwaitId);
            return returnValue.Item1;
        }

        public static Task<(bool, T)> WaitForReturnValueAsync<T>(this IJobQueue queue, IJob jobWaiting, IJob jobToWaitFor) => queue.WaitForReturnValueAsync<T>(jobWaiting.JobId, jobToWaitFor.JobId);
        public static Task<(bool, T)> WaitForReturnValueAsync<T>(this IJobQueue queue, IJob jobWaiting, Guid jobToWaitForId) => queue.WaitForReturnValueAsync<T>(jobWaiting.JobId, jobToWaitForId);
        public static Task<(bool, T)> WaitForReturnValueAsync<T>(this IJobQueue queue, Guid jobWaitingId, IJob jobToWaitFor) => queue.WaitForReturnValueAsync<T>(jobWaitingId, jobToWaitFor.JobId);

        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, IJob jobWaiting, IEnumerable<IJob> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaiting, x)));
        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, IJob jobWaiting, IEnumerable<Guid> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaiting.JobId, x)));
        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, Guid jobWaitingId, IEnumerable<IJob> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaitingId, x.JobId)));
        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, Guid jobWaitingId, IEnumerable<Guid> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaitingId, x)));

        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, IJob jobWaiting, params IJob[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaiting, x)));
        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, IJob jobWaiting, params Guid[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaiting, x)));
        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, Guid jobWaitingId, params IJob[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaitingId, x)));
        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, Guid jobWaitingId, params Guid[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaitingId, x)));

        public static async Task<(bool, T)> WaitForReturnValueAsync<T>(this IJobQueue queue, Guid awaitingJobId, Guid jobToAwaitId)
        {
            return await queue.WaitForReturnValueAsync<T>(awaitingJobId, jobToAwaitId, x => x == null ? default : (T)x);
        }

        public static Task<(bool, T)> WaitForReturnValueAsync<T>(this IJobQueue queue, IJob jobWaiting, IJob jobToWaitFor, Func<object, T> converter) => queue.WaitForReturnValueAsync<T>(jobWaiting.JobId, jobToWaitFor.JobId, converter);
        public static Task<(bool, T)> WaitForReturnValueAsync<T>(this IJobQueue queue, IJob jobWaiting, Guid jobToWaitForId, Func<object, T> converter) => queue.WaitForReturnValueAsync<T>(jobWaiting.JobId, jobToWaitForId, converter);
        public static Task<(bool, T)> WaitForReturnValueAsync<T>(this IJobQueue queue, Guid jobWaitingId, IJob jobToWaitFor, Func<object, T> converter) => queue.WaitForReturnValueAsync<T>(jobWaitingId, jobToWaitFor.JobId, converter);

        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, IJob jobWaiting, IEnumerable<IJob> jobIds, Func<object, T> converter) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaiting, x, converter)));
        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, IJob jobWaiting, IEnumerable<Guid> jobIds, Func<object, T> converter) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaiting.JobId, x, converter)));
        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, Guid jobWaitingId, IEnumerable<IJob> jobIds, Func<object, T> converter) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaitingId, x.JobId, converter)));
        public static Task<(bool, T)[]> WaitForReturnValueAsync<T>(this IJobQueue queue, Guid jobWaitingId, IEnumerable<Guid> jobIds, Func<object, T> converter) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync<T>(jobWaitingId, x, converter)));

        public static async Task<(bool, T)> WaitForReturnValueAsync<T>(this IJobQueue queue, Guid awaitingJobId, Guid jobToAwaitId, Func<object, T> converter)
        {
            (bool, object) returnValue = await queue.WaitForReturnValueAsync(awaitingJobId, jobToAwaitId);
            return (returnValue.Item1, converter(returnValue.Item2));
        }

        public static Task<(bool, object)> WaitForReturnValueAsync(this IJobQueue queue, IJob jobWaiting, IJob jobToWaitFor) => queue.WaitForReturnValueAsync(jobWaiting.JobId, jobToWaitFor.JobId);
        public static Task<(bool, object)> WaitForReturnValueAsync(this IJobQueue queue, IJob jobWaiting, Guid jobToWaitForId) => queue.WaitForReturnValueAsync(jobWaiting.JobId, jobToWaitForId);
        public static Task<(bool, object)> WaitForReturnValueAsync(this IJobQueue queue, Guid jobWaitingId, IJob jobToWaitFor) => queue.WaitForReturnValueAsync(jobWaitingId, jobToWaitFor.JobId);

        public static Task<(bool, object)[]> WaitForReturnValueAsync(this IJobQueue queue, IJob jobWaiting, IEnumerable<IJob> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync(jobWaiting, x)));
        public static Task<(bool, object)[]> WaitForReturnValueAsync(this IJobQueue queue, IJob jobWaiting, IEnumerable<Guid> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync(jobWaiting.JobId, x)));
        public static Task<(bool, object)[]> WaitForReturnValueAsync(this IJobQueue queue, Guid jobWaitingId, IEnumerable<IJob> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync(jobWaitingId, x.JobId)));
        public static Task<(bool, object)[]> WaitForReturnValueAsync(this IJobQueue queue, Guid jobWaitingId, IEnumerable<Guid> jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync(jobWaitingId, x)));

        public static Task<(bool, object)[]> WaitForReturnValueAsync(this IJobQueue queue, IJob jobWaiting, params IJob[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync(jobWaiting, x)));
        public static Task<(bool, object)[]> WaitForReturnValueAsync(this IJobQueue queue, IJob jobWaiting, params Guid[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync(jobWaiting, x)));
        public static Task<(bool, object)[]> WaitForReturnValueAsync(this IJobQueue queue, Guid jobWaitingId, params IJob[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync(jobWaitingId, x)));
        public static Task<(bool, object)[]> WaitForReturnValueAsync(this IJobQueue queue, Guid jobWaitingId, params Guid[] jobIds) => Task.WhenAll(jobIds.Select(x => queue.WaitForReturnValueAsync(jobWaitingId, x)));

    }
}
