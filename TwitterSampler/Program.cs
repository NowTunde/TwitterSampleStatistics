using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TweetsQueueService;
using TwitterSampler.Interfaces;

namespace TwitterSampler
{

	public class Program
	{

		public static IConfigurationRoot? Config;
		// See https://aka.ms/new-console-template for more information

		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
						 .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
						 .MinimumLevel.Debug()
						 .Enrich.FromLogContext()
						 .CreateLogger();

			try
			{
				// Start!
				MainAsync(args).Wait();
			}
			catch (Exception exp)
			{
				Log.Logger.Error(exp, exp.Message);
			}


		}

		static async Task MainAsync(string[] args)
		{
			ServiceCollection serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);

			IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

			try
			{
				Log.Information("Starting service");
				//Start the Queue Service
				var queueClient = serviceProvider.GetService<IQueueClient>();
				var queueReceiver = serviceProvider.GetService<IQueueReceiver>();

				
				var tweetSampleGetter = serviceProvider.GetService<ITweetSampleGetter>();
				if (tweetSampleGetter == null)
				{
					Log.Logger.Error("TweetSampleGetter is null. DI of TweetSampleGetter retrieval failed!");
				}
				else if(queueReceiver != null)
				{
					 await tweetSampleGetter.Run(queueReceiver);
				}
				
				
				Log.Information("Ending service");
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Error running service");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		private static void ConfigureServices(IServiceCollection serviceCollection)
		{
			// Add logging
			serviceCollection.AddSingleton(LoggerFactory.Create(builder =>
			{
				builder
					.AddSerilog(dispose: true);
			}));

			serviceCollection.AddLogging();

			// Build configuration
			Config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
				.AddJsonFile("appsettings.json", false)
				.Build();

			// Add access to generic IConfigurationRoot
			serviceCollection.AddSingleton<IConfigurationRoot>(Config);

			// Add app
			var queueClient = new QueueClient(Log.Logger);
			serviceCollection.AddSingleton<ITweetSampleGetter>(new TweetSampleGetter(Config, Log.Logger, queueClient));

			serviceCollection.AddSingleton<IQueueClient>(queueClient);
			serviceCollection.AddSingleton<IQueueReceiver>(new QueueReceiver(Log.Logger, Config));
		}
	}

}