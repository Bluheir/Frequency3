using Discord.Commands;
using Frequency2.Types.Attributes;
using System.Threading.Tasks;
using Frequency2.Methods;
using Frequency2.Source;
using Discord;
using static Frequency2.Methods.MessageMethods;
using System.Linq;
using System.Collections.Generic;
using Discord.WebSocket;

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

		[Command("commandinfo", RunMode = RunMode.Async)]
		[Summary("Gets the info about a command including its parameters and summary for its parameters")]
		public async Task GetCommandInfoAsync([Remainder, Summary("The command name")]string commandname)
		{
			IMessageChannel channel;
			if (Context.Message.IsPrivate())
				channel = Context.Channel;
			else
				channel = await Context.User.GetOrCreateDMChannelAsync();
			if(!Frequency2Client.Instance.CommandInfos.TryGetValue(commandname.ToLower(), out CommandInfo info))
			{
				await channel.SendMessageAsync($"{Context.User.Mention} {GetError(19)}");
				return;
			}
			EmbedBuilder Embed = new EmbedBuilder();

			Embed.WithTitle($"Command: {commandname.ToLower()}");
			Embed.AddField("Summary:", info.Summary ?? "No summary provided");

			string title = "Parameters:";
			string parameters = "";

			foreach(var parameter in info.Parameters)
			{
				parameters += $"    **Parameter Name**: {parameter.Name}\n    **Summary** : {parameter.Summary ?? "No summary provided"}\n";
			}
			if(parameters != "")
				Embed.AddField(title, parameters);

			await channel.SendMessageAsync("", false, Embed.Build());
		}
		[Command("help")]
		[Summary("Shows a list of every command")]
		public async Task GetHelpAsync()
		{

			var commands = Frequency2Client.Instance.CommandInfos.Keys.ToList();

			int page = 1;

			List<EmbedBuilder> Embeds = new List<EmbedBuilder>();
			string field = null;
			

			for(int i = 0; i < commands.Count; i++)
			{
				if(i % 10 == 0 || i == commands.Count - 1)
				{
					if(page != 1)
						Embeds[Embeds.Count - 1].Fields[0].Value = field;

					if (i == commands.Count - 1)
					{
						field += $"**{i + 1})** {commands[i]}\n";
						Embeds[Embeds.Count - 1].Fields[0].Value = field;
						break;
					}

					Embeds.Add(new EmbedBuilder()
					{
						Title = $"Page {page} out of ",
						Fields = new List<EmbedFieldBuilder>
						{
							new EmbedFieldBuilder()
							{
								Name = "Commands:",
							}
						},
						Description = "Hint: type **commandinfo <command name>** to get more information about a command"
					});
					field = "";
					page++;
				}
				field += $"**{i + 1})** " + commands[i] + "\n";

			}
			foreach (var embed in Embeds)
				embed.Title += Embeds.Count;
			var message = await Context.Channel.SendMessageAsync("", false, Embeds[0].Build());
			await message.PaginateAsync(Embeds.ToArray());
		}
	}
}