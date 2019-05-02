using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static Frequency2.Data.Databases;
using static Frequency2.Methods.MessageMethods;

namespace Frequency2.Modules
{
	public class UserSettings : ModuleBase<ShardedCommandContext>
	{
		[Command("prefix")]
		[Summary("Sets the command prefix for you")]
		public async Task ChangePrefixAsync([Summary("The new prefix")]string prefix)
		{
			if(prefix.Length > 10 || prefix == Context.Client.CurrentUser.Mention)
			{
				await ReplyAsync($"{Context.User.Mention} {GetError(16)}");
				return;
			}

			var user = await Users.GetValue((long)Context.User.Id);
			user.Prefix = prefix;

			if (user.SendCompMessage)
				await ReplyAsync($":white_check_mark: Successfully changed your prefix to `{prefix}`");
			await Users.SaveAsync(user);

		}
		
		[Command("togglemsg")]
		[Summary("Disables some messages the bot sends")]
		public async Task ToggleMsg()
		{
			var user = await Users.GetValue((long)Context.User.Id);
			user.SendCompMessage = !user.SendCompMessage;
			await Users.SaveAsync(user);
		}
	}
}
