using System.Threading.Tasks;
using Discord.Commands;
using Victoria;
using Discord.WebSocket;
using Discord;
using Frequency2.Methods;
using Victoria.Entities;
using static Frequency2.Methods.MessageMethods;
using System;
using System.Linq;
using static Frequency2.Source.Frequency2Client;
using static Frequency2.Audio.AudioSuccessType;
using static Frequency2.Data.Databases;
using Frequency2.Source;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Frequency2.Audio
{
	public sealed class AudioService
	{
		public static readonly AudioService Instance = new AudioService();

		private AudioService()
		{
			_guildConfigs = new ConcurrentDictionary<ulong, GuildMusicConfig>();
		}

		private readonly ConcurrentDictionary<ulong, GuildMusicConfig> _guildConfigs;
		internal static LavaRestClient LavaRestClient { get; } = new LavaRestClient(
			Config.Configuration.Config.LavaLinkSettings.Host,
			Config.Configuration.Config.LavaLinkSettings.Port,
			Config.Configuration.Config.LavaLinkSettings.Password
		);

		public async Task Log(LogMessage arg)
		=> await Logger.Instance.LogAsync(arg);
		public async Task TrackFinished(LavaPlayer player, LavaTrack ptrack, TrackEndReason endReason)
		{
			var guild = _guildConfigs.GetOrAdd(player.VoiceChannel.GuildId, new GuildMusicConfig());


			guild.IsPlayed = endReason == TrackEndReason.Replaced;

			LavaTrack track = null;

			if (player.Queue.Count == 0 && !guild.Repeat)
			{
				if (guild.IsPlayed)
					guild.IsPlayed = false;
				return;
			}

			if (guild.Repeat && player.Queue.Count == 0 && !guild.IsPlayed)
			{

				track = ptrack;
				await player.TextChannel.SendMessageAsync("", false, await EmbedMethods.GetEmbedQueue(track, player, ptrack));
				await player.PlayAsync(track);
				return;

			}
			
			if(guild.Repeat)
				player.Queue.Enqueue(ptrack);
			if(!guild.IsPlayed)
				track = player.Queue.Dequeue() as LavaTrack;

			if (endReason != TrackEndReason.Replaced/* && !guild.IsPlayed)//*/)
			{
				await player.TextChannel.SendMessageAsync("", false, await EmbedMethods.GetEmbedQueue(track, player, ptrack));
				await player.PlayAsync(track);
			}
		}



		/// <summary>
		/// Joins the text channel of the current user
		/// </summary>
		/// <param name="Context">The context</param>
		/// <param name="textChannel">The text channel to add</param>
		/// <returns>Returns the Lavaplayer after</returns>
		public async Task<Tuple<AudioSuccessType, LavaPlayer>> JoinAsync(ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			SocketVoiceChannel usersChannel = (Context.User as IVoiceState).VoiceChannel as SocketVoiceChannel;
			var state = LavaClient.GetPlayer(Context.Guild.Id);

			if (state != null && usersChannel == null)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return new Tuple<AudioSuccessType, LavaPlayer>(Inchannel, state);
			}
			if (usersChannel == null)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return new Tuple<AudioSuccessType, LavaPlayer>(Error, null);
			}
			var user = await Users.GetValue((long)Context.User.Id);
			if (state == null)
			{
				
				LavaPlayer player = await LavaClient.ConnectAsync(usersChannel, textChannel);
				if(sendError && user.SendCompMessage)
					await textChannel.SendMessageAsync(":musical_note: Successfully joined your voice channel!");
				return new Tuple<AudioSuccessType, LavaPlayer>(Successful, player);
			}

			SocketVoiceChannel myChannel = state.VoiceChannel as SocketVoiceChannel;

			if (myChannel == usersChannel)
				return new Tuple<AudioSuccessType, LavaPlayer>(Successful, state);

			if (!(myChannel.Users.Count > 1 && (Context.User as IGuildUser).ContainsRole("DJ")) && !(myChannel.Users.Count == 1))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return new Tuple<AudioSuccessType, LavaPlayer>(Inchannel, LavaClient.GetPlayer(Context.Guild.Id));
			}
			
			await LavaClient.DisconnectAsync(usersChannel);
			if(sendError && user.SendCompMessage)
				await textChannel.SendMessageAsync(":musical_note: Successfully joined your voice channel!");
			return new Tuple<AudioSuccessType, LavaPlayer>(Successful, await LavaClient.ConnectAsync(usersChannel, textChannel));
		}

		public async Task<LavaTrack> PlayAsync(string song, ICommandContext Context, ITextChannel textChannel, bool sendError = true, bool prioritiseSoundcloud = false)
		{
			var tplayer = await JoinAsync(Context, textChannel, false);
			var player = tplayer.Item2;

			if (tplayer.Item1 == Error)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return null;
			}
			else if(tplayer.Item1 == Inchannel)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return null;
			}

			SocketVoiceChannel myChannel = player.VoiceChannel as SocketVoiceChannel;
			
			if((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				return await QueueAsync(song, Context, textChannel, sendError, prioritiseSoundcloud);
			}

			var track = await GetTrack(song, prioritiseSoundcloud);

			await player.PlayAsync(track);
			

			var guild = _guildConfigs.GetOrAdd(player.VoiceChannel.GuildId, new GuildMusicConfig());
			
			if(sendError)
			{
				EmbedBuilder Embed = new EmbedBuilder();

				Embed.Title = "Playing Song";
				Embed.Description = $"[{track.Title}]({track.Uri.AbsoluteUri})";

				
				Embed.AddField("Length", track.Length.ToString(), true);
				if(guild.Repeat)
					Embed.AddField("Tracks in Queue", (player.Queue.Count + 1).ToString(), true);
				else
					Embed.AddField("Tracks in Queue", (player.Queue.Count).ToString(), true);

				Embed.AddField("Next Track", player.Queue.Count == 0 ? "No tracks" : (player.Queue.Peek() as LavaTrack).Title, true);

				Embed.ImageUrl = await track.FetchThumbnailAsync();

				await textChannel.SendMessageAsync("", false, Embed.Build());
			}
			return track;
		}
		
		
		public async Task<bool> PlayTracksAsync(string url, 
			ICommandContext Context, 
			ITextChannel textChannel, 
			bool sendError = true, 
			bool clear = true)
		{
			var tplayer = await JoinAsync(Context, textChannel, false);
			var player = tplayer.Item2;
			var guild = _guildConfigs.GetOrAdd(player.VoiceChannel.GuildId, new GuildMusicConfig());

			if (tplayer.Item1 == Error)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return false;
			}
			else if(tplayer.Item1 == Inchannel)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return false;
			}

			SocketVoiceChannel myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return false;
			}
			if (player.Queue.Count == 0 && !clear && !guild.IsPlayed)
				return await PlayTracksAsync(url, Context, textChannel, sendError, true);
				
			if (clear)
				player.Queue.Clear();

			var tracks = (await LavaRestClient.SearchTracksAsync(url)).Tracks;
			foreach (var track in tracks)
			{
				if (track == tracks.FirstOrDefault())
					continue;
				player.Queue.Enqueue(track);
			}

			if (clear)
			{
				var track = tracks.FirstOrDefault();
				if (sendError)
				{
					EmbedBuilder Embed = new EmbedBuilder
					{
						Title = "Playing Song",
						Description = $"[{track.Title}]({track.Uri.AbsoluteUri})"
					};

					Embed.AddField("Length", track.Length.ToString(), true);
					Embed.AddField("Tracks in Queue", (player.Queue.Count-1).ToString(), true);
					Embed.AddField("Next Track", player.Queue.Count == 0 ? "No tracks" : (player.Queue.Peek() as LavaTrack).Title, true);

					Embed.ImageUrl = await track.FetchThumbnailAsync();

					await textChannel.SendMessageAsync("", false, Embed.Build());
				}
				await player.PlayAsync(track);
			}
			return true;
		}

		public async Task<LavaTrack> QueueAsync(String url, ICommandContext Context, ITextChannel textChannel, bool sendError = true, bool prioritiseSoundcloud = false)
		{
			var tplayer = await JoinAsync(Context, textChannel, false);
			LavaPlayer player = tplayer.Item2;

			if(tplayer.Item1 == Error)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return null;
			}
			else if(tplayer.Item1 == Inchannel)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return null;
			}
			if(player.Queue.Count == 0 && !player.IsPlaying)
			{
				var ltrack = await PlayAsync(url, Context, textChannel, sendError, prioritiseSoundcloud);
				return ltrack;
			}
			var track = await GetTrack(url, prioritiseSoundcloud);
			
			player.Queue.Enqueue(track);
			if (sendError)
			{
				EmbedBuilder Embed = new EmbedBuilder
				{
					Title = "Enqueued Song",
					Description = $"[{track.Title}]({track.Uri.AbsoluteUri})"
				};

				Embed.AddField("Length", track.Length.ToString(), true);
				Embed.AddField("Tracks in Queue", (player.Queue.Count).ToString(), true);
				
				Embed.AddField("Progress of current song", player.CurrentTrack.Position.ToString(), true);
				Embed.AddField("Length of current song", player.CurrentTrack.Length.ToString(), true);

				Embed.ImageUrl = await track.FetchThumbnailAsync();

				await textChannel.SendMessageAsync("", false, Embed.Build());
			}
			return track;
		}
		public async Task SkipTrackAsync(ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			var player = LavaClient.GetPlayer(Context.Guild.Id);
			var user = await Users.GetValue((long)Context.User.Id);
			

			if (player == null)
			{
				if(sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(17)}");
				return;
			}
			
			if (!player.IsPlaying)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(17)}");
				return;
			}
			var myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return;
			}


			if (player.Queue.Count < 1)
			{
				await player.StopAsync();
				if (sendError && user.SendCompMessage)
					await textChannel.SendMessageAsync(":musical_note: Successfully skipped the current track!");
			}
			else
			{
				await player.StopAsync();
			}
		}
		public async Task PauseAsync(ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			var player = LavaClient.GetPlayer(Context.Guild.Id);
			var user = await Users.GetValue((long)Context.User.Id);

			if (player == null || !player.IsPlaying)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(17)}");
				return;
			}
			var myChannel = player.VoiceChannel as SocketVoiceChannel;


			if (myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ"))
			{
				bool right = true;

				if (player.IsPaused)
				{
					foreach (var users in myChannel.Users)
					{
						if (users.Id == Context.Client.CurrentUser.Id)
							continue;
						if (users.ContainsRole("DJ"))
						{
							right = false;
							break;
						}
					}
				}
				else
					right = false;
				if (!right)
				{
					if (sendError)
						await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");

					return;
				}
			}

			if (player.IsPaused)
				await player.ResumeAsync();
			else
				await player.PauseAsync();
			if (sendError && user.SendCompMessage)
				await textChannel.SendMessageAsync(":musical_note: Successfully paused/resumed the current song!");
		}
		public async Task RepeatAsync(ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{

			var player = LavaClient.GetPlayer(Context.Guild.Id);
			var user = await Users.GetValue((long)Context.User.Id);

			

			if (player != null)
			{
				var myChannel = player.VoiceChannel as SocketVoiceChannel;
				if ((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
				{
					if (sendError)
						await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
					return;
				}
			}

			var guild = _guildConfigs.GetOrAdd(Context.Guild.Id, new GuildMusicConfig());
			guild.Repeat = !guild.Repeat;
			if (player != null)
			{
				if (guild.Repeat && guild.IsPlayed && player.Queue.Count == 0)
					guild.IsPlayed = false;
			}
			
			if (user.SendCompMessage && sendError)
				await textChannel.SendMessageAsync(":musical_note: Toggled repeating for the queue!");
		}

		public async Task ShuffleAsync(ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			var player = LavaClient.GetPlayer(Context.Guild.Id);
			var user = await Users.GetValue((long)Context.User.Id);
			var guild = _guildConfigs.GetOrAdd(player.VoiceChannel.GuildId, new GuildMusicConfig());

			if (player == null)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(17)}");
				return;
			}

			if (!player.IsPlaying)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(17)}");
				return;
			}

			if(player.Queue.Count < 2)
			{
				
			}
			var myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return;
			}


			
		}

		public async Task<LavaTrack> GetTrack(string url, bool prioritiseSoundcloud = false)
		{
			try
			{
				LavaTrack track = null;

				if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
					track = (await LavaRestClient.SearchTracksAsync(url)).Tracks.FirstOrDefault();
				else
				{
					if (prioritiseSoundcloud)
						track = (await LavaRestClient.SearchSoundcloudAsync(url)).Tracks.FirstOrDefault();
					else
						track = (await LavaRestClient.SearchYouTubeAsync(url)).Tracks.FirstOrDefault();
				}

				return track;
			}
			catch
			{
				return null;
			}
		}

	}
}
