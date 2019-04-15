using Discord.WebSocket;
using Discord;
using System.Linq;

namespace Frequency2.Methods
{
	public static class DiscordUserMethods
	{
		public static bool ContainsRole(this IGuildUser user, string role)
		=> (user as SocketGuildUser).Roles.FirstOrDefault(x => x.Name == role) != null;

		public static bool ContainsRole(this IGuildUser user, ulong roleId)
		=> (user as SocketGuildUser).Roles.FirstOrDefault(x => x.Id == roleId) != null;

		public static bool IsPrivateMessage(this IMessage message)
		=> message.Channel is IDMChannel;

	}
}
