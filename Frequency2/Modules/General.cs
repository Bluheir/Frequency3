using Discord.Commands;
using Frequency2.Types.Attributes;
using System.Threading.Tasks;
using Frequency2.Methods;
using Frequency2.Source;
using Discord;
using static Frequency2.Methods.MessageMethods;

namespace Frequency2.Modules
{
	public class General : ModuleBase<SocketCommandContext>
	{
		
		[Command("ping", RunMode = RunMode.Async)]
		[Summary("Replies pong in the current channel")]
		public async Task Pong()
		{
			await Context.Channel.SendMessageAsync("Pong");
			
		}

		[Command("commandinfo")]
		public async Task GetCommandInfoAsync([Remainder, Summary("The command name")]string commandname)
		{
			IMessageChannel channel;
			if (Context.Message.IsPrivate())
				channel = Context.Channel;
			else
				channel = await Context.User.GetOrCreateDMChannelAsync();
			if(!Frequency2Client.Instance.CommandInfos.TryGetValue(commandname, out CommandInfo info))
			{
				await channel.SendMessageAsync($"{Context.User.Mention} {GetError(19)}");
				return;
			}
			EmbedBuilder Embed = new EmbedBuilder();

			Embed.WithTitle($"Command: {commandname}");
			Embed.AddField("Summary:", info.Summary);

			string title = "Parameters:";
			string parameters = "";

			foreach(var parameter in info.Parameters)
			{
				parameters += $"    **Parameter Name**: {parameter.Name}\n    **Summary** : {parameter.Summary ?? "No summary provided"}\n";
			}

			Embed.AddField(title, parameters);

			await channel.SendMessageAsync("", false, Embed.Build());
		}

		
	}
}