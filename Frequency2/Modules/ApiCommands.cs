using Discord.Commands;
using System.Collections.Generic;
using Frequency2.REST.UrbanDictionary;
using System.Threading.Tasks;
using Frequency2.Methods;
using Discord;
using System;

namespace Frequency2.Modules
{
	public class ApiCommands : ModuleBase<ShardedCommandContext>
	{
		private const uint MaxPages = 20;

		[Command("ubdefinition")]
		[Summary("Gets a definition of a word from the urban dictionary")]
		public async Task GetUbDefinitionAsync(
			[Summary("The max amount of definitions for this")]
			uint maxdefinitions,
			[Summary("The word to get the urban dictionary definition from")]
			[Remainder]
			string word)
		{
			if (maxdefinitions > MaxPages)
				maxdefinitions = MaxPages;
			if (maxdefinitions == 0)
				maxdefinitions = 1;
			UrbanDictionaryClient client = new UrbanDictionaryClient();
			
			var definitions = new List<IDefinition>();
			{
				var definition = await client.GetDefinitionAsync(word);
				if (definition.Count == 0)
					return;

				if (maxdefinitions > 1)
				{
					for (int i = 0; i < definition.Count; i++)
					{
						if ((uint)i == maxdefinitions)
							break;
						definitions.Add(definition[i]);

					}
				}
				else
					definitions.Add(definition[0]);
			}

			List<EmbedBuilder> Pages = new List<EmbedBuilder>();
			int b = 0;
			foreach(var def in definitions)
			{
				if (def.Example.Length > 2048 || def.Definition.Length > 2048)
					continue;
				b++;
					
				var Embed = new EmbedBuilder();

				Embed.WithTitle($"Definition {b} out of ");

				Embed.AddField("Word: ", def.Word);

				Embed.AddField("Author: ", def.Author, true);
				Embed.AddField("Current Vote: ", String.IsNullOrWhiteSpace(def.Current_Vote) ? "No current vote" : def.Current_Vote, true);
				Embed.AddField("Definition Id: ", "" + def.DefinitionId, true);
			
				Embed.AddField("Ratings: ", $":thumbsdown: {def.Thumbs_Down} :thumbsup: {def.Thumbs_Up}");
				
				if (def.Definition.Length > 1024)
				{
					Embed.AddField("Definition p1: ", def.Definition.Substring(0, 1024));
					Embed.AddField("Definition p2: ", def.Definition.Substring(1024));
				}
				else
				{
					Embed.AddField("Definition: ", def.Definition);
				}
				if (def.Example.Length > 1024)
				{
					Embed.AddField("Example p1: ", def.Example.Substring(0, 1024));
					string sub = def.Example.Substring(1024);
					Embed.AddField("Example p2: ", sub);
						
					
					
				}
				else
				{
					Embed.AddField("Example: ", def.Example);
				}
				
				Embed.AddField("Written On: ", def.Written_On);

				Pages.Add(Embed);
			}
			foreach (var embed in Pages)
				embed.Title += "" + b;
			var message = await Context.Channel.SendMessageAsync("", false, Pages[0].Build());
			await message.PaginateAsync(Pages.ToArray());
		}
	}
}
