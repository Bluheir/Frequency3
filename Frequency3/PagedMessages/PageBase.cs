using Discord;
using Discord.WebSocket;
using Frequency3.Core;
using System.Threading.Tasks;

namespace Frequency3.PagedMessages
{
	public abstract class PageBase
	{
		public bool IsPaginated => PaginationService.Instance.ContainsMessage(Message.Id);
		public IUserMessage Message { get; }

		public PageBase(IUserMessage message)
		{
			Message = message;
		}
		public abstract Task<bool> SpecialFunctions(PageBase x, SocketReaction y);
	}
}
