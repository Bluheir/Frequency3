using System.Collections.Concurrent;
using Victoria;

namespace Frequency3.Audio
{
	public class GuildMusicConfig
	{
		public bool Repeat { get; set; }
		public int? MaxQueueTimes { get; set; }
		public bool IsLocked => MaxQueueTimes == 0;
		public ConcurrentDictionary<ulong, int> UserQueueTimes { get; }
		public bool IsPlayed { get; set; }
		public CQueue<TrackInput> Queue { get; }

		public GuildMusicConfig()
		{
			Repeat = false;
			MaxQueueTimes = null;
			UserQueueTimes = new ConcurrentDictionary<ulong, int>();
			IsPlayed = false;
			Queue = new CQueue<TrackInput>();
		}
		public int GetUserInputs(ulong id)
		{
			return UserQueueTimes.GetOrAdd(id, x => 0);
		}
		public int DecrementUser(ulong id)
		{
			return UserQueueTimes.AddOrUpdate(id, 0, (x, y) =>
			{
				if (y == 0)
					return 0;
				return y - 1;
			});
		}
	}
}
