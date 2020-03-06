using Discord;
using Frequency3.Audio;
using System.Threading.Tasks;
using Victoria;


namespace Frequency3.Helpers.Methods
{
	public static class EmbedMethods
	{
		public static async Task<Embed> GetEmbedQueue(LavaTrack track, CQueue<TrackInput> tracks
		, LavaTrack ptrack)
		{
			EmbedBuilder Embed = new EmbedBuilder
			{
				Title = "Playing Song",
				Description = $"[{track.Title}]({track.Url})"
			};

			Embed.AddField("Length", track.Duration.ToString(), true);
			Embed.AddField("Tracks in Queue", (tracks.Count).ToString(), true);
			Embed.AddField("Previous Track", ptrack.Title, true);
			Embed.AddField("Next Track", (tracks.Count == 0) ? "No tracks" : (tracks[0]).Track.Title, true);

			Embed.ImageUrl = await track.FetchArtworkAsync();
			return Embed.Build();
		}

	}
}
