using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Frequency2.MainTypes
{
	public class CommandHandler
	{
		private readonly DiscordShardedClient _client;
		private readonly CommandService _commandService;
		private readonly IServiceProvider _services;

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
			}
		}

		private Task MessageReceived(SocketMessage message)
		{
			return Task.CompletedTask;
		}
	}
}
