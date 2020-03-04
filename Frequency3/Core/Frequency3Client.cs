using System;
using System.Collections.Generic;
using Discord.Net;
using Discord;
using Frequency3.Core;
using System.Reflection;
using Discord.WebSocket;
using Frequency3.Audio;
using System.Threading.Tasks
using Frequency3.Logging;

namespace Frequency3.Core
{
	public class Frequency3Client
	{
		private readonly DiscordShardedClient _client;
		private readonly CommandProvider _commands;
		private readonly PaginationService _pagination;
		private readonly BotConfiguration _config;
		private readonly Logger _logger;
		private readonly AudioService _service;
		private static Frequency3Client _instance;
		private int s;

		public static Frequency3Client Instance => _instance;

		public Frequency3Client()
		{
			if(_instance != null)
			{
				throw new InvalidOperationException("Cannot create another instnace of a singleton.");
			}
			_client = new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = true,
				LogLevel = LogSeverity.Info,
			});
			_commands = new CommandProvider(_client);
			_pagination = new PaginationService(_client);
			_config = BotConfiguration.Config;
			var lava = _config.LavaLinkSettings;
			
			_service = new AudioService(lava.Host, lava.Password, lava.Port, _client);
			_logger = new Logger(@"C:\Users\User\Desktop\Logs\");
			s = 0;
			_instance = this;
		}

		public async Task StartAsync()
		{
			_client.UserJoined += UserJoined;
			_client.Log += Log;
			_client.ShardReady += ShardReady;
			Console.WriteLine(_config.Token);
			await _commands.AddModulesAsync(Assembly.GetExecutingAssembly());
			await _client.LoginAsync(TokenType.Bot, _config.Token, false);

			await _client.StartAsync();
			await Task.Delay(-1);
		}

		private async Task ShardReady(DiscordSocketClient client)
		{
			if (++s == _client.Shards.Count) 
			{
				await _service.StartAsync();
			}
		}

		public async Task Log(LogMessage msg)
		{
			await _logger.LogAsync(msg);
		}

		private Task UserJoined(SocketGuildUser arg)
		{
			return Task.CompletedTask;
		}
	}
}
