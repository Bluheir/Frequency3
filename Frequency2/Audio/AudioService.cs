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
		private readonly ConcurrentDictionary<ulong, GuildMusicConfig> _guildConfigs;
		internal static LavaRestClient LavaRestClient = new LavaRestClient(
			Config.Configuration.Config.LavaLinkSettings.Host,
			Config.Configuration.Config.LavaLinkSettings.Port,
			Config.Configuration.Config.LavaLinkSettings.Password
		);

		private AudioService()
		{
			_guildConfigs = new ConcurrentDictionary<ulong, GuildMusicConfig>();
		}
		private GuildMusicConfig GetOrAddConfig(ulong id)
		{
			return _guildConfigs.GetOrAdd(id, x => new GuildMusicConfig());
		}
		public async Task Log(LogMessage arg)
		=> await Logger.Instance.LogAsync(arg);
		public async Task TrackFinished(LavaPlayer player, LavaTrack ptrack, TrackEndReason endReason)
		{
			var guild = GetOrAddConfig(player.VoiceChannel.GuildId);

			if(guild.Repeat)
			{ 
				if(guild.IsPlayed)
				{
					if (endReason != TrackEndReason.Replaced)
					{
						guild.IsPlayed = false;
						var track = guild.Queue.Dequeue();
						await player.TextChannel.SendMessageAsync("", false, await EmbedMethods.GetEmbedQueue(track, guild.Queue, ptrack));
						await player.PlayAsync(track);
					}
					else
						return;
				}
				else
				{
					if (endReason == TrackEndReason.Replaced)
					{
						guild.IsPlayed = true;
						guild.Queue.Enqueue(ptrack);
					}
					else
					{
						guild.Queue.Enqueue(ptrack);
						var track = guild.Queue.Dequeue();
						await player.TextChannel.SendMessageAsync("", false, await EmbedMethods.GetEmbedQueue(track, guild.Queue, ptrack));
						await player.PlayAsync(track);
					}

				}
			}
			else
			{
				if (endReason != TrackEndReason.Replaced)
					if (guild.Queue.Count > 0)
					{
						var track = guild.Queue.Dequeue();
						await player.TextChannel.SendMessageAsync("", false, await EmbedMethods.GetEmbedQueue(track, guild.Queue, ptrack));
						await player.PlayAsync(track);
					}
				
			}
		
		}
		public async Task<Tuple<AudioSuccessType, LavaPlayer>> JoinAsync(ICommandContext Context, ITextChannel textChannel, bool sendError = true, bool sendPickyError = true)
		{
			SocketVoiceChannel usersChannel = (Context.User as IVoiceState).VoiceChannel as SocketVoiceChannel;
			var state = LavaClient.GetPlayer(Context.Guild.Id);
			var user = await Users.GetValue((long)Context.User.Id);
			

			if (state == null && usersChannel != null)
			{
				LavaPlayer player = await LavaClient.ConnectAsync(usersChannel, textChannel);
				if (sendError && user.SendCompMessage)
					await textChannel.SendMessageAsync(":musical_note: Successfully joined your voice channel!");
				return new Tuple<AudioSuccessType, LavaPlayer>(Successful, player);
			}
			if(state == null)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return new Tuple<AudioSuccessType, LavaPlayer>(UserNotInChannel, null);
			}
			if(usersChannel == null)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return new Tuple<AudioSuccessType, LavaPlayer>(AudioSuccessType.Inchannel, state);
			}
			if(state.VoiceChannel.Id == usersChannel.Id)
			{
				if (sendError && sendPickyError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return new Tuple<AudioSuccessType, LavaPlayer>(AudioSuccessType.SameChannel, state);
			}
			var usersInChannel = await (state.VoiceChannel.GetUsersAsync()).ToList();
			if(usersInChannel.Count > 1 && !HasRole(Context.User as SocketGuildUser))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return new Tuple<AudioSuccessType, LavaPlayer>(AudioSuccessType.InvalidPerms, state);
			}
			{
				await LavaClient.DisconnectAsync(state.VoiceChannel);
				LavaPlayer player = await LavaClient.ConnectAsync(usersChannel, textChannel);

				if (sendError && user.SendCompMessage)
					await textChannel.SendMessageAsync(":musical_note: Successfully joined your voice channel!");
				return new Tuple<AudioSuccessType, LavaPlayer>(Successful, player);
			}
		}
		public List<Embed> GetTracks(ulong GuildId)
		{
			var player = LavaClient.GetPlayer(GuildId);
			var guild = GetOrAddConfig(GuildId);

			if (player == null)
				return new List<Embed>();
			var tracks = guild.Queue;

			int page = 1;

			List<EmbedBuilder> Embeds = new List<EmbedBuilder>();
			var first = player.CurrentTrack;
			string field = null;


			for (int i = 0; i < tracks.Count; i++)
			{
				var track = tracks[i];
				if (i % 7 == 0 || i == tracks.Count - 1)
				{
					if (page != 1)
						Embeds[Embeds.Count - 1].Fields[0].Value = field;

					if (i == tracks.Count - 1)
					{
						field += $"**{i + 1})** [{track.Title}]({track.Uri.AbsoluteUri}) by {track.Author}\n";
						Embeds[Embeds.Count - 1].Fields[0].Value = field;
						break;
					}
					Embeds.Add(new EmbedBuilder()
					{
						Title = $"{tracks.Count} Tracks\nPage {page} out of ",
						Fields = new List<EmbedFieldBuilder>
						{
							new EmbedFieldBuilder()
							{
								Name = "Tracks:",
							}
						},
						Description = $"Current Track: {first.Title} by {first.Author}"
					});
					field = "";
					page++;
				}
				field += $"**{i + 1})** [{track.Title}]({track.Uri.AbsoluteUri}) by {track.Author}\n";

			}
			page--;
			return Embeds.Select(x =>
			{
				x.Title += page;
				return x.Build();
			}).ToList();
		}
		public async Task<LavaTrack> PlayAsync(string song, ICommandContext Context, ITextChannel textChannel, bool sendError = true, bool prioritiseSoundcloud = false) 
		{
			var tplayer = await JoinAsync(Context, textChannel, true, false);
			var player = tplayer.Item2;

			if (tplayer.Item1 != Successful && tplayer.Item1 != SameChannel)
			{
				return null;
			}

			SocketVoiceChannel myChannel = player.VoiceChannel as SocketVoiceChannel;
			
			if ((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				return await QueueAsync(song, Context, textChannel, sendError, prioritiseSoundcloud, false);
			}

			var track = await GetTrack(song, prioritiseSoundcloud);

			await player.PlayAsync(track);


			var guild = _guildConfigs.GetOrAdd(player.VoiceChannel.GuildId, new GuildMusicConfig());

			if (sendError)
			{
				EmbedBuilder Embed = new EmbedBuilder();

				Embed.Title = "Playing Song";
				Embed.Description = $"[{track.Title}]({track.Uri.AbsoluteUri})";


				Embed.AddField("Length", track.Length.ToString(), true);
				if (guild.Repeat)
					Embed.AddField("Tracks in Queue", (guild.Queue.Count).ToString(), true);
				else
					Embed.AddField("Tracks in Queue", (guild.Queue.Count).ToString(), true);

				Embed.AddField("Next Track", guild.Queue.Count == 0 ? "No tracks" : (guild.Queue[0] as LavaTrack).Title, true);

				Embed.ImageUrl = await track.FetchThumbnailAsync();

				await textChannel.SendMessageAsync("", false, Embed.Build());
			}
			return track;
		}


		public async Task<bool> PlayTracksAsync(string url,
			ICommandContext Context,
			ITextChannel textChannel,
			bool sendError = true,
			bool clear = true, bool shuffled = false)
		{
			var tplayer = await JoinAsync(Context, textChannel, true, false);
			var player = tplayer.Item2;
			var guild = _guildConfigs.GetOrAdd(player.VoiceChannel.GuildId, new GuildMusicConfig());

			if (tplayer.Item1 != Successful && tplayer.Item1 != SameChannel)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return false;
			}
	

			SocketVoiceChannel myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return false;
			}
			if (guild.Queue.Count == 0 && !clear && !guild.IsPlayed)
				return await PlayTracksAsync(url, Context, textChannel, sendError, true);

			if (clear)
				guild.Queue.Clear();

			var tracks = new CQueue<LavaTrack>((await LavaRestClient.SearchTracksAsync(url, true)).Tracks);
			if(shuffled)
			{
				tracks.Shuffle();
			}
			foreach (var track in tracks)
			{
				if (track == tracks.FirstOrDefault())
					continue;
				guild.Queue.Enqueue(track);
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
					Embed.AddField("Tracks in Queue", (guild.Queue.Count).ToString(), true);
					Embed.AddField("Next Track", guild.Queue.Count == 0 ? "No tracks" : (guild.Queue[0] as LavaTrack).Title, true);

					Embed.ImageUrl = await track.FetchThumbnailAsync();

					await textChannel.SendMessageAsync("", false, Embed.Build());
				}
				await player.PlayAsync(track);
			}
			return true;
		}

		public async Task<LavaTrack> QueueAsync(String url, ICommandContext Context, ITextChannel textChannel, bool sendError = true, bool prioritiseSoundcloud = false, bool joinVC = true)
		{
			Tuple<AudioSuccessType, LavaPlayer> tplayer;
			if (joinVC)
			{
				tplayer = await JoinAsync(Context, textChannel, true, false);
			}
			else
				tplayer = new Tuple<AudioSuccessType, LavaPlayer>(new AudioSuccessType(), LavaClient.GetPlayer(Context.Guild.Id));
			LavaPlayer player = tplayer.Item2;
			var guild = GetOrAddConfig(Context.Guild.Id);

			if (tplayer.Item1 != Successful && tplayer.Item1 != SameChannel)
			{
				return null;
			}

			if (guild.Queue.Count == 0 && !player.IsPlaying)
			{
				var ltrack = await PlayAsync(url, Context, textChannel, sendError, prioritiseSoundcloud);
				return ltrack;
			}
			var track = await GetTrack(url, prioritiseSoundcloud);

			guild.Queue.Enqueue(track);
			if (sendError)
			{
				EmbedBuilder Embed = new EmbedBuilder
				{
					Title = "Enqueued Song",
					Description = $"[{track.Title}]({track.Uri.AbsoluteUri})"
				};

				Embed.AddField("Length", track.Length.ToString(), true);
				Embed.AddField("Tracks in Queue", (guild.Queue.Count).ToString(), true);

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
			var guild = GetOrAddConfig(Context.Guild.Id);



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
			var myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return;
			}
			



			if (guild.Queue.Count < 1)
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
		public async Task InsertSongAsync(string song, int index, ICommandContext Context, ITextChannel textChannel, bool sendError = true, bool prioritiseSoundcloud = false)
		{
			var player = LavaClient.GetPlayer(Context.Guild.Id);
			var user = await Users.GetValue((long)Context.User.Id);
			var guild = GetOrAddConfig(Context.Guild.Id);
			index -= 1;

			if (player == null || !player.IsPlaying)
			{
				await PlayAsync(song, Context, textChannel, sendError, prioritiseSoundcloud);
				return;
			}
			if (player == null)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(17)}");
				return;
			}

			if (index >= guild.Queue.Count)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} :x: That index is out of range");
			}
			
			var myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return;
			}
			var track = await GetTrack(song, prioritiseSoundcloud);
			guild.Queue.Insert(index, track);

			if(user.SendCompMessage && sendError)
			await textChannel.SendMessageAsync($":musical_note: Successfully inserted song `{track.Title}` at index {index} of the queue!");

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
				if (guild.Repeat && guild.IsPlayed && guild.Queue.Count == 0)
					guild.IsPlayed = false;
			}

			if (user.SendCompMessage && sendError)
				await textChannel.SendMessageAsync(":musical_note: Toggled repeating for the queue!");
		}
		public async Task SetVolume(int volume, ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			var player = LavaClient.GetPlayer(Context.Guild.Id);
			var user = await Users.GetValue((long)Context.User.Id);

			if (player == null || !player.IsPlaying)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} :x: There is no song playing");
				return;
			}
		
			if(volume > 100 || volume < 0)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} :x: Please type in a proper volume number");
			}
			var myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return;
			}

			await player.SetVolumeAsync(volume);

			if (sendError && user.SendCompMessage)
				await textChannel.SendMessageAsync(":musical_note: Successfully set the volume to " + volume);
		}
		public async Task RemoveSongAtAsync(int index, ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			var player = LavaClient.GetPlayer(Context.Guild.Id);
			var user = await Users.GetValue((long)Context.User.Id);
			var guild = GetOrAddConfig(Context.Guild.Id);
			index -= 1;


			if (player == null || !player.IsPlaying)
			{
				if(sendError)
				{
					await textChannel.SendMessageAsync($"{Context.User.Mention} :x: There are no songs playing");
				}
				return;
			}
			

			if (index >= guild.Queue.Count)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} :x: That index is out of range");
			}

			var myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return;
			}

			var track = guild.Queue[index];
			guild.Queue.RemoveAt(index);

			if (user.SendCompMessage && sendError)
				await textChannel.SendMessageAsync($":musical_note: Successfully removed song `{track.Title}` at index {index} of the queue!");
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

			if (guild.Queue.Count < 2)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(18)}");
			}
			var myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return;
			}

			guild.Queue.Shuffle();

			if (sendError && user.SendCompMessage)
				await (textChannel.SendMessageAsync($":musical_note: Successfully shuffled the queue!"));


		}

		private bool HasRole(SocketGuildUser user, string role = "DJ")
		{
			return user.Roles.Where(x => x.Name == role).FirstOrDefault() != null;
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
