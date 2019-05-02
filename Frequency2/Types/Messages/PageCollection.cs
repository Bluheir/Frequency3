using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;
using Discord.Rest;

namespace Frequency2.Types.Messages
{
	public class PageCollection : 
		INoMessagePageCollection
	{
		private List<Page> _pages;
		public IReadOnlyList<Page> Pages => _pages;

		public IEnumerator<Page> GetEnumerator()
		=> Pages.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();

		public int CurrentPage { get; private set; }

		public int Count => Pages.Count;

		
		public IUserMessage Message { get; private set; }

		public Page this[int index]
		{
			get
			=> Pages[index];
		}
		public static async Task<PageCollection> ConstructorAsync(IEnumerable<Page> pages, IUserMessage message, int currentPage = 0)
		{
			PageCollection outval = new PageCollection
			{
				_pages = new List<Page>(pages),
				Message = message,
				CurrentPage = currentPage
			};
			var page = outval.Pages[outval.CurrentPage];
			{
				if (!String.IsNullOrWhiteSpace(page.Content))
					await outval.Message.ModifyAsync(x => x.Content = page.Content);
				await outval.Message.ModifyAsync(x => x.Embed = page.Embed);
			}
			return outval;
		}
		private PageCollection(){}

		public PageCollection(IEnumerable<Page> pages, IUserMessage message, int currentPage = 0)
		{
			_pages = new List<Page>(pages);
			Message = message;
			CurrentPage = currentPage;
		}

		public async Task<bool> PageAtAsync(int index)
		{
			if (index >= Count || index < 0 || index == CurrentPage)
				return false;

			var page = Pages[index];
			if (!String.IsNullOrWhiteSpace(page.Content))
				await Message.ModifyAsync(x => x.Content = page.Content);
			await Message.ModifyAsync(x => x.Embed = page.Embed);
			CurrentPage = index;
			return true;
		}

		public static async Task PaginateAsync(PageCollection pages)
		{
			try
			{
				foreach (var emoji in PaginationService<DiscordShardedClient>.Emojis)
					await pages.Message.AddReactionAsync(emoji);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			
			PaginationService<DiscordShardedClient>.Paginate(pages.Message.Id, pages);
		}
	}
}
