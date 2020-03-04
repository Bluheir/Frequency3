using Discord;
using Discord.WebSocket;
using Frequency3.PagedMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frequency3.Core
{
	public class PaginationService 
	{
		private readonly DiscordShardedClient _client;
		internal readonly Dictionary<ulong, PageBase> _messages;

		public static bool ContainsEmoji(string name)
		=> Emojis.Where(x => x.Name == name).FirstOrDefault() != null;
		public static PaginationService Instance { get; private set; }

		public PaginationService(DiscordShardedClient client)
		{
			if (Instance != null)
				throw new InvalidOperationException("Cannot create another instance of a singleton");
			Instance = this;
			_messages = new Dictionary<ulong, PageBase>();
			_client = client;
			_client.ReactionAdded += _client_ReactionAdded;
		}

		private async Task _client_ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (!_messages.ContainsKey(message.Id))
				return;
			if (_client.CurrentUser.Id != message.Value.Author.Id)
				return;
			if (reaction.UserId == _client.CurrentUser.Id)
				return;



			var collection = _messages[message.Id];

			var cache = await message.GetOrDownloadAsync();

			bool remove = await collection.SpecialFunctions(collection, reaction);

			if (remove)
			{
				await cache.DeleteAsync();
				_messages.Remove(cache.Id);
			}

		}
		public void Paginate(ulong message, PageBase pages)
		{
			Instance._messages.Add(message, pages);
		}
		public bool ContainsMessage(ulong message)
		{
			return Instance._messages.ContainsKey(message);
		}

		public static IReadOnlyList<Emoji> Emojis = new List<Emoji>()
		{
			new Emoji("⏪"),
			new Emoji("\u25C0"),
			new Emoji("\u25B6"),
			new Emoji("\u23F9")
		};
		public static IReadOnlyList<Emoji> AudioEmojis = new List<Emoji>()
		{
			new Emoji("\u23F9"),
			new Emoji("🎲"),
			new Emoji("🔁"),
			new Emoji("\u23ED"),
			new Emoji("\u27A1"),
			new Emoji("\u274C")

		};
	}
}
