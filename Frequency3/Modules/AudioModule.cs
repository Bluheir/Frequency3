using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using Frequency3.Audio;
using Discord;

namespace Frequency3.Modules
{
	public class AudioModule : ModuleBase<ShardedCommandContext>
	{
		private static AudioService? _audio => AudioService.Instance;

		[Command("play")]
		public async Task PlaySongAsync([Remainder]string song)
		{
	    	await _audio.PlayTrackAsync(song, Context, Context.Channel as ITextChannel, false);
		}
		[Command("queue")]
		public async Task EnqueueAsync([Remainder]string song)
		{
			await _audio.QueueAsync(song, Context, Context.Channel as ITextChannel, false);
		}
        [Command("skip")]
        public async Task SkipAsync()
        {
            await _audio.SkipAsync(Context, Context.Channel as ITextChannel);
        }
        [Command("join")]
        public async Task JoinAsync()
        {
            await _audio.JoinAsync(Context, Context.Channel as ITextChannel);
        }
        [Command("shuffle")]
        public async Task ShuffleAsync()
        {
            await _audio.ShuffleAsync(Context, Context.Channel as ITextChannel);
        }
        [Command("leave")]
        public async Task LeaveAsync()
        {
            await _audio.LeaveAsync(Context, Context.Channel as ITextChannel);
        }
        [Command("setlimit")]
        public async Task SetLimitAsync(uint limit)
        {
            if(limit > int.MaxValue)
            {
                await Context.Channel.SendMessageAsync($":musical_note: {Context.User.Mention} Can't set the queue limit above {int.MaxValue}");
            }
            await _audio.SetQueueLimitAsync(Context, Context.Channel as ITextChannel, limit);
        }
	}
}
