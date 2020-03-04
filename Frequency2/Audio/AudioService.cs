using System.Threading.Tasks;
using Discord.Commands;
using Victoria;
using Discord.WebSocket;
using Discord;
using Frequency2.Methods;
using Victoria.Resolvers;
using Victoria.Enums;
using static Frequency2.Methods.MessageMethods;
using System;
using System.Linq;
using static Frequency2.Source.Frequency2Client;
using static Frequency2.Audio.AudioSuccessType;
using static Frequency2.Data.Databases;
using Frequency2.Source;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Frequency2.Types.Messages;
using Victoria.EventArgs;

namespace Frequency2.Audio
{

	public sealed class AudioService
	{
		public static readonly AudioService Instance = new AudioService();
		private readonly ConcurrentDictionary<ulong, GuildMusicConfig> _guildConfigs;
		

		private AudioService()
		{
			_guildConfigs = new ConcurrentDictionary<ulong, GuildMusicConfig>();
		}
		public GuildMusicConfig GetOrAddConfig(ulong guildId)
		{
			return _guildConfigs.GetOrAdd(guildId, x => new GuildMusicConfig());
		}
		public async Task Log(LogMessage arg)
		=> await Logger.Instance.LogAsync(arg);
		public async Task TrackFinished(TrackEndedEventArgs args)
		{
			LavaPlayer player = args.Player;
			LavaTrack ptrack = args.Track;
			TrackEndReason endReason = args.Reason;

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
			var state = LavaClient.GetPlayer(Frequency2Client.Instance._client.GetGuild(Context.Guild.Id)); 
			var user = await Users.GetValue((long)Context.User.Id);
			

			if (state == null && usersChannel != null)
			{
				LavaPlayer player = await LavaClient.JoinAsync(usersChannel, textChannel);
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
				await LavaClient.LeaveAsync(state.VoiceChannel);
				LavaPlayer player = await LavaClient.JoinAsync(usersChannel, textChannel);

				if (sendError && user.SendCompMessage)
					await textChannel.SendMessageAsync(":musical_note: Successfully joined your voice channel!");
				return new Tuple<AudioSuccessType, LavaPlayer>(Successful, player);
			}
		}
		public List<Embed> GetTracks(ulong GuildId)
		{
			var player = LavaClient.GetPlayer(Frequency2Client.Instance._client.GetGuild(GuildId));
			var guild = GetOrAddConfig(GuildId);

			if (player == null)
				return new List<Embed>();
			var tracks = guild.Queue;

			int page = 1;

			List<EmbedBuilder> Embeds = new List<EmbedBuilder>();
			var first = player.Track;
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
						field += $"**{i + 1})** [{track.Title}]({track.Url}) by {track.Author}\n";
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
				field += $"**{i + 1})** [{track.Title}]({track.Url}) by {track.Author}\n";

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
			
			if ((player.PlayerState == PlayerState.Playing && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
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
				Embed.Description = $"[{track.Title}]({track.Url})";


				Embed.AddField("Length", track.Duration.ToString(), true);
				if (guild.Repeat)
					Embed.AddField("Tracks in Queue", (guild.Queue.Count).ToString(), true);
				else
					Embed.AddField("Tracks in Queue", (guild.Queue.Count).ToString(), true);

				Embed.AddField("Next Track", guild.Queue.Count == 0 ? "No tracks" : (guild.Queue[0] as LavaTrack).Title, true);

				Embed.ImageUrl = await track.FetchArtworkAsync();

				await textChannel.SendMessageAsync("", false, Embed.Build());
			}
			return track;
		}
		public async Task<bool> PlayTracksAsync(string url, ICommandContext Context, ITextChannel textChannel, bool sendError = true, bool clear = true, bool shuffled = false)
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

			if ((player.PlayerState == PlayerState.Playing && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return false;
			}
			if (guild.Queue.Count == 0 && !clear && !guild.IsPlayed)
				return await PlayTracksAsync(url, Context, textChannel, sendError, true);

			if (clear)
				guild.Queue.Clear();

			var tracks = new CQueue<LavaTrack>((await LavaClient.SearchAsync(url)).Tracks);
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
						Description = $"[{track.Title}]({track.Url})"
					};

					Embed.AddField("Length", track.Duration.ToString(), true);
					Embed.AddField("Tracks in Queue", (guild.Queue.Count).ToString(), true);
					Embed.AddField("Next Track", guild.Queue.Count == 0 ? "No tracks" : (guild.Queue[0] as LavaTrack).Title, true);

					Embed.ImageUrl = await track.FetchArtworkAsync();

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
				tplayer = new Tuple<AudioSuccessType, LavaPlayer>(new AudioSuccessType(), LavaClient.GetPlayer(Frequency2Client.Instance._client.GetGuild(Context.Guild.Id)));
			LavaPlayer player = tplayer.Item2;
			var guild = GetOrAddConfig(Context.Guild.Id);

			if (tplayer.Item1 != Successful && tplayer.Item1 != SameChannel)
			{
				return null;
			}

			if (guild.Queue.Count == 0 && player.PlayerState != PlayerState.Playing)
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
		public async Task SkipTrackAsync(ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			var player = LavaClient.GetPlayer(Frequency2Client.Instance._client.GetGuild(Context.Guild.Id));
			var user = await Users.GetValue((long)Context.User.Id);
			var guild = GetOrAddConfig(Context.Guild.Id);



			if (player == null)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(17)}");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(17)}");
				return;
			}
			var myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.PlayerState == PlayerState.Playing && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
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
			var player = LavaClient.GetPlayer(Frequency2Client.Instance._client.GetGuild(Context.Guild.Id));
			var user = await Users.GetValue((long)Context.User.Id);
			var guild = GetOrAddConfig(Context.Guild.Id);
			index -= 1;

