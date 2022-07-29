using TweetsQueueService.Models;

namespace TweetsQueueService
{
    public interface IQueueReceiver
    {
        Task Run(IQueueClient queueClient, bool testMode=false);
        void ProcessTweet(Tweet tweet, int curTweetsPerMinute);
        void PrintReport();
    }
}