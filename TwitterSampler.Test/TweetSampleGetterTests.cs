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
    public class TweetSampleGetterTests
    {
        private const string TweetString = "{\"data\":{\"id\":\"1552660616447512577\",\"text\":\"@xo0mi what makes a fitna dangerous. Its that people in huge number buy what the fitna is saying. Same is the case with imran. He is a dajjal's rep and most lethal fitna Pakistan ever had.\"}}";
        private readonly IConfiguration _configuration;
        public TweetSampleGetterTests()
        {
            _configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                            .AddJsonFile("appsettings.json", false)
                            .Build();
        }

        [TestMethod]
        public async Task GetTweets_ShouldNotThrowException()
        {
            var mockLogger = new Mock<ILogger>();
            var mockClient = new Mock<IQueueClient>();
            var mockHttp = new Mock<IHttpClientFactory>();

            var queueClient = new QueueClient(mockLogger.Object);

            var clientHandlerStub = new DelegatingHandlerStub();
            var client = new HttpClient(clientHandlerStub);

            mockHttp.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockHttp.Object;


            var tweetSampleGetter = new TweetSampleGetter(_configuration, mockLogger.Object, mockClient.Object, factory);

            await tweetSampleGetter.GetTweets();
            //confirm that no exception is thrown
            Assert.IsTrue(true);
        }
    }
}
