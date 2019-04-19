using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Collections.Generic;
using System;
using Frequency2.Methods;
using Frequency2.Audio;
using System.Threading.Tasks;

namespace Frequency2.Modules
{
		
	public class Music : ModuleBase<SocketCommandContext>
	{
		internal static AudioService Audio => AudioService.Instance;

		
		[Command("play", RunMode = RunMode.Async)]
		public async Task PlayAsync([Remainder]string url)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PlayAsync(url, Context, Context.Channel as ITextChannel);
		}

		[Command("join", RunMode = RunMode.Async)]
		public async Task JoinAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.JoinAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("playlist")]
		public async Task PlayListAsync([Remainder]string url)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PlayTracksAsync(url, Context, Context.Channel as ITextChannel);
		}

		[Command("queuesc")]
		public async Task EnqueueSCAsync([Remainder]string url)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.QueueAsync(url, Context, Context.Channel as ITextChannel, prioritiseSoundcloud: true);
		}

		[Command("searchsc"), Alias("soundcloudsearch", "playsoundcloud")]
		public async Task SearchSCAsync([Remainder]string query)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PlayAsync(query, Context, Context.Channel as ITextChannel, prioritiseSoundcloud: true);
		}

		[Command("queue"), Alias("enqueue")]
		public async Task EnqueueYTAsync([Remainder]string query)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.QueueAsync(query, Context, Context.Channel as ITextChannel);
		}

		[Command("skip")]
		public async Task SkipAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.SkipTrackAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("pause")]
		public async Task PauseAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PauseAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("repeat")]
		public async Task RepeatAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.RepeatAsync(Context, Context.Channel as ITextChannel);
		}

		[Command("queuelist")]
		public async Task QueueList([Remainder]string url)
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.PlayTracksAsync(url, Context, Context.Channel as ITextChannel, clear: false);
		}

		[Command("shuffle")]
		public async Task ShuffleAsync()
		{
			if (Context.Message.IsPrivate())
				return;
			await Audio.ShuffleAsync(Context, Context.Channel as ITextChannel);
		}

	}
}
