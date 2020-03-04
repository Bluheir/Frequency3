using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Frequency3.Modules
{
	public class GeneralCommands : ModuleBase<ShardedCommandContext>
	{
		[Command("test")]
		public async Task Test()
		{
			await Context.Channel.SendMessageAsync("Hello bitch");
		}
	}
}
