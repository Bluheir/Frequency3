

using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Frequency2.Config;
using System.Collections.Concurrent;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Frequency2
{
	
	class Program
	{

		private static void Main()
		=> new Program().MainAsync().GetAwaiter().GetResult();

		private DiscordSocketClient _client;
		private CommandService _commands;
		private IServiceProvider _services;
		public static Victoria.LavaSocketClient LavaClient { get; private set; }

		private readonly ConcurrentDictionary<ulong, int> _userTimeouts = new ConcurrentDictionary<ulong, int>();

	    internal async Task MainAsync()
		{
			var config = Configuration.Config;

			_client = new DiscordSocketClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = true,
				LogLevel = Discord.LogSeverity.Info,
				MessageCacheSize = 1000
			});

			_ = TimeOutReset();

			_commands = new CommandService(new CommandServiceConfig
			{
				LogLevel = LogSeverity.Info,
				DefaultRunMode = RunMode.Async
			});

			_services = new ServiceCollection()
				.AddSingleton(_client)
				.AddSingleton(_commands)
				//.AddSingleton<Victoria.LavaRestClient>()
				//.AddSingleton(LavaClient)
				.AddSingleton(Audio.AudioService.Instance)
				.BuildServiceProvider();
			
			
			await _commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly(), _services);

			_client.MessageReceived += MessageReceived;
			_client.Ready += ShardReady;
			_client.Log += Log;

			await _client.LoginAsync(TokenType.Bot, config.Token);
			await _client.StartAsync();


			await Task.Delay(-1);
		}

		private Task Log(LogMessage arg)
		{
			Console.WriteLine($"{DateTime.Now} {arg.Source}] {arg.Message}");
			return Task.CompletedTask;
		}

		private async Task TimeOutReset()
		{
			while(true)
			{
				await Task.Delay(10000);
				_userTimeouts.Clear();
			}
		}
	
		private int shardnum = 0;
		private async Task ShardReady(/*DiscordSocketClient arg*/)
		{
			/*if (++shardnum == _client.Shards.Count)*/
			{
				await _client.SetGameAsync(Configuration.Config.RichPresence, type: ActivityType.Watching);
				Console.WriteLine(LavaClient == null || _client == null);
				LavaClient = new Victoria.LavaSocketClient();

				LavaClient.Log += Audio.AudioService.Instance.Log;
				LavaClient.OnTrackFinished += Audio.AudioService.Instance.TrackFinished;
				
				await LavaClient.StartAsync(_client);
			}
		}

		private async Task MessageReceived(SocketMessage arg)
		{
			SocketUserMessage Message = arg as SocketUserMessage;
			SocketCommandContext Context = new SocketCommandContext(_client, Message);

			if (Context.User.IsBot)
				return;

			int argpos = 0;
			if (!Message.HasStringPrefix(".f", ref argpos) && !Message.HasMentionPrefix(Context.Client.CurrentUser, ref argpos))
				return;

			if (_userTimeouts.AddOrUpdate(Context.User.Id, 1, (ulong id, int i) => { return i + 1; }) == 5)
			{
				_userTimeouts[Context.User.Id]--;
				return;
			}

			var result = await _commands.ExecuteAsync(Context, argpos, _services);
			if(!result.IsSuccess)
			{
				_ = Context.Channel.SendMessageAsync($"{Context.User.Mention} Error: `{result.ErrorReason}`");                                                 
			}
		}
		
	}
}
