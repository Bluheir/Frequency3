using System;
using System.Collections.Generic;
using System.Text;

namespace Frequency2.Audio
{
	public enum AudioSuccessType
	{
		/// <summary>
		/// Successful
		/// </summary>
		Successful = 0,
		/// <summary>
		/// Unsuccessful and not valid
		/// </summary>
		Error = 2,
		/// <summary>
		/// The user is not in a channel for that guild but the bot is
		/// </summary>
		Inchannel
	}
}
