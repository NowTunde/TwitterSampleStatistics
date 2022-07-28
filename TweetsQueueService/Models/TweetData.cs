using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TweetsQueueService.Models
{
    public class TweetData
    {
        [JsonPropertyName("data")]
        public TweetContent? Data { get; set; }
    }
}

