using Quartz;
using Quartz.Impl;
using Quartz.Util;
using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Project: SmartEarth.Demos.Scheduler
 * Author: Prince Owen
 * Date: 24th August, 2018
 * Description: A demo implementing the potential of an 
 * effective and reliable scheduler
 */

namespace SmartEarth.Demos.Scheduler
{
    class Program
    {
        static async Task MainAsync(string[] args)
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();

            var date = DateTime.Now;
            var startTime = DateBuilder.DateOf(date.Hour, date.Minute, date.Second+5, date.Day, date.Month, date.Year);
            

            IJobDetail job = JobBuilder.Create<Job>().Build();

            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create().WithIdentity("trigger1", "group1").StartAt(startTime).Build();

            DateTimeOffset? offset = await scheduler.ScheduleJob(job, trigger);
            Console.WriteLine("{0} will run at: {1} and repeat: {2} time(s), every {3} seconds", job.Key, offset, trigger.RepeatCount, trigger.RepeatInterval.TotalSeconds);
            

            // Wait for 20 seconds
            await Task.Delay(20000);
        }

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }
    }
    class Job : IJob
    {
        //static ILogProvider Log => LogProvider..GetLogger(typeof(Job));
        
        public virtual Task Execute(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;
            Console.WriteLine();
            Console.WriteLine("The job says: {0} executing at {1}", key, DateTime.Now.ToString("r"));
            return Task.CompletedTask;
        }
    }
}
