using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TubeScan.Scheduling
{
    internal class JobScheduleTimer
    {
        public JobScheduleTimer(System.Timers.Timer timer, JobScheduleInfo info)
        {
            Timer = timer;
            Info = info;
        }

        public System.Timers.Timer Timer { get; }
        public JobScheduleInfo Info { get; }
    }
}
