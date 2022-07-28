using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RichardSzalay.MockHttp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TweetsQueueService;
using TweetsQueueService.Models;

namespace TwitterSampler.Test
{
    [TestClass]
    public  class GetQueueClientTests
    { 
        [TestMethod]
        public async Task GetQueueTest_Should_ReturnQueue()
        {
            var mockLogger = new Mock<ILogger>();

            var queueClient = new QueueClient(mockLogger.Object);
            queueClient.Enqueue(GetFakeTweet());
            var queue = await queueClient.GetQueue();
            //confirm that no exception is thrown
            Assert.IsNotNull(queue);
        }

        private Tweet GetFakeTweet()
        {
            Tweet fakeTweet = new Tweet() { TotalTweetsCount = 1 };

            var tweetString = "{\"data\":{\"id\":\"1552660616447512577\",\"text\":\"@xo0mi what makes a fitna dangerous. Its that people in huge number buy what the fitna is saying. Same is the case with imran. He is a dajjal's rep and most lethal fitna Pakistan ever had.\"}}";

            fakeTweet.TweetMessage =
                JsonSerializer.Deserialize<TweetData>(tweetString);
            return fakeTweet;
        }
    }
}
