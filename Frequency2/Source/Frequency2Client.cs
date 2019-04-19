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
using System.Collections.Generic;
using System.Linq;
using Frequency2.Types.Attributes;
using Frequency2.Methods;

namespace Frequency2.Source
{
	
	public sealed class Frequency2Client
	{

		private DiscordShardedClient _client;
		private CommandHandler _commands;
		private IServiceProvider _services;
		
		public static Victoria.LavaShardClient LavaClient { get; private set; }
		private Frequency2Client(){}

		public static Frequency2Client Instance { get; } = new Frequency2Client();
		
		public IReadOnlyList<CommandInfo> Commands
		{
			get
			{
				if (_comms == null)
					_comms = GetCommands();
				return _comms;
			}
		}
		public IReadOnlyDictionary<string, CommandInfo>  CommandInfos { get; private set; }

		private IReadOnlyList<CommandInfo> _comms = null;

		private IReadOnlyList<CommandInfo> GetCommands(bool all = false)
		{
			if (all)
				return _commands.GetCommands();
			var retVal = new List<CommandInfo>();
			var modules = _commands.GetModules();
			var Commands = new Dictionary<string, CommandInfo>();
			foreach(var module in modules)
			{
				bool cont = true;
				foreach(var attribute in module.Attributes)
				{
					if(attribute.GetType() == typeof(IgnoreAttribute))
					{
						cont = false;
						break;
					}
				}
				if (!cont)
					continue;
				foreach(var command in module.Commands)
				{
					bool con = true;
					foreach(var attribute in command.Attributes)
					{
						if(attribute.GetType() == typeof(IgnoreAttribute))
						{
							con = false;
							break;
						}
					}
					if (!con)
						continue;
					retVal.Add(command);
					Commands.Add(CommandsReflection.GetFullName(command), command);
				}
			}
			CommandInfos = Commands;
			return retVal;
		}

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
			_ = Commands;
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
