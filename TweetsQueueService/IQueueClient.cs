using TweetsQueueService.Models;

namespace TweetsQueueService
{
    public interface IQueueClient
    {
        void Enqueue(Object tweet);
        Task<Queue<Tweet>> GetQueue();
    }
}