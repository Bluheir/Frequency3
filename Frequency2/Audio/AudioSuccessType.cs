using System;
using System.Collections.Generic;
using System.Text;

namespace Frequency2.Audio
{
	public enum AudioSuccessType : byte
	{
		/// <summary>
		/// Successful
		/// </summary>
		Successful = 1,
		/// <summary>
		/// Invalid Permissions
		/// </summary>
		InvalidPerms,
		/// <summary>
		/// The user is not in a channel for that guild but the bot is
		/// </summary>
		Inchannel,
		/// <summary>
		/// The user is in the same channel as the bot
		/// </summary>
		SameChannel,
		/// <summary>
		/// The bot and the user isn't in a channel
		/// </summary>
		UserNotInChannel

	}
}