			if (player == null || player.PlayerState != PlayerState.Playing)
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

			if ((player.PlayerState == PlayerState.Playing && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
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
			var player = LavaClient.GetPlayer(Frequency2Client.Instance._client.GetGuild(Context.Guild.Id));
			var user = await Users.GetValue((long)Context.User.Id);

			if (player == null || player.PlayerState != PlayerState.Playing)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(17)}");
				return;
			}
			var myChannel = player.VoiceChannel as SocketVoiceChannel;


			if (myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ"))
			{
				bool right = true;

				if (player.PlayerState == PlayerState.Playing)
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

			if (player.PlayerState == PlayerState.Paused)
				await player.ResumeAsync();
			else
				await player.PauseAsync();
			if (sendError && user.SendCompMessage)
				await textChannel.SendMessageAsync(":musical_note: Successfully paused/resumed the current song!");
		}
		public async Task<bool> RepeatAsync(IGuild tguild, IGuildUser tuser, ITextChannel textChannel, bool sendError = true)
		{

			var player = LavaClient.GetPlayer(Frequency2Client.Instance._client.GetGuild(tguild.Id));
			var user = await Users.GetValue((long)tuser.Id);



			if (player != null)
			{
				var myChannel = player.VoiceChannel as SocketVoiceChannel;
				if ((player.PlayerState == PlayerState.Playing && myChannel.Users.Count > 2 && !(tuser).ContainsRole("DJ")))
				{
					if (sendError)
						await textChannel.SendMessageAsync($"{tuser.Mention} {GetError(12)}");
					return false;
				}
			}

			var guild = _guildConfigs.GetOrAdd(tguild.Id, new GuildMusicConfig());
			guild.Repeat = !guild.Repeat;
			if (player != null)
			{
				if (guild.Repeat && guild.IsPlayed && guild.Queue.Count == 0)
					guild.IsPlayed = false;
			}

			if (user.SendCompMessage && sendError)
				await textChannel.SendMessageAsync(":musical_note: Toggled repeating for the queue!");
			return true;
		}
		public async Task SetVolume(int volume, ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			var player = LavaClient.GetPlayer(Frequency2Client.Instance._client.GetGuild(Context.Guild.Id));
			var user = await Users.GetValue((long)Context.User.Id);

			if (player == null || player.PlayerState != PlayerState.Playing)
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

			if ((player.PlayerState == PlayerState.Playing && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return;
			}

			await player.UpdateVolumeAsync((ushort)volume);

			if (sendError && user.SendCompMessage)
				await textChannel.SendMessageAsync(":musical_note: Successfully set the volume to " + volume);
		}
		public async Task RemoveSongAtAsync(int index, ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			var player = LavaClient.GetPlayer(Frequency2Client.Instance._client.GetGuild(Context.Guild.Id));
			var user = await Users.GetValue((long)Context.User.Id);
			var guild = GetOrAddConfig(Context.Guild.Id);
			index -= 1;


			if (player == null || player.PlayerState != PlayerState.Playing)
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

			if ((player.PlayerState == PlayerState.Playing && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
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
		public async Task<bool> ShuffleAsync(IGuild tguild, IGuildUser tuser, ITextChannel textChannel, bool sendError = true)
		{
			var player = LavaClient.GetPlayer(tguild);
			var user = await Users.GetValue((long)tuser.Id);
			var guild = _guildConfigs.GetOrAdd(player.VoiceChannel.GuildId, new GuildMusicConfig());

			if (player == null)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{tuser.Mention} {GetError(17)}");
				return false;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{tuser.Mention} {GetError(17)}");
				return false;
			}

			if (guild.Queue.Count < 2)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{tuser.Mention} {GetError(18)}");
			}
			var myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.PlayerState == PlayerState.Playing && myChannel.Users.Count > 2 && !(tuser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{tuser.Mention} {GetError(12)}");
				return false;
			}

			guild.Queue.Shuffle();

			if (sendError && user.SendCompMessage)
				await (textChannel.SendMessageAsync($":musical_note: Successfully shuffled the queue!"));
			return true;
		}
		public async Task<AudioController> SendConfigurationMessageAsync(IGuildUser user, ITextChannel channel)
		{
			var player = LavaClient.GetPlayer(user.Guild);

			if (player == null)
				return null;
			
			var msg = await channel.SendMessageAsync("", false, new EmbedBuilder().WithTitle("How to use").WithDescription(
				":stop_button: - Removes this message\n" +
				":game_die: - Shuffles the queue\n" +
				":repeat: - Toggles repeating in the queue\n" +
				":track_next: - Goes to the next track\n" +
				":arrow_right: - Leaves the current channel\n" +
				":x: - Clears the queue").Build());
			var cont = new AudioController(msg, user, player);
			await cont.PaginateAsync();
			return cont;
		}
		public async Task StopAllAsync(ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			var player = LavaClient.GetPlayer(Frequency2Client.Instance._client.GetGuild(Context.Guild.Id));
			var user = await Users.GetValue((long)Context.User.Id);
			var guild = _guildConfigs.GetOrAdd(player.VoiceChannel.GuildId, new GuildMusicConfig());

			if (player == null)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(17)}");
				return;
			}

			if (player.PlayerState != PlayerState.Playing)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(17)}");
				return;
			}

		
			var myChannel = player.VoiceChannel as SocketVoiceChannel;

			if ((player.PlayerState == PlayerState.Playing && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return;
			}

			guild.Queue.Clear();
			await player.StopAsync();

			if (sendError && user.SendCompMessage)
				await (textChannel.SendMessageAsync($":musical_note: Successfully stopped everything!"));


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
					track = (await LavaClient.SearchAsync(url)).Tracks.FirstOrDefault();
				else
				{
					if (prioritiseSoundcloud)
						track = (await LavaClient.SearchSoundCloudAsync(url)).Tracks.FirstOrDefault();
					else
						track = (await LavaClient.SearchYouTubeAsync(url)).Tracks.FirstOrDefault();
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
