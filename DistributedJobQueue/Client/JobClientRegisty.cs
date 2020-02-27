using DistributedJobQueue.Job;
using DistributedJobQueue.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.Client
{
    public static class JobClientRegisty
    {
        private static Dictionary<IJobQueue, IJobQueueClient> QueueClientReverseLookup { get; set; } = new Dictionary<IJobQueue, IJobQueueClient>();
        private static Dictionary<Guid, IJobQueue> JobIdQueueReverseLookup { get; set; } = new Dictionary<Guid, IJobQueue>();

        public static void UnregisterJobDispatch(this IJobQueue queue, IJob job)
        {
            lock (JobIdQueueReverseLookup)
            {
                if (JobIdQueueReverseLookup.ContainsKey(job.JobId))
                {
                    JobIdQueueReverseLookup.Remove(job.JobId);
                }
            }
        }
        public static void UnregisterQueue(this IJobQueueClient client, IJobQueue queue)
        {
            lock (QueueClientReverseLookup)
            {
                if (QueueClientReverseLookup.ContainsKey(queue))
                {
                    QueueClientReverseLookup.Remove(queue);
                }
            }
        }

        public static void RegisterJobDispatch(this IJobQueue queue, IJob job)
        {
            lock (JobIdQueueReverseLookup)
            {
                if (JobIdQueueReverseLookup.ContainsKey(job.JobId))
                {
                    JobIdQueueReverseLookup[job.JobId] = queue;
                }
                else
                {
                    JobIdQueueReverseLookup.Add(job.JobId, queue);
                }
            }
        }
        public static void RegisterQueue(this IJobQueueClient client, IJobQueue queue)
        {
            lock (QueueClientReverseLookup)
            {
                if (QueueClientReverseLookup.ContainsKey(queue))
                {
                    QueueClientReverseLookup[queue] = client;
                }
                else
                {
                    QueueClientReverseLookup.Add(queue, client);
                }
            }
        }

        public static IJobQueue GetDispatchingQueue(this IJob job)
        {
            lock (JobIdQueueReverseLookup)
            {
                if (JobIdQueueReverseLookup.TryGetValue(job.JobId, out IJobQueue queue))
                {
                    return queue;
                }
            }
            return null;
        }
        public static IJobQueueClient GetDispatchingClient(this IJobQueue queue)
        {
            lock (QueueClientReverseLookup)
            {
                if (QueueClientReverseLookup.TryGetValue(queue, out IJobQueueClient client))
                {
                    return client;
                }
            }
            return null;
        }
        public static IJobQueueClient GetDispatchingClient(this IJob job) => job.GetDispatchingQueue()?.GetDispatchingClient();
    }
}
