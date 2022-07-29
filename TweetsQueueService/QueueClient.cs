using System;
using Serilog;
using TweetsQueueService.Models;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace TweetsQueueService
{
    public class QueueClient : IQueueClient
    {
        
        private ILogger _logger;
        private readonly Queue<Tweet> _tweetQueue;

        public QueueClient(ILogger logger)
        {
            _logger = logger;
            _tweetQueue = new Queue<Tweet>();
        }

        public void Enqueue(Object tweetObj)
        {
            var message = JsonSerializer.SerializeToUtf8Bytes(tweetObj);
            var tweet = JsonSerializer.Deserialize<Tweet>(message);

            if(tweet != null)
            {
                _tweetQueue.Enqueue(tweet);
            }
        }

        public async Task<Queue<Tweet>> GetQueue()
        {
            await WaitForTweets();

            return _tweetQueue;
        }

        public async Task WaitForTweets()
        {
            await Task.Run(() =>
            {
                while (_tweetQueue.Count == 0)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(1000));
                }
            });
        }
    }

}

