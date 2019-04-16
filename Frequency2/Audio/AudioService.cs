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
using Frequency2.Source;

namespace Frequency2.Audio
{
	public sealed class AudioService
	{
		public static readonly AudioService Instance = new AudioService();

		

		private AudioService()
		{
		}

		internal static LavaRestClient LavaRestClient { get; } = new LavaRestClient();

		public async Task Log(LogMessage arg)
		=> await Logger.Instance.LogAsync(arg);
		public async Task TrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason endReason)
		{
			player.Queue.Dequeue();
			if (player.Queue.Count > 0)
			{
				await player.PlayAsync(player.Queue.Peek() as LavaTrack);
			}
			
		}

		

		/// <summary>
		/// Joins the text channel of the current user
		/// </summary>
		/// <param name="Context">The context</param>
		/// <param name="textChannel">The text channel to add</param>
		/// <returns>Returns the Lavaplayer after</returns>
		public async Task<Tuple<bool, LavaPlayer>> JoinAsync(ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			SocketVoiceChannel usersChannel = (Context.User as IVoiceState).VoiceChannel as SocketVoiceChannel;
			var state = (Context.Client.CurrentUser as IVoiceState);
			if (state != null && usersChannel == null)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return new Tuple<bool, LavaPlayer>(true, LavaClient.GetPlayer(Context.Guild.Id));
			}
			if (usersChannel == null)
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return new Tuple<bool, LavaPlayer>(false, null);
			}
			if (state == null)
			{
				LavaPlayer player = await LavaClient.ConnectAsync(usersChannel);
				return new Tuple<bool, LavaPlayer>(true, player);
			}
			SocketVoiceChannel myChannel = state.VoiceChannel as SocketVoiceChannel;
			if (Context.Channel == usersChannel)
				return new Tuple<bool, LavaPlayer>(true, LavaClient.GetPlayer(Context.Guild.Id));
			if (!(myChannel.Users.Count > 1 && (Context.User as IGuildUser).ContainsRole("DJ")) || !(myChannel.Users.Count == 1))
			{
				if (sendError)
					await textChannel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				return new Tuple<bool, LavaPlayer>(true, LavaClient.GetPlayer(Context.Guild.Id));
			}
			return new Tuple<bool, LavaPlayer>(false, await LavaClient.ConnectAsync(usersChannel, textChannel));
		}

		public async Task<LavaTrack> PlayAsync(string song, ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			var tplayer = await JoinAsync(Context, textChannel, false);
			var player = tplayer.Item2;

			if (tplayer.Item1 == false)
			{
				if (sendError)
					await Context.Channel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return null;
			}
			SocketVoiceChannel usersChannel = (Context.User as IVoiceState).VoiceChannel as SocketVoiceChannel;
			SocketVoiceChannel myChannel = player.VoiceChannel as SocketVoiceChannel;
			
			if((player.IsPlaying && myChannel.Users.Count > 2 && !(Context.User as IGuildUser).ContainsRole("DJ")))
			{
				if (sendError)
				{
					await Context.Channel.SendMessageAsync($"{Context.User.Mention} {GetError(12)}");
				}
				return null;
			}

			if (song.StartsWith("https://www.youtube.com/playlist"))
				return await PlayTracksAsync(song, Context, textChannel, sendError);

			LavaTrack track = null;

			if (Uri.TryCreate(song, UriKind.Absolute, out Uri uriResult)
				&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
			{
				if(song.Contains("youtube"))
				{
					track = (await LavaRestClient.SearchYouTubeAsync(song)).Tracks.FirstOrDefault();
				}
				else if(song.Contains("soundcloud"))
				{
					track = (await LavaRestClient.SearchSoundcloudAsync(song)).Tracks.FirstOrDefault();
				}
			}
			else
			{
				track = (await LavaRestClient.SearchYouTubeAsync(song)).Tracks.FirstOrDefault();
			}
			await player.PlayAsync(track);
			return track;
		}
		
		public async Task<LavaTrack> PlayTracksAsync(string url, ICommandContext Context, ITextChannel textChannel, bool sendError = true)
		{
			var tplayer = await JoinAsync(Context, textChannel, false);
			var player = tplayer.Item2;

			if (tplayer.Item1 == false)
			{
				if (sendError)
					await Context.Channel.SendMessageAsync($"{Context.User.Mention} {GetError(11)}");
				return null;
			}
			SocketVoiceChannel usersChannel = (Context.User as IVoiceState).VoiceChannel as SocketVoiceChannel;
			SocketVoiceChannel myChannel = (Context.Client.CurrentUser as IVoiceState).VoiceChannel as SocketVoiceChannel;

			var tracks = (await LavaRestClient.SearchTracksAsync(url)).Tracks;
			foreach(var track in tracks)
			{
				player.Queue.Enqueue(track);
			}
			return tracks.FirstOrDefault();
		}


	}
}
