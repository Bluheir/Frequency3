using Discord;
using Frequency2.Types.Messages;
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
		=> await message.PaginateAsync(0, pages);

		public static async Task PaginateAsync(this IUserMessage message, int start, params Page[] pages)
		=> await PageCollection.PaginateAsync(new PageCollection(pages, message, start));

		public static async Task PaginateAsync(this IUserMessage message, int start, params Embed[] embeds)
		{
			List<Page> pages = new List<Page>(embeds.Length);
			foreach (var item in embeds)
				pages.Add(new Page(item));
			await message.PaginateAsync(start, pages.ToArray());
		}

		public static async Task PaginateAsync(this IUserMessage message, params Embed[] embeds)
		=> await message.PaginateAsync(0, embeds);

		public static async Task PaginateAsync(this IUserMessage message, int start, params EmbedBuilder[] embeds)
		=> await message.PaginateAsync(start, embeds.Select(x => x.Build()).ToArray());

		public static async Task PaginateAsync(this IUserMessage message, params EmbedBuilder[] embeds)
		=> await message.PaginateAsync(0, embeds);
	}
}
