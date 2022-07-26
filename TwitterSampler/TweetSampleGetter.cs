using System;
using System.Net;
using System.Net.Http.Headers;
using TwitterSampler.Interfaces;
using Microsoft.Extensions.Configuration;

namespace TwitterSampler
{
    public class TweetSampleGetter : ITweetSampleGetter
    {
        private readonly string _url;
        private HttpClient? _client;
        private readonly string _bearerToken;

        public TweetSampleGetter(IConfiguration config)
        {
            _url = config.GetSection("TwiterSampleStreamUrl")?.Value ?? String.Empty;
            _bearerToken = config.GetSection("BearerToken")?.Value ?? String.Empty;
        }

        public  async Task Connect()
        {
            using ( _client = new HttpClient())
            {

                _client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", _bearerToken);
                _client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
                
                var stream = await _client.GetStreamAsync(_url);

                using (var reader = new StreamReader(stream))
                {

                    while (!reader.EndOfStream)
                    {

                        //We are ready to read the stream
                        var currentLine = reader.ReadLine();
                    }
                }
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

        public async Task Run()
        {
           await Connect();

            return;
        }
    }
}

