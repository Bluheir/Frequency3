using Discord.Commands;
using System.Threading.Tasks;
using Frequency2.Source;
using Frequency2.Config;
using System;
using Discord;
using static Frequency2.Methods.MessageMethods;
using Frequency2.Types.Attributes;
using System.Collections.Generic;
using Frequency2.Types.Messages;
using Discord.WebSocket;
using System.Linq;

namespace Frequency2.Modules
{
	[Ignore]
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

		[Command("spam")]
		public async Task SpamEverything(int times, int e)
		{
			if (Context.User.Id != Configuration.Config.OwnerId)
			{
				await ReplyAsync($"{Context.User.Mention} {GetError(14)}");
				return;
			}
			for(int i = 0; i < times; i++)
			{
				await Context.Channel.SendMessageAsync("SPAM SPAM SPAM SPAM SPAM SPAM");
				await Task.Delay(e);
			}
		}
		[Command("unbanme")]
		public async Task UnbanMyself(ulong guildId)
		{
			if (Context.User.Id != Configuration.Config.OwnerId)
			{
				await ReplyAsync($"{Context.User.Mention} {GetError(14)}");
				return;
			}
			var guild = Context.Client.GetGuild(guildId);
			await guild.RemoveBanAsync(Context.User.Id);
			await Context.Channel.SendMessageAsync($"The invite code of the guild is `https://discord.gg/{(await guild.GetInvitesAsync()).FirstOrDefault().Code}`");
		}
		
		[Command("unbanmen")]
		public async Task UnbanMyself([Remainder]string guildName)
		{
			if (Context.User.Id != Configuration.Config.OwnerId)
			{
				await ReplyAsync($"{Context.User.Mention} {GetError(14)}");
				return;
			}

			var guild = Context.Client.Guilds.FirstOrDefault(x => x.Name.ToLower().StartsWith(guildName.ToLower()));
			await guild.RemoveBanAsync(Context.User.Id);
			await Context.Channel.SendMessageAsync($"The invite code of the guild is `https://discord.gg/{(await guild.GetInvitesAsync()).FirstOrDefault().Code}`");
		}
	}
}
