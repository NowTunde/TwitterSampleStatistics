﻿using System;
using System.ComponentModel.DataAnnotations;

namespace TwitterSampler.Models
{
    public class Tweet
    {
        public TweetData? TweetMessage { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public DateTime ReceivedTime { get; set; }
        public int TotalTweetsCount { get; set; }
    }
}

