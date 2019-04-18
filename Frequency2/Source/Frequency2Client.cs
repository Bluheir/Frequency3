using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Frequency2.Config;
using Microsoft.Extensions.DependencyInjection;
using System;
using Frequency2.Data.Models;
using Frequency2.Data;
using System.IO;
using System.Threading.Tasks;

namespace Frequency2.Source
{
	public class Frequency2Client
	{

		private DiscordShardedClient _client;
		private CommandHandler _commands;
		private IServiceProvider _services;
		public static Victoria.LavaShardClient LavaClient { get; private set; }

		public async Task StartAsync(bool sleep = true)
		{
			var config = Configuration.Config;

			_client = new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = true,
				LogLevel = Discord.LogSeverity.Info,
				MessageCacheSize = 1000
			});

			_services = new ServiceCollection()
				.AddSingleton(_client)
				.AddSingleton(Audio.AudioService.Instance)
				.BuildServiceProvider();

			_commands = new CommandHandler(_client,
				new CommandService(new CommandServiceConfig
				{
					LogLevel = LogSeverity.Info,
					DefaultRunMode = RunMode.Async
				}),
				_services);

			new Logger(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "/Logs", add: true);

			_client.ShardReady += ShardReady;
			_client.Log += Log;

			Databases.Users = await DataBase<long, UserData>.Async(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "/Databases/Users.db", x =>
			{
				UserData retVal = new UserData
				{
					Key = x,
					Prefix = ".f",
					SendCompMessage = true
				};
				return retVal;
			});

			await _commands.InitializeAsync();

			await _client.LoginAsync(TokenType.Bot, config.Token);
			await _client.StartAsync();

			if(sleep)
				await Task.Delay(-1);
		}

		private async Task Log(LogMessage arg)
		=> await Logger.Instance.LogAsync(arg);
		

		private int shardnum = 0;
		private async Task ShardReady(DiscordSocketClient arg)
		{
			if (++shardnum == _client.Shards.Count)
			{
				await _client.SetGameAsync(Configuration.Config.RichPresence, type: ActivityType.Watching);
				LavaClient = new Victoria.LavaShardClient();

				LavaClient.Log += Audio.AudioService.Instance.Log;
				LavaClient.OnTrackFinished += Audio.AudioService.Instance.TrackFinished;

				await LavaClient.StartAsync(_client, new Victoria.Configuration
				{
					AutoDisconnect = false,
					SelfDeaf = false,
					LogSeverity = LogSeverity.Info,
					Host = Configuration.Config.LavaLinkSettings.Host,
					Password = Configuration.Config.LavaLinkSettings.Password,
					Port = Configuration.Config.LavaLinkSettings.Port
				});
			}
		}
	}
}
