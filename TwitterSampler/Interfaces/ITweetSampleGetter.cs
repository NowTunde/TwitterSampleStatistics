using System;
using TweetsQueueService;

namespace TwitterSampler.Interfaces
{
    public interface ITweetSampleGetter
    {
        Task GetTweets();
        Task SetQueries();
        Task GetTweetStreamSample();
        Task Run(IQueueReceiver client);
    }
}

