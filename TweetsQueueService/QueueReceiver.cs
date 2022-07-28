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
        private int _sleepTime, _curTweetsCount;
        private bool _lockTaken;
        private const int ReportTimeoutMilliseconds = 500;
        private DateTime _tweetStartTime;
        private static object _lockObj = new object();
        private static object _lockObj2 = new object();

        public QueueReceiver(ILogger logger, IConfigurationRoot configuration)
        {
            _logger = logger;
            _tweetsReport = new ReportModel();
            _updatedTweetsReport = new ReportModel();
            _configuration = configuration;
            _sleepTime = int.Parse(_configuration.GetSection("ReceiverSleepTimeSeconds")?.Value ?? "0");
        }

        public async Task Run(IQueueClient queueClient)
        {
            //_tweetStartTime = tweetStartTime;

            try
            {
                //DeQueue:
                var queue = await queueClient.GetQueue();
                Tweet tweet = new Tweet();
                

                _tweetStartTime = queue.Peek().ReceivedTime;
                DateTime startTime = _tweetStartTime;
                while (true)
                {
                    if(queue.Count > 0)
                    {
                        tweet = queue.Dequeue();
                        _tweetStartTime = _tweetStartTime == default(DateTime) ? tweet.ReceivedTime : _tweetStartTime;

                        
                        if(tweet == null || tweet.TweetMessage == null || string.IsNullOrEmpty(tweet.TweetMessage.Text))
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
                        if(DateTime.Now.Subtract(startTime).Minutes >= 1)
                        {
                            PrintReport();
                            startTime = DateTime.Now;
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
                    
                //calculate mean receival time
                //calculate the standard deviation
                //Set the count average per minute
                //Set the start time
                //Set the end time
                //Set the total tweets received
                _updatedTweetsReport.TotalTweetsRecieved += 1;
                _updatedTweetsReport.StartTime = _tweetStartTime;
                _updatedTweetsReport.EndTime = tweet.ReceivedTime;
                if (tweet.ReceivedTime.Subtract(_tweetStartTime).Minutes >= 1)
                {
                    _updatedTweetsReport.AverageTweetPerMinute = _updatedTweetsReport.TotalTweetsRecieved / (tweet.ReceivedTime.Subtract(_tweetStartTime).Minutes);

                }
                if(_updatedTweetsReport.TotalTweetsRecieved != tweet.TotalTweetsCount)
                {
                    throw new Exception("Tweet count missmatch");
                }

            }
            
        }

        public void PrintReport()
        {
            var resultOutputAsJson = JsonConvert.SerializeObject(_updatedTweetsReport, Formatting.Indented);
            System.Diagnostics.Debug.WriteLine(resultOutputAsJson);
        }
    }
}

