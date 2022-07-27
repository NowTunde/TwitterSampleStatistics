using System;
namespace TweetsQueueService.Models
{
    public class ReportModel
    {
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; }
        public int TotalTweetsRecieved { get; set; }
        public float AverageTweetPerMinute { get; set; }
        public float StandardDeviation { get; set; }
    }
}

