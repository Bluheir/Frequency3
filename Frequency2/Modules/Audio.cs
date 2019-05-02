using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Collections.Generic;
using System;
using Frequency2.Methods;
using Frequency2.Audio;
using System.Threading.Tasks;
using Frequency2.Types.Attributes;
using Frequency2.Types.Messages;

namespace Frequency2.Modules
{

	public class Music : ModuleBase<SocketCommandContext>
	{
		internal static AudioService Audio => AudioService.Instance;


		[Command("play", RunMode = RunMode.Async)]
		[Summary("Joins the current users void channel and plays the specified song. If the url isn't valid, searches YouTube for the term")]
		public async Task PlayAsync([Remainder, Summary("The url or search term")]string query)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PlayAsync(query, Context, Context.Channel as ITextChannel);
		}

		[Command("join", RunMode = RunMode.Async)]
		[Summary("Joins the current user's voice channel")]
		public async Task JoinAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.JoinAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("playlist", RunMode = RunMode.Async)]
		[Summary("Adds every track from the playlist and plays the first track")]
		public async Task PlayListAsync([Remainder, Summary("The url or search term")]string query)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PlayTracksAsync(query, Context, Context.Channel as ITextChannel);
		}

		[Command("queuesc", RunMode = RunMode.Async)] 
		[Summary("Enqueues a song. If the url isn't valid, searches SoundCloud for the term")]
		public async Task EnqueueSCAsync([Remainder, Summary("The url or search term")]string query)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.QueueAsync(query, Context, Context.Channel as ITextChannel, prioritiseSoundcloud: true);
		}

		[Command("searchsc", RunMode = RunMode.Async), Alias("soundcloudsearch", "playsoundcloud")]
		[Summary("Joins the current users void channel and plays the specified song. If the url isn't valid, searches SoundCloud for the term")]
		public async Task SearchSCAsync([Remainder, Summary("The url or search term")]string query)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PlayAsync(query, Context, Context.Channel as ITextChannel, prioritiseSoundcloud: true);
		}

		[Command("queue", RunMode = RunMode.Async), Alias("enqueue")]
		[Summary("Enqueues a song. If the url isn't valid, searches YouTube for the term")]
		public async Task EnqueueYTAsync([Remainder, Summary("The url or search term")]string query)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.QueueAsync(query, Context, Context.Channel as ITextChannel);
		}

		[Command("skip", RunMode = RunMode.Async)]
		[Summary("Skips the current track")]
		public async Task SkipAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.SkipTrackAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("pause", RunMode = RunMode.Async)]
		[Summary("Pauses/Resumes the current track")]
		public async Task PauseAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PauseAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("repeat", RunMode = RunMode.Async)]
		[Summary("Toggles repeating tracks")]
		public async Task RepeatAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.RepeatAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("queuelist", RunMode = RunMode.Async)]
		[Summary("Adds every track from the playlist to the queue")]
		public async Task QueueList([Remainder, Summary("The url or search term")]string query)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PlayTracksAsync(query, Context, Context.Channel as ITextChannel, clear: false);
		}

		[Command("shuffle", RunMode = RunMode.Async), Alias("shufflequeue", "randomqueue")]
		[Summary("Shuffles the queue")]
		public async Task ShuffleAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.ShuffleAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("tracks", RunMode = RunMode.Async)]
		[Summary("Shows a list of tracks in the current queue")]
		public async Task GetTracksAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			var tracks = Audio.GetTracks(Context.Guild.Id);
			if (tracks.Count == 0)
				return;
			var message = await Context.Channel.SendMessageAsync(embed: tracks[0]);
			await message.PaginateAsync(tracks.ToArray());
		}
	}
}
