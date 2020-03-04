using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Frequency2.Types.Messages.PaginationService<Discord.WebSocket.BaseSocketClient>;

namespace Frequency2.Types.Messages
{
	public class PageCollection : PageBase,
		IEnumerable<Page>,
		IEnumerable,
		IReadOnlyList<Page>,
		IReadOnlyCollection<Page>
	{
		public PageCollection(IEnumerable<Page> pages, IUserMessage message) : base()
		{
			_pages = new List<Page>(pages);

			Message = message;
			CurrentPage = 0;
		}
		public PageCollection(IEnumerable<Embed> embeds, IUserMessage message) : this(embeds.Select(x => new Page(x)), message) { }
		public async Task<bool> PaginateAsync()
		{
			try
			{
				foreach (var emoji in PaginationService<DiscordShardedClient>.Emojis)
					await Message.AddReactionAsync(emoji);
			}
			catch { return false; }



			PaginationService<DiscordShardedClient>.Paginate(Message.Id, this);
			return true;
		}
		public override async Task<bool> SpecialFunctions(PageBase x, SocketReaction y)
		{

			var emoji = y.Emote.Name;
			var collection = (PageCollection)x;

			if (emoji == Emojis[0].Name)
			{
				await PageAtAsync(0);
			}
			else if (emoji == Emojis[1].Name)
			{
				int page = collection.CurrentPage - 1;
				if (collection.CurrentPage == 0)
					page = collection.Count - 1;
				await collection.PageAtAsync(page);
			}
			else if (emoji == Emojis[2].Name)
			{
				int page = collection.CurrentPage + 1;
				if (collection.CurrentPage == collection.Count - 1)
					page = 0;
				await collection.PageAtAsync(page);
			}
			else if (emoji == Emojis[3].Name)
			{
				return true;
			}

			return false;
		}


		public int CurrentPage { get; private set; }

		public IEnumerator<Page> GetEnumerator()
		=> Pages.GetEnumerator();

		public Page this[int index] => _pages[index];

		public IReadOnlyList<Page> Pages => _pages;

		private List<Page> _pages;

		public int Count => _pages.Count;

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

		IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();
	}
}
