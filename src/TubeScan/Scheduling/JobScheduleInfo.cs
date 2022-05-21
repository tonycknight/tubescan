﻿namespace TubeScan.Scheduling
{
    internal class JobScheduleInfo
    {
        public JobScheduleInfo(IJob job, TimeSpan frequency)
        {
            Job = job;
            Frequency = frequency;
        }

        public TimeSpan Frequency { get; }

        public DateTime LastExecution { get; set; }
        
        public IJob Job { get; }
    }
}