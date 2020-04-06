using DistributedJobQueue.Client;
using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;


namespace DistributedJobQueue.Job.Wrappers
{
    public class AutoRequeueJob : IJob
    {
        public AutoRequeueJob()
        {

        }
        public AutoRequeueJob(IJob jobToAdd, TimeSpan startToStart, TimeSpan endToStart, IEnumerable<DayOfWeek> blacklistDays, DateTime startQueueTime, DateTime endQueueTime)
        {
            AdditionalJob = jobToAdd;
            StartToStart = startToStart;
            EndToStart = endToStart;
            BlacklistDays = blacklistDays;
            StartQueueTime = startQueueTime.ToUniversalTime().TimeOfDay;
            EndQueueTime = endQueueTime.ToUniversalTime().TimeOfDay;
        }

        public Guid JobId { get; set; }
        public IRequirement Requirement { get; set; }
        public IJob AdditionalJob { get; set; }
        public TimeSpan EndToStart { get; set; }
        public TimeSpan StartToStart { get; set; }
        public IEnumerable<DayOfWeek> BlacklistDays { get; set; }
        public TimeSpan StartQueueTime { get; set; }
        public TimeSpan EndQueueTime { get; set; }

        public async Task<IEnumerable<IJob>> Run()
        {
            AdditionalJob.JobId = Guid.NewGuid();
            while (await this.GetDispatchingQueue().TryEnqueueAsync(AdditionalJob))
            {
                DateTime todaysDate = DateTime.UtcNow;
                if (BlacklistDays.Contains(todaysDate.DayOfWeek))
                {
                    await Task.Delay(TimeSpan.FromDays(1));
                }
                else if(!(todaysDate.TimeOfDay >= StartQueueTime))
                {
                    await Task.Delay(StartQueueTime - todaysDate.TimeOfDay);
                }
                else if(!(todaysDate.TimeOfDay < EndQueueTime))
                {
                    await Task.Delay(StartQueueTime + (TimeSpan.FromHours(24)-todaysDate.TimeOfDay));
                }
                else
                {
                    DateTime startIntervalBegin = todaysDate;
                    if (await this.GetDispatchingQueue().WaitForCompletionAsync(AdditionalJob.JobId))
                    {
                        TimeSpan timeRan = todaysDate - startIntervalBegin;
                        if ((StartToStart - timeRan) < EndToStart)
                        {
                            await Task.Delay(EndToStart);
                        }
                        else
                        {
                            await Task.Delay(StartToStart - timeRan);
                        }

                    }
                    else
                    {
                        break;
                    }
                }
            }
            return new IJob[0];
        }
    }
}
