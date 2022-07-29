using System.Net.Http.Headers;
using TwitterSampler.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Text.Json;
using TwitterSampler.Models;
using TweetsQueueService;

namespace TwitterSampler
{
    public class TweetSampleGetter : ITweetSampleGetter
    {
        private readonly string _url;
        private readonly ILogger _logger;
        private readonly string _bearerToken;
        private int _tweetCount;
        private readonly IQueueClient _queueClient;
        private readonly HttpClient _client;
        public TweetSampleGetter(IConfiguration config, ILogger logger, IQueueClient queueClient, IHttpClientFactory client)
        {
            _url = config.GetSection("TwiterSampleStreamUrl")?.Value ?? String.Empty;
            _bearerToken = config.GetSection("BearerToken")?.Value ?? String.Empty;

            _logger = logger;
            _queueClient = queueClient;

            _client = client.CreateClient();          
        }

        public  async Task GetTweets()
        {
            try
            {             
                _client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", _bearerToken);
                _client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

                var stream = await _client.GetStreamAsync(_url);

                using var reader = new StreamReader(stream);

                _logger.Information("TweetSampleGetter Started Successfully!");

                while (!reader.EndOfStream)
                {
                    var currentTweetStr = reader.ReadLine();
                    if (!string.IsNullOrEmpty(currentTweetStr))
                    {
                        var tweet = new Tweet();
                        _tweetCount += 1;
                        tweet.TotalTweetsCount = _tweetCount;
                        tweet.ReceivedTime = DateTime.Now;
                        tweet.TweetMessage = JsonSerializer.Deserialize<TweetData>(currentTweetStr);
                        //Queue the tweet for processing and reporting
                        _queueClient.Enqueue(tweet);
                    }
                }
            }
            catch (Exception exp)
            {
                _logger.Error(exp.Message);
            }
            return;
        }
  
        public Task GetTweetStreamSample()
        {
            throw new NotImplementedException();
        }

        public Task SetQueries()
        {
            throw new NotImplementedException();
        }

        public async Task Run(IQueueReceiver queueReceiver)
        {
            

            if (queueReceiver == null)
            {
                Log.Logger.Error("queueReceiver is null. DI of queueReceiver retrieval failed!");
            }
            else
            {
                await Task.WhenAll(GetTweets(), queueReceiver.Run(_queueClient));
            }
           
            return;
        }
    }
}

