

using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Frequency2.Config;
using System.Collections.Concurrent;

namespace Frequency2
{
	
	class Program
	{

		static void Main(string[] args)
		=> new Program().MainAsync().GetAwaiter().GetResult();

		private DiscordShardedClient _client;
		private CommandService _commands;
		private Victoria.LavaShardClient _lavalink;

		private readonly ConcurrentDictionary<ulong, int> _userTimeouts = new ConcurrentDictionary<ulong, int>();

	    internal async Task MainAsync()
		{
			var config = Configuration.Config;
			System.Console.WriteLine(config.ApexApiKey);

			_client = new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = true,
				LogLevel = Discord.LogSeverity.Info,
				MessageCacheSize = 1000
			});

			_lavalink = new Victoria.LavaShardClient();
			await _lavalink.StartAsync(_client);

			_ = TimeOutReset();

			_commands = new CommandService(new CommandServiceConfig
			{
				LogLevel = LogSeverity.Info,
				DefaultRunMode = RunMode.Async
			});

			await _commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly(), null);

			_client.MessageReceived += MessageReceived;
			_client.ShardReady += ShardReady;

			await _client.LoginAsync(TokenType.Bot, config.Token);
			await _client.StartAsync();

			await Task.Delay(-1);
		}

		private async Task TimeOutReset()
		{
			while(true)
			{
				await Task.Delay(10000);
				_userTimeouts.Clear();
			}
		}
	
		private async Task ShardReady(DiscordSocketClient arg)
		{
			await _client.SetGameAsync(Configuration.Config.RichPresence, type: ActivityType.Watching);
			Audio.AudioService.Init(_lavalink);
		}

		private async Task MessageReceived(SocketMessage arg)
		{
			SocketUserMessage Message = arg as SocketUserMessage;
			ShardedCommandContext Context = new ShardedCommandContext(_client, Message);

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

			var result = await _commands.ExecuteAsync(Context, argpos, null);
			if(!result.IsSuccess)
			{
				_ = Context.Channel.SendMessageAsync($"Error: `{result.ErrorReason}`");                                                 
			}
		}
		
	}
}
