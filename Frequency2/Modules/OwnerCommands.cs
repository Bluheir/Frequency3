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

		[Command("testpage")]
		public async Task TestPaginate()
		{
			if (Context.User.Id != Configuration.Config.OwnerId)
			{
				await ReplyAsync($"{Context.User.Mention} {GetError(14)}");
				return;
			}
			EmbedBuilder embed1 = new EmbedBuilder
			{
				Fields = new List<EmbedFieldBuilder>()
				{
					new EmbedFieldBuilder
					{
						Name = "hello",
						Value = "test"
					},
					new EmbedFieldBuilder
					{
						Name = "hello2",
						Value = "test2"
					},
				}
			};
			EmbedBuilder embed2 = new EmbedBuilder
			{
				Fields = new List<EmbedFieldBuilder>()
				{
					new EmbedFieldBuilder
					{
						Name = "hello1234",
						Value = "test43434"
					},
					new EmbedFieldBuilder
					{
						Name = "hello212341241234132",
						Value = "test2132412341241324"
					},
				}
			};
			var message = await Context.Channel.SendMessageAsync("", false, embed1.Build());
			await PageCollection.PaginateAsync(new PageCollection(new List<Page> { new Page(embed1.Build()), new Page(embed2.Build()) }, message ));
		}
	}
}
