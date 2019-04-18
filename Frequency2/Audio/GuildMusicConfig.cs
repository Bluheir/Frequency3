using System.Collections.Concurrent;

namespace Frequency2.Audio
{
	public class GuildMusicConfig
	{
		public bool Repeat { get; set; } = false;
		public int? MaxQueueTimes { get; set; } = null;
		public ConcurrentDictionary<ulong, int> UserQueueTimes { get; set; } = new ConcurrentDictionary<ulong, int>();
		public bool IsPlayed { get; set; } = false;
	}
}
