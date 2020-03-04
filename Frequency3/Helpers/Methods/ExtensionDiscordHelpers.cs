using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Frequency3.Helpers.Methods
{
	public static class ExtensionDiscordHelpers
	{
		public static IVoiceChannel? UsersVC(this ICommandContext context)
		{
			if (context.User == null || context == null)
				return null;
			return ((IVoiceState)context.User).VoiceChannel;
		}
		public static bool HasRole(this IGuildUser user, string role)
		{
			if (user == null)
				return false;
			SocketGuildUser uu = (SocketGuildUser)user;
			return uu.Roles.Where(x => x.Name == role).FirstOrDefault() != null;
		}
	}
}
