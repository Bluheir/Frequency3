using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Collections.Generic;
using System;
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
			await Audio.PlayAsync(url, Context, Context.Channel as ITextChannel);
		}

		[Command("join", RunMode = RunMode.Async)]
		public async Task JoinAsync()
		{
			
			//Console.WriteLine(Activator.CreateInstance(typeof(AudioService)));
			await Audio.JoinAsync(Context, Context.Channel as ITextChannel);
		}
	}
}
