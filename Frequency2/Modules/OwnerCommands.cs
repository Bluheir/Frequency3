using Discord.Commands;
using System.Threading.Tasks;
using Frequency2.Source;
using Frequency2.Config;
using System;
using Discord;
using static Frequency2.Methods.MessageMethods;

namespace Frequency2.Modules
{
	public class OwnerCommands : ModuleBase<ShardedCommandContext>
	{
		[Command("savelog")]
		public async Task SaveLogAsync()
		{
			if(Context.User.Id != Configuration.Config.OwnerId)
			{
				await ReplyAsync($"{Context.User.Mention} {GetError(14)}");
				return;
			}
			await Logger.Instance.SaveLogAsync();
		}

		[Command("log")]
		public async Task LogAsync([Remainder]string message)
		{
			if (Context.User.Id != Configuration.Config.OwnerId)
			{
				await ReplyAsync($"{Context.User.Mention} {GetError(14)}");
				return;
			}
			await Logger.Instance.LogAsync(new LogMessage(LogSeverity.Info, "discordapp.com", message));
		}
	}
}
