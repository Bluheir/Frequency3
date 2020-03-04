using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Reflection;
using System.Threading.Tasks;

namespace Frequency3.Core
{
	public class CommandProvider
	{
		private static CommandProvider instance;
		private readonly CommandService _commandService;
		private readonly DiscordShardedClient _client;
		private readonly Dictionary<string, List<string>> _potentialCommands = new Dictionary<string, List<string>>();

		public static CommandProvider Instance => instance;

		public CommandProvider(DiscordShardedClient client)
		{
			if (instance != null)
				throw new InvalidOperationException("Cannot create another instance of a singleton");
			_client = client ?? throw new ArgumentNullException($"Parameter {nameof(client)} cannot be null");
			_commandService = new CommandService(new CommandServiceConfig()
			{
				CaseSensitiveCommands = false,
				DefaultRunMode = RunMode.Async,
				LogLevel = LogSeverity.Verbose,
			});
			instance = this;
			_client.MessageReceived += MessageReceived;
		}

		private async Task MessageReceived(SocketMessage arg)
		{
			if (!(arg is SocketUserMessage))
				return;
			var msg = (SocketUserMessage)arg;

			if (msg.Author.IsBot)
				return;
			int argPos = 0;
			if (!msg.HasStringPrefix(".f", ref argPos))
				return;

			await _commandService.ExecuteAsync(new ShardedCommandContext(_client, msg), argPos, null);
		}

		public async Task<IEnumerable<ModuleInfo>> AddModulesAsync(Assembly assembly)
		{
			var a = new List<ModuleInfo>(await _commandService.AddModulesAsync(assembly, null));
			foreach(var item in a)
			{
				foreach(var command in item.Commands)
				{
					string b = GetName(command);
					Console.WriteLine(b);
					if (_potentialCommands.ContainsKey(b))
					{
						var c = _potentialCommands[b];
						c.Add(GetFullName(command));
					}
					else
						_potentialCommands.Add(b, new List<string> { GetFullName(command) });
				}
			}
			return a;
		}
		public Task<ModuleInfo> AddModuleAsync(Type type)
		{
			return _commandService.AddModuleAsync(type, null);
		}
		public static string GetFullName(CommandInfo info)
		{
			string b = GetName(info) + " ";

			foreach(var param in info.Parameters)
			{
				b += ParameterType.TypeToParameter.GetValueOrDefault(param.Type);
			}
			return b;
		}
		public static string GetName(CommandInfo info)
		{
			return GetName(info.Module) + info.Name;
		}
		public static string GetName(ModuleInfo? info)
		{
			if (info == null)
				return "";
			if (info.Group == null)
				return "";
			return GetName(info.Parent) + info.Name + " ";

		}

	}
}
