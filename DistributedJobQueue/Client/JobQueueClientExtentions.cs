using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedJobQueue.Client
{
    public static class JobQueueClientExtentions
    {
        public static async Task<bool> StartDeamonAsync(this IJobQueueClient client, bool? throwOnErrorExit = null)
        {
            bool thro = DeriveThrowOnErrorExit(throwOnErrorExit);

            bool cleanExit = true;
            Exception dirtyExitExpection = null;
            SemaphoreSlim awaiter = new SemaphoreSlim(0, 1);

            ThreadPool.QueueUserWorkItem(async (x) =>
            {
                try
                {
                    bool cont = true;
                    {
                        cont = await client.RunNextAsync();
                    } while (cont) ;
                }
                catch (Exception e)
                {
                    dirtyExitExpection = e;
                    cleanExit = false;
                }
                finally
                {
                    awaiter.Release();
                }
            });

            //block this thread until deamon thread exits
            await awaiter.WaitAsync();

            if(!cleanExit && thro)
            {
                throw dirtyExitExpection;
            }

            return cleanExit;
        }
        public static bool StartDeamon(this IJobQueueClient client, bool? throwOnErrorExit = null)
        {
            bool thro = DeriveThrowOnErrorExit(throwOnErrorExit);

            bool cleanExit = true;
            Exception dirtyExitExpection = null;
            Semaphore awaiter = new Semaphore(0, 1);

            ThreadPool.QueueUserWorkItem(async (x) =>
            {
                try
                {
                    bool cont = true;
                    {
                        cont = await client.RunNextAsync();
                    } while (cont) ;
                }
                catch (Exception e)
                {
                    dirtyExitExpection = e;
                    cleanExit = false;
                }
                finally
                {
                    awaiter.Release();
                }
            });

            //block this thread until deamon thread exits
            awaiter.WaitOne();

            if (!cleanExit && thro)
            {
                throw dirtyExitExpection;
            }

            return cleanExit;
        }

        private static bool DeriveThrowOnErrorExit(bool? throwOnErrorExit)
        {
            if (!throwOnErrorExit.HasValue)
            {
                #if DEBUG
                    return true;
                #else
                    return false;
                #endif
            }
            return throwOnErrorExit.Value;
        }
    }
}
