using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Collections.Generic;
using System;
using Frequency2.Methods;
using Frequency2.Audio;
using System.Threading.Tasks;
using Frequency2.Types.Attributes;

namespace Frequency2.Modules
{

	public class Music : ModuleBase<SocketCommandContext>
	{
		internal static AudioService Audio => AudioService.Instance;


		[Command("play", RunMode = RunMode.Async)]
		[Summary("Joins the current users void channel and plays the specified song. If the url isn't valid, searched YouTube for the term")]
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

		[Command("searchsc"), Alias("soundcloudsearch", "playsoundcloud")]
		[Summary("Joins the current users void channel and plays the specified song. If the url isn't valid, searched SoundCloud for the term")]
		public async Task SearchSCAsync([Remainder, Summary("The url or search term")]string query)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PlayAsync(query, Context, Context.Channel as ITextChannel, prioritiseSoundcloud: true);
		}

		[Command("queue"), Alias("enqueue")]
		[Summary("Enqueues a song. If the url isn't valid, searches YouTube for the term")]
		public async Task EnqueueYTAsync([Remainder, Summary("The url or search term")]string query)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.QueueAsync(query, Context, Context.Channel as ITextChannel);
		}

		[Command("skip")]
		[Summary("Skips the current track")]
		public async Task SkipAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.SkipTrackAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("pause")]
		[Summary("Pauses/Resumes the current track")]
		public async Task PauseAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PauseAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("repeat")]
		[Summary("Toggles repeating tracks")]
		public async Task RepeatAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.RepeatAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("queuelist")]
		[Summary("Adds every track from the playlist to the queue")]
		public async Task QueueList([Remainder, Summary("The url or search term")]string query)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PlayTracksAsync(query, Context, Context.Channel as ITextChannel, clear: false);
		}

		[Command("shuffle"), Alias("shufflequeue", "randomqueue")]
		[Summary("Shuffles the queue")]
		public async Task ShuffleAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.ShuffleAsync(Context, Context.Channel as ITextChannel);
		}

	}
}
