using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using Frequency2.Audio;

namespace Frequency2.Modules
{
	public class Audio : ModuleBase<ShardedCommandContext>
	{
		internal static AudioService audio = new AudioService();
	}
}
