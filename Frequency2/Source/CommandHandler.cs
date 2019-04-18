using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using static Frequency2.Data.Databases;

namespace Frequency2.Source
{
	public class CommandHandler
	{
		private readonly DiscordShardedClient _client;
		private readonly CommandService _commandService;
		private readonly IServiceProvider _services;
		private readonly ConcurrentDictionary<ulong, int> _userTimeouts = new ConcurrentDictionary<ulong, int>();
		public CommandHandler(DiscordShardedClient client, CommandService commandService, IServiceProvider services)
		{
			_client = client;
			_commandService = commandService;
			_services = services;
		}

		public async Task InitializeAsync(bool hookEvents = true)
		{
			await _commandService.AddModulesAsync(System.Reflection.Assembly.GetExecutingAssembly(), _services);

			if (hookEvents)
			{
				_client.MessageReceived += MessageReceived;
				_ = TimeOutReset();
			}
		}
		private async Task TimeOutReset()
		{
			while (true)
			{
				await Task.Delay(10000);
				_userTimeouts.Clear();
			}
		}
		private async Task MessageReceived(SocketMessage message)
		{
			SocketUserMessage Message = message as SocketUserMessage;
			SocketCommandContext Context = new ShardedCommandContext(_client, Message);

			if (Context.User.IsBot)
				return;

			int argpos = 0;

			var user = await Users.GetOrAddValue((long)Context.User.Id);
			await Users.SaveAsync(user);

			if (!Message.HasStringPrefix(user.Prefix, ref argpos) && !Message.HasMentionPrefix(Context.Client.CurrentUser, ref argpos))
				return;

			if (_userTimeouts.AddOrUpdate(Context.User.Id, 1, (ulong id, int i) => { return i + 1; }) == 5)
			{
				_userTimeouts[Context.User.Id]--;
				return;
			}

			var result = await _commandService.ExecuteAsync(Context, argpos, _services);
			if (!result.IsSuccess)
			{
				_ = Context.Channel.SendMessageAsync($"{Context.User.Mention} Error: `{result.ErrorReason}`");
				
			}
		}
	}
}
