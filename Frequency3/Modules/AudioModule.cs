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
			try
			{
				await _audio.PlayTrackAsync(song, Context, Context.Channel as ITextChannel, false);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}
		[Command("queue")]
		public async Task EnqueueAsync([Remainder]string song)
		{
			await _audio.PlayTrackAsync(song, Context, Context.Channel as ITextChannel, false);
		}
	}
}
