using TweetsQueueService.Models;

namespace TweetsQueueService
{
    public interface IQueueReceiver
    {
        Task Run(IQueueClient queueClient);
        void ProcessTweet(Tweet tweet, int curTweetsPerMinute);
        void PrintReport(int curTweetsPerMinute);
    }
}