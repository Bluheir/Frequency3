using System.Collections.Concurrent;
using System.Collections.Generic;
using Victoria;

namespace Frequency2.Audio
{
	public class GuildMusicConfig
	{
		public bool Repeat { get; set; }
		public int? MaxQueueTimes { get; set; }
		public bool IsLocked => MaxQueueTimes == 0;
		public ConcurrentDictionary<ulong, int> UserQueueTimes { get; }
		public bool IsPlayed { get; set; }
		public CQueue<LavaTrack> Queue { get; }

		public GuildMusicConfig()
		{
			Repeat = false;
			MaxQueueTimes = null;
			UserQueueTimes = new ConcurrentDictionary<ulong, int>();
			IsPlayed = false;
			Queue = new CQueue<LavaTrack>();
		}
	}
}
