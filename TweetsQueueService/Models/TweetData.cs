using System;
using System.ComponentModel.DataAnnotations;

namespace TweetsQueueService.Models
{
    public class TweetData
    {
        [Key]
        public long Id { get; set; }
        public string? Text { get; set; }
    }
}

