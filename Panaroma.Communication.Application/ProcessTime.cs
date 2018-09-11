using System;

namespace Panaroma.Communication.Application
{
    public class ProcessTime
    {
        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public TimeSpan TotalProcessTime { get; set; }
    }
}