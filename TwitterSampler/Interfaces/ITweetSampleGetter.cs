using System;
using TweetsQueueService;

namespace TwitterSampler.Interfaces
{
    public interface ITweetSampleGetter
    {
        Task Connect();
        Task SetQueries();
        Task GetTweetStreamSample();
        Task Run(IQueueReceiver client);
    }
}

