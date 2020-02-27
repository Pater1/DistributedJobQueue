using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DistributedJobQueue.Client;
using DistributedJobQueue.Queue;
using DistributedJobQueue.Requirements;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedJobQueue.ASP
{
    public static class IServiceCollectionExtentions
    {
        //TODO: refactor to proper factory pattern
        public static IServiceCollection AddJobClient<T>(this IServiceCollection services, IJobQueue queue, IEnumerable<IRequirement> requirements) where T: IJobQueueClient, new()
        {
            services.AddSingleton<IJobQueue>(queue);

            IJobQueueClient client = new T();
            client.Queue = queue;
            foreach(IRequirement req in requirements)
            {
                client.RegisterFullfilledRequirement(req);
            }
            services.AddSingleton<IJobQueueClient>(client);

            ThreadPool.QueueUserWorkItem(async (_) => await client.StartDeamonAsync(true));

            return services;
        }
    }
}
