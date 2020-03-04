using Discord;
using Discord.WebSocket;
using Frequency2.Types.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frequency2.Methods
{
	public static class MessageMethods
	{
		public static string GetError(uint errorCode)
		{
			if (!Frequency2.Types.Commands.CommandErrorFix.Errors.TryGetValue(errorCode, out Types.Commands.CommandErrorFix value))
				return GetError(303);
			return $":x: Error: {errorCode} `{value}`";
		}

		public static bool IsPrivate(this IMessage message)
		=> message.Channel is IDMChannel;

		

		public static async Task PaginateAsync(this IUserMessage message, params Page[] pages)
		=> await new PageCollection(pages, message).PaginateAsync();

		public static async Task PaginateAsync(this IUserMessage message, params Embed[] embeds)
		{
			List<Page> pages = new List<Page>(embeds.Length);
			foreach (var item in embeds)
				pages.Add(new Page(item));
			await message.PaginateAsync(pages.ToArray());
		}

		public static async Task PaginateAsync(this IUserMessage message, params EmbedBuilder[] embeds)
		=> await message.PaginateAsync(embeds.Select(x => x.Build()).ToArray());

		public static async Task PaginateAsync(this IUserMessage message, PageCollection collection)
		{
			if (collection == null)
				return;
			if (collection.IsPaginated)
				return;
			await collection.PaginateAsync();
		}
	}
}
