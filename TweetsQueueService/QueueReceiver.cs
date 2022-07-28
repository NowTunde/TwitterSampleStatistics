using System;
using Serilog;
using TweetsQueueService.Models;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace TweetsQueueService
{
    public class QueueReceiver : IQueueReceiver
    {
        private ILogger _logger;
        private ReportModel _tweetsReport, _updatedTweetsReport;
        private readonly IConfigurationRoot _configuration;
        private int _sleepTime;
        private int _curTweetsCount;
        private DateTime _tweetStartTime;
        private DateTime _cycleStartTime;
        private static object _lockObj = new object();
        private static object _lockObj2 = new object();
        private static List<int> _tweetsPerMinute;

        public QueueReceiver(ILogger logger, IConfigurationRoot configuration)
        {
            _logger = logger;
            _tweetsReport = new ReportModel();
            _updatedTweetsReport = new ReportModel();
            _configuration = configuration;
            _sleepTime = int.Parse(_configuration.GetSection("ReceiverSleepTimeSeconds")?.Value ?? "0");
            _tweetsPerMinute = new List<int>();
        }

        public async Task Run(IQueueClient queueClient)
        {
            try
            {
                //DeQueue:
                var queue = await queueClient.GetQueue();
                Tweet tweet = new Tweet();
                
                _tweetStartTime = queue.Peek().ReceivedTime;
                _cycleStartTime = _tweetStartTime;
                while (true)
                {
                    if(queue.Count > 0)
                    {
                        tweet = queue.Dequeue();
                        _tweetStartTime = _tweetStartTime == default(DateTime) ? tweet.ReceivedTime : _tweetStartTime;

                        
                        if(tweet == null || tweet.TweetMessage == null || string.IsNullOrEmpty(tweet?.TweetMessage?.Data?.Text))
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(_sleepTime));
                            continue;

                        }
                        else
                        {
                            lock (_lockObj2)
                            {
                                _curTweetsCount += 1;

                                ProcessTweet(tweet, _curTweetsCount);
                                
                            }
                        }

                        //Check if we have been processing tweets for a minute otherwise continue:
                        if(tweet.ReceivedTime.Subtract(_cycleStartTime).Minutes >= 1)
                        {
                            _tweetsPerMinute?.Add(_curTweetsCount);
                            PrintReport();
                            _cycleStartTime = tweet.ReceivedTime;
                            _curTweetsCount = 0;
                        }
                    }   
                }

            }
            catch (Exception exp)
            {
                _logger.Error(exp, exp.Message);
            }
        }

        public void ProcessTweet(Tweet tweet, int curTweetsCount)
        {                        
            lock(_lockObj)
            {
                  
                _updatedTweetsReport.TotalTweetsRecieved += 1;
                _updatedTweetsReport.StartTime = _tweetStartTime;
                _updatedTweetsReport.EndTime = tweet.ReceivedTime;
                if (tweet.ReceivedTime.Subtract(_tweetStartTime).Minutes >= 1)
                {
                    _updatedTweetsReport.AverageTweetPerMinute = _updatedTweetsReport.TotalTweetsRecieved / (tweet.ReceivedTime.Subtract(_tweetStartTime).Minutes);

                }

                if (curTweetsCount < _updatedTweetsReport.TotalTweetsRecieved)
                {
                    _updatedTweetsReport.StandardDeviation = GetStandardDeviation();
                }

                if (_updatedTweetsReport.TotalTweetsRecieved != tweet.TotalTweetsCount)
                {
                    throw new Exception("Tweet count missmatch");
                }

               
            }
            
        }

        private double GetStandardDeviation()
        {
            var sumOfSquaresOfDifferences = _tweetsPerMinute.Select(freq =>
            (freq - _updatedTweetsReport.AverageTweetPerMinute) *
            (freq - _updatedTweetsReport.AverageTweetPerMinute)).Sum();

            return Math.Sqrt((double)sumOfSquaresOfDifferences / _tweetsPerMinute.Count());
        }

        public void PrintReport()
        {
            Console.WriteLine("===========================================================================");
            Console.WriteLine("TweetSampler Statistics:");
            var resultOutputAsJson = JsonConvert.SerializeObject(_updatedTweetsReport, Formatting.Indented);
            Console.WriteLine(resultOutputAsJson);

            _tweetsReport = _updatedTweetsReport;
        }
    }
}

