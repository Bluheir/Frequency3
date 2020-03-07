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
using Victoria.Enums;
using Victoria.Responses.Rest;
using System.Linq;
using System.Collections.Concurrent;

namespace Frequency3.Audio
{
	public sealed class AudioService
	{
		private readonly LavaNode _audio;
		private readonly DiscordShardedClient _client;
		private readonly ConcurrentDictionary<ulong, GuildMusicConfig> _guildConfigs;
		private static AudioService? _instance;

		public static AudioService? Instance => _instance;

		public AudioService(LavaConfig config, DiscordShardedClient _client)
		{
			if (_instance != null)
				throw new InvalidOperationException("Cannot create another instance of a singleton");

			_audio = new LavaNode(_client, config);
			_instance = this;
			this._client = _client;
			_guildConfigs = new ConcurrentDictionary<ulong, GuildMusicConfig>();

			_audio.OnTrackEnded += TrackFinish;
			_audio.OnLog += OnLog;
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

		private async Task OnLog(LogMessage arg)
		{
			await Frequency3Client.Instance.Log(arg);
		}
		private async Task TrackFinish(TrackEndedEventArgs arg)
		{
			var player = arg.Player;
			var endReason = arg.Reason;
			var ptrack = arg.Track;

			var guild = GetOrAddConfig(player.VoiceChannel.GuildId);

			if (guild.Repeat) // If the guild enabled repeating, do this
			{
				if (guild.IsPlayed) // If the previous playing track was replaced, do this
				{
					if (endReason != TrackEndReason.Replaced) // If the track wasn't replaced, do this
					{
						guild.IsPlayed = false;
						var track = guild.Queue.Dequeue();
						await player.TextChannel.SendMessageAsync("", false, await EmbedMethods.GetEmbedQueue(track, guild.Queue, ptrack));
						await player.PlayAsync(track);
					}
					else // If it wasn't replaced, don't do anything because it doesn't need to be added at the end of the queue.
						return;
				}
				else
				{
					if (endReason == TrackEndReason.Replaced) // If the track was replaced, declare that it was replaced
					{
						guild.IsPlayed = true;
						guild.Queue.Enqueue(new TrackInput(ptrack, guild.CurrentUserPlaying));
					}
					else
					{
						guild.Queue.Enqueue(new TrackInput(ptrack, guild.CurrentUserPlaying));
						var track = guild.Queue.Dequeue();
						await player.TextChannel.SendMessageAsync("", false, await EmbedMethods.GetEmbedQueue(track, guild.Queue, ptrack));
						await player.PlayAsync(track);
					}

				}
			}
			else // If the guild didn't enable repeating do this
			{
				if (endReason != TrackEndReason.Replaced)
					if (guild.Queue.Count > 0)
					{
						var track = guild.Queue.Dequeue();
                        guild.DecrementUser(guild.CurrentUserPlaying);
						await player.TextChannel.SendMessageAsync("", false, await EmbedMethods.GetEmbedQueue(track, guild.Queue, ptrack));
                        guild.CurrentUserPlaying = track.UserInputter;
						await player.PlayAsync(track);
					}

			}
		}
		public async Task StartAsync()
		{
			await _audio.ConnectAsync();
		}
		public GuildMusicConfig GetOrAddConfig(ulong guildId)
		{
			return _guildConfigs.GetOrAdd(guildId, x => new GuildMusicConfig());
		}
		public async Task<LavaPlayer?> JoinAsync(ICommandContext context, ITextChannel textChannel, bool sendMsg = true, bool sendAdv = true)
		{
			if (context.Guild == null)
				return null; ;
			_audio.TryGetPlayer(context.Guild, out var guild);
			var usersVc = context.UsersVC() as SocketVoiceChannel;
			var user = context.User as IGuildUser;

			if (usersVc == null)
			{
				if (sendMsg)
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You are not in a voice channel.");
				return null;
			}
			if (guild == null)
			{
				if (sendMsg && sendAdv)
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} Successfuly joined your voice channel!");
				return await _audio.JoinAsync(context.UsersVC(), textChannel);
			}
			else if (usersVc.Id == guild.VoiceChannel.Id)
			{
				if (sendMsg && sendAdv)
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} We are in the same voice channel already!");
				return guild;
			}
			else if (usersVc.Users.Count > 1 && !user.HasRole("DJ"))
			{
				if (sendMsg)
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You don't have the valid permissions to do this action.");
				return null;
			}
			if (sendMsg && sendAdv)
				await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} Successfuly joined your voice channel!");
            await _audio.LeaveAsync(guild.VoiceChannel);
			return await _audio.JoinAsync(context.UsersVC(), textChannel);
		}
		public async Task<LavaTrack?> PlayTrackAsync(string song, ICommandContext context, ITextChannel textChannel, bool useSC)
		{
			bool isLink = song.StartsWith("https://youtube.com/watch?v=") ||
			song.StartsWith("youtube.com/watch?v=") ||
			song.StartsWith("soundcloud.com") ||
			song.StartsWith("https://soundcloud.com") ||
			song.StartsWith("https://www.youtube.com/playlist?list=") ||
			song.StartsWith("youtube.com/playlist?list=");

			var usersVc = context.UsersVC() as SocketVoiceChannel;
			var user = context.User as IGuildUser;
			var player = await JoinAsync(context, textChannel, true, false);
			var guild = GetOrAddConfig(context.Guild.Id);

			if (player == null)
				return null;

			if (usersVc.Users.Count > 2 && !user.HasRole("DJ") && player.PlayerState == PlayerState.Playing)
			{
				return await QueueAsync(song, context, textChannel, useSC);

			}
			SearchResponse response;
			if (!isLink)
			{
				if (useSC)
					response = await _audio.SearchSoundCloudAsync(song);
				else
					response = await _audio.SearchYouTubeAsync(song);
			}
			else
			{
				response = await _audio.SearchAsync(song);
			}

			if (response.Tracks.Count == 0)
			{
				await textChannel.SendMessageAsync(":musical_note: Cannot find the song :(.");
				return null;
			}
			if (response.Tracks.Count > 1 && (isLink && song.Contains("playlist?list=")))
			{
                var tracks = new List<TrackInput>();
                foreach(var item in response.Tracks)
                {
                    tracks.Add(new TrackInput(item, context.User.Id));
                }
				for (int i = 1; i < tracks.Count; i++)
				{
					guild.Queue.Enqueue(tracks[i]);
				}
			}
			var track = response.Tracks.FirstOrDefault();
            await player.PlayAsync(track);

            guild.CurrentUserPlaying = context.User.Id;

			{
				EmbedBuilder Embed = new EmbedBuilder();

				Embed.Title = "Playing Song";
				Embed.Description = $"[{track.Title}]({track.Url})";


				Embed.AddField("Length", track.Duration.ToString(), true);
				if (guild.Repeat)
					Embed.AddField("Tracks in Queue", (guild.Queue.Count).ToString(), true);
				else
					Embed.AddField("Tracks in Queue", (guild.Queue.Count).ToString(), true);

				Embed.AddField("Next Track", guild.Queue.Count == 0 ? "No tracks" : (guild.Queue[0].Track as LavaTrack).Title, true);

				Embed.ImageUrl = await track.FetchArtworkAsync();

				await textChannel.SendMessageAsync("", false, Embed.Build());
			}
			return track;
		}
		public async Task<LavaTrack?> QueueAsync(string song, ICommandContext context, ITextChannel textChannel, bool useSc)
		{
			bool isLink = song.StartsWith("https://youtube.com/watch?v=") ||
			song.StartsWith("youtube.com/watch?v=") ||
			song.StartsWith("soundcloud.com") ||
			song.StartsWith("https://soundcloud.com") ||
			song.StartsWith("https://www.youtube.com/playlist?list=") ||
			song.StartsWith("youtube.com/playlist?list=");

			var guild = GetOrAddConfig(context.Guild.Id);
			var player = await JoinAsync(context, textChannel, true, false);

			if (player == null || player.PlayerState != PlayerState.Playing)
			{
				return await PlayTrackAsync(song, context, textChannel, useSc);
			}


            if (guild.GetUserInputs(context.User.Id) == guild.MaxQueueTimes && !(context.User as SocketGuildUser).HasRole("DJ"))
            {
                await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You cannot add any more music to the queue.");
                return null;
            }

            SearchResponse response;
			if (!isLink)
			{
				if (useSc)
					response = await _audio.SearchSoundCloudAsync(song);
				else
					response = await _audio.SearchYouTubeAsync(song);
			}
			else
			{
				response = await _audio.SearchAsync(song);
			}

			if (response.Tracks.Count == 0)
			{
				await textChannel.SendMessageAsync(":musical_note: Cannot find the song :(.");
				return null;
			}
            
			LavaTrack track;
            guild.IncrementUser(context.User.Id);
			guild.Queue.Enqueue(new TrackInput(track = response.Tracks.FirstOrDefault(), context.User.Id));
			if (response.Tracks.Count > 1 && (isLink && song.Contains("playlist?list=")) && (context.User as IGuildUser).HasRole("DJ"))
			{
				for (int i = 1; i < response.Tracks.Count; i++)
				{
					guild.Queue.Enqueue(new TrackInput(response.Tracks[i], context.User.Id));
				}
			}

			{
				EmbedBuilder Embed = new EmbedBuilder
				{
					Title = "Enqueued Song",
					Description = $"[{track.Title}]({track.Url})"
				};

				Embed.AddField("Length", track.Duration.ToString(), true);
				Embed.AddField("Tracks in Queue", (guild.Queue.Count).ToString(), true);

				Embed.AddField("Progress of current song", player.Track.Position.ToString(), true);
				Embed.AddField("Length of current song", player.Track.Duration.ToString(), true);

				Embed.ImageUrl = await track.FetchArtworkAsync();

				await textChannel.SendMessageAsync("", false, Embed.Build());
			}
			return track;
		}
		public async Task LeaveAsync(ICommandContext context, ITextChannel textChannel)
		{
			var usersVc = context.UsersVC() as SocketVoiceChannel;
			var user = context.User as SocketGuildUser;
			_audio.TryGetPlayer(context.Guild, out var guild);


			if (guild == null)
			{
				await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} I am not in a voice channel.");
				return;
			}
			else if (usersVc == null || usersVc.Id != guild.VoiceChannel.Id)
			{
				if ((guild.VoiceChannel as SocketVoiceChannel).Users.Count > 1 && !user.HasRole("DJ"))
				{
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You don't have the valid permissions to do this action.");
					return;
				}
			}
			else if (usersVc.Id == guild.VoiceChannel.Id)
			{
				if ((guild.VoiceChannel as SocketVoiceChannel).Users.Count > 2 && !user.HasRole("DJ"))
				{
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You don't have the valid permissions to do this action.");
					return;
				}
			}
			await _audio.LeaveAsync(guild.VoiceChannel);
            await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} Successfully left the voice channel.");
        }
		public async Task SkipAsync(ICommandContext context, ITextChannel textChannel)
		{
			var usersVc = context.UsersVC() as SocketVoiceChannel;
			var user = context.User as SocketGuildUser;
			_audio.TryGetPlayer(context.Guild, out var guild);


			if (guild == null)
			{
				await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} I am not in a voice channel.");
				return;
			}
			else if (guild.PlayerState != PlayerState.Playing)
			{
				await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} I am not playing anything");
			}
			else if (usersVc == null || usersVc.Id != guild.VoiceChannel.Id)
			{
				if ((guild.VoiceChannel as SocketVoiceChannel).Users.Count > 1 && !user.HasRole("DJ"))
				{
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You don't have the valid permissions to do this action.");
					return;
				}
			}
			else if (usersVc.Id == guild.VoiceChannel.Id)
			{
				if ((guild.VoiceChannel as SocketVoiceChannel).Users.Count > 2 && !user.HasRole("DJ"))
				{
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You don't have the valid permissions to do this action.");
					return;
				}
			}
			await guild.StopAsync();
		}
		public async Task ShuffleAsync(ICommandContext context, ITextChannel textChannel)
		{
			var usersVc = context.UsersVC() as SocketVoiceChannel;
			var user = context.User as SocketGuildUser;
			_audio.TryGetPlayer(context.Guild, out var guild);
			var gg = GetOrAddConfig(context.Guild.Id);


			if (guild == null)
			{
				await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} I am not in a voice channel.");
				return;
			}
			else if (guild.PlayerState != PlayerState.Playing)
			{
				await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} I am not playing anything");
			}
			else if (usersVc == null || usersVc.Id != guild.VoiceChannel.Id)
			{
				if ((guild.VoiceChannel as SocketVoiceChannel).Users.Count > 1 && !user.HasRole("DJ"))
				{
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You don't have the valid permissions to do this action.");
					return;
				}
			}
			else if (usersVc.Id == guild.VoiceChannel.Id)
			{
				if ((guild.VoiceChannel as SocketVoiceChannel).Users.Count > 2 && !user.HasRole("DJ"))
				{
					await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You don't have the valid permissions to do this action.");
					return;
				}
			}
			gg.Queue.Shuffle();
            await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} Successfully shuffled the queue.");
        }
        public async Task SetQueueLimitAsync(ICommandContext context, ITextChannel textChannel, uint limit)
        {
            var user = context.User as SocketGuildUser;
            _audio.TryGetPlayer(context.Guild, out var guild);
            var p = GetOrAddConfig(context.Guild.Id);

            if (guild == null)
            {
                await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} I am not in a voice channel.");
                return;
            }
            else if (!user.HasRole("DJ"))
            {
                await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} You don't have the valid permissions to do this.");
                return;
            }
            p.MaxQueueTimes = (int)limit;
            await textChannel.SendMessageAsync($":musical_note: {context.User.Mention} Successfully changed the max amount of tracks a person can add to the queue to {limit}.");
        }
		
		
	}
}
