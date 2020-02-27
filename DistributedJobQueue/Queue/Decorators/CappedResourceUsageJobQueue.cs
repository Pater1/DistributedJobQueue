using DistributedJobQueue.Job;
using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace DistributedJobQueue.Queue.Decorators
{
    public class CappedResourceUsageJobQueue : IJobQueue, IDisposable
    {
        public enum Resource
        {
            CPU,
            RAM,
            //NET_IN,
            //NET_OUT,
            //NET = NET_IN | NET_OUT
            //GPU
            //DISK
        }
        public CappedResourceUsageJobQueue(IJobQueue baseQueue, params (Resource resource, decimal limit)[] recourceLimits): this(baseQueue, (IEnumerable<(Resource resource, decimal limit)>)recourceLimits) {}
        public CappedResourceUsageJobQueue(IJobQueue baseQueue, IEnumerable<(Resource resource, decimal limit)> recourceLimits)
        {
            BaseQueue = baseQueue;
            Proc = Process.GetCurrentProcess();
            RecourceLimits = recourceLimits.ToArray();

            CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            CpuCounter.NextValue();
            //RamCounter = new PerformanceCounter("Memory", "Available MBytes", Proc.ProcessName);
            //RamCounter.NextValue();
        }

        private IJobQueue BaseQueue { get; }
        private Process Proc { get; }

        private PerformanceCounter CpuCounter { get; }
        //private PerformanceCounter RamCounter { get; }

        private IEnumerable<(Resource resource, decimal limit)> RecourceLimits { get; }

        public void Dispose()
        {
            Proc.Dispose();
        }

        private decimal GetResourceUsage(Resource rec)
        {
            decimal ret;
            switch (rec)
            {
                case Resource.CPU:
                    ret = (decimal)CpuCounter.NextValue();
                    break;
                case Resource.RAM:
                    //ret = GC.GetTotalMemory(false);
                    ret = Proc.PrivateMemorySize64;
                    break;
                default:
                    ret = decimal.MaxValue;
                    break;
            }
            return ret;
        }

        public async Task<(bool, IJob)> TryDequeueAsync(IRequirement requirementsFulfillable = null)
        {
            //Proc.MachineName
            while(!RecourceLimits.Select(x => GetResourceUsage(x.resource) < x.limit).Aggregate((a,b) => a && b))
            {
                await Task.Delay(10);
                Proc.Refresh();
            }

            return await BaseQueue.TryDequeueAsync(requirementsFulfillable);
        }

        public Task<bool> TryEnqueueAsync(IJob job)
        {
            return BaseQueue.TryEnqueueAsync(job);
        }

        public Task<bool> WaitForCompletionAsync(Guid jobId)
        {
            return BaseQueue.WaitForCompletionAsync(jobId);
        }
    }
}
