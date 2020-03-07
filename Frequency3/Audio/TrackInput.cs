using System;
using Victoria;

namespace Frequency3.Audio
{
	public class TrackInput
	{
		public LavaTrack Track { get; }
		public ulong UserInputter { get; }

		public TrackInput(LavaTrack track, ulong user)
		{
			Track = track ?? throw new ArgumentNullException($"Argument {nameof(track)} cannot be null.");
			UserInputter = user;
		}

		public static implicit operator LavaTrack(TrackInput input)
		{
			return input.Track;
		}
	}
}
