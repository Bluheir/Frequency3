using Discord;
using Discord.WebSocket;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Frequency2.Source;

namespace Frequency2.Types.Messages
{
	public class PaginationService<T> where T : BaseSocketClient
	{
		public static void Paginate(ulong message, PageBase pages)
		{
			Instance._messages.Add(message, pages);
		}
		public static bool ContainsMessage(ulong message)
		{
			return Instance._messages.ContainsKey(message);
		}

		private readonly T _client;
		internal readonly Dictionary<ulong, PageBase> _messages;

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
		public static bool ContainsEmoji(string name)
		=> Emojis.Where(x => x.Name == name).FirstOrDefault() != null;

		public static PaginationService<T> Instance { get; private set; }

		public PaginationService(T client)
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


			if (Frequency2Client.Instance._commands._userTimeouts.AddOrUpdate(reaction.User.Value.Id, 1, (ulong id, int i) => { return i + 1; }) == 5)
			{
				Frequency2Client.Instance._commands._userTimeouts[reaction.User.Value.Id]--;
				return;
			}

			var cache = await message.GetOrDownloadAsync();

			bool remove = await collection.SpecialFunctions(collection, reaction);

			if (remove)
			{
				await cache.DeleteAsync();
				_messages.Remove(cache.Id);
			}

		}
	}
}
