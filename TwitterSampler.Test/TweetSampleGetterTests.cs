using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TweetsQueueService;
using TweetsQueueService.Models;
using TwitterSampler.Interfaces;

namespace TwitterSampler.Test
{
    [TestClass]
    public class TweetSampleGetterTests
    {
        public TweetSampleGetterTests()
        {

        }

        [TestMethod]
        public void GetTweets_ShouldReturnTweets()
        {
            //setup
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger>();
            var mockClient = new Mock<IQueueClient>();
            //var mockTweetSampleGetter = new Mock<ITweetSampleGetter>();
            var queueClient = new QueueClient(mockLogger.Object);
            //Queue
            //var queue = Task.Run(() => new Queue<Tweet>());
            //queue.Result.Enqueue(GetFakeTweet());
            var mockTweetSampleGetter = new TweetSampleGetter(mockConfiguration.Object, mockLogger.Object, mockClient.Object);
            
            mockClient.Setup(client => client.Enqueue(GetFakeTweet()));


.            var tweetOutput = mockClient.Dequeue();

            Assert.IsNotNull(tweetOutput);
        }

        private Tweet GetFakeTweet()
        {
            Tweet fakeTweet = new Tweet();

            var tweetString = "{\"data\":{\"id\":\"1552660616447512577\",\"text\":\"@xo0mi what makes a fitna dangerous. Its that people in huge number buy what the fitna is saying. Same is the case with imran. He is a dajjal's rep and most lethal fitna Pakistan ever had.\"}}";

            fakeTweet.TweetMessage =
                JsonSerializer.Deserialize<TweetData>(tweetString);
            return fakeTweet;
        }
    }
}
