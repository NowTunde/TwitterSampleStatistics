using System;
using System.Net;
using System.Net.Http.Headers;
using TwitterSampler.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;
//using Newtonsoft.Json;
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

        public TweetSampleGetter(IConfiguration config, ILogger logger, IQueueClient queueClient)
        {
            _url = config.GetSection("TwiterSampleStreamUrl")?.Value ?? String.Empty;
            _bearerToken = config.GetSection("BearerToken")?.Value ?? String.Empty;

            _logger = logger;
            _queueClient = queueClient;
        }

        public  async Task GetTweets()
        {
            try
            {
                using var _client = new HttpClient();


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

        public Tweet GetFakeTweet()
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(new Random().Next() % 5000));
            Tweet fakeTweet = new Tweet();
            
            var tweetString = "{\"data\":{\"id\":\"1552660616447512577\",\"text\":\"@xo0mi what makes a fitna dangerous. Its that people in huge number buy what the fitna is saying. Same is the case with imran. He is a dajjal's rep and most lethal fitna Pakistan ever had.\"}}";

            fakeTweet.TweetMessage = 
                JsonSerializer.Deserialize<TweetData>(tweetString);
            return fakeTweet;
        }

        //ToDo - Delete and Replace with connect good
        public async Task GetTweetsFake()
        {
            try
            {
                _logger.Information("TweetSampleGetter Started Successfully!");
                var fakeTweet = new Tweet();

             await   Task.Run(() =>
                {
                    while (fakeTweet != null)
                    {

                        if (!string.IsNullOrEmpty(fakeTweet?.TweetMessage?.Data?.Text))
                        {
                            var tweet = new Tweet();
                            _tweetCount += 1;
                            tweet.TotalTweetsCount = _tweetCount;
                            tweet.ReceivedTime = DateTime.Now;
                            tweet.TweetMessage = fakeTweet.TweetMessage;

                            //Queue the tweet for processing and reporting
                            _queueClient.Enqueue(tweet);
                        }

                        
                        fakeTweet = GetFakeTweet();
                    }

                });
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
                //await queueReceiver.Run(_queueClient);
            }
            

            return;
        }
    }
}

