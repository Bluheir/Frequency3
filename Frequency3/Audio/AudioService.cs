using System;
using System.Collections.Generic;
using System.Text;
using Victoria;
using Frequency3.Core;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Victoria.EventArgs;
using Frequency3.Helpers.Methods;

namespace Frequency3.Audio
{
	public sealed class AudioService
	{
		private readonly LavaNode _audio;
		private readonly DiscordShardedClient _client;
		private static AudioService? _instance;

		public AudioService(LavaConfig config, DiscordShardedClient _client)
		{
			if (_instance != null)
				throw new InvalidOperationException("Cannot create another instance of a singleton");
			_audio = new LavaNode(_client, config);
			_instance = this;
			this._client = _client;

			_audio.OnTrackEnded += TrackFinish;
			_audio.OnLog += OnLog;
		}

		private async Task OnLog(LogMessage arg)
		{
			await Frequency3Client.Instance.Log(arg);
		}

		private async Task TrackFinish(TrackEndedEventArgs arg)
		{
			var player = arg.Player;
			var reason = arg.Reason;
			var track = arg.Track;

			
		}

		public AudioService(string host, string password, ushort port, DiscordShardedClient _client) : this(new LavaConfig()
		{
			Hostname = host,
			Authorization = password,
			Port = port
		}, _client)
		{ }
		public AudioService(DiscordShardedClient _client) : this(new LavaConfig()
		{
			Hostname = BotConfiguration.Config.LavaLinkSettings.Host,
			Authorization = BotConfiguration.Config.LavaLinkSettings.Password,
			Port = BotConfiguration.Config.LavaLinkSettings.Port,
		}, _client)
		{ }

		public async Task StartAsync()
		{
			await _audio.ConnectAsync();
		}

		public async Task<LavaPlayer?> JoinAsync(ICommandContext context, ITextChannel textChannel, bool sendMsg = true, bool sendAdv = true)
		{
			if (context.Guild == null)
				return null; ;
			var guild = _audio.GetPlayer(context.Guild);
			var usersVc = context.UsersVC() as SocketVoiceChannel;
			var user = context.User as IGuildUser;

			if(usersVc == null)
			{
				if(sendMsg)
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You are not in a voice channel.");
				return null;
			}
			if(guild == null)
			{
				if(sendMsg && sendAdv)
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} Successfuly joined your voice channel!");
				return await _audio.JoinAsync(context.UsersVC(), textChannel);
			}
			else if(usersVc.Id == guild.VoiceChannel.Id)
			{
				if (sendMsg && sendAdv)
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} We are in the same voice channel already!");
				return guild;
			}
			else if(usersVc.Users.Count > 1 && !user.HasRole("DJ"))
			{
				if (sendMsg)
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You don't have the valid permissions to do this action.");
				return null;
			}
			if (sendMsg && sendAdv)
				await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} Successfuly joined your voice channel!");
			return await _audio.JoinAsync(context.UsersVC(), textChannel);
		}
		public async Task<>
		
	}
}
