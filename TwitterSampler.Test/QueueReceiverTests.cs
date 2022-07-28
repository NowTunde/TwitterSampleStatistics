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
using TwitterSampler.Interfaces;

namespace TwitterSampler.Test
{
    [TestClass]
    public class QueueReceiverTests
    {
        private readonly IConfigurationRoot _configuration;
        public QueueReceiverTests()
        {
            _configuration = new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                                        .AddJsonFile("appsettings.json", false)
                                        .Build();
        }

        [TestMethod]
        public async Task ExceptionNotThrown_When_RunMethodIsCalled()
        {
            Tweet tweet = GetFakeTweet();

            var mockLogger = new Mock<ILogger>();
            var mockClient = new Mock<IQueueClient>();
            var mockHttp = new Mock<IHttpClientFactory>();

            var queueClient = new QueueClient(mockLogger.Object);
            queueClient.Enqueue(tweet);
            var queueReceiver = new QueueReceiver(mockLogger.Object, _configuration);
            await queueReceiver.Run(queueClient, true);
            //NO Exceptions are thrown
            Assert.IsTrue(true);  
        }

        [TestMethod]
        public async Task TweetsReceived_And_CreationTimesMatch()
        {
            Tweet tweet = GetFakeTweet();

            var mockLogger = new Mock<ILogger>();
            var mockClient = new Mock<IQueueClient>();
            var mockHttp = new Mock<IHttpClientFactory>();

            var queueClient = new QueueClient(mockLogger.Object);
            queueClient.Enqueue(tweet);

            var queueReceiver = new QueueReceiver(mockLogger.Object, _configuration);

            var queue = await queueClient.GetQueue();
            var outputTweet = queue.Dequeue();

            Assert.AreEqual(outputTweet?.CreationTime, tweet?.CreationTime);
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
