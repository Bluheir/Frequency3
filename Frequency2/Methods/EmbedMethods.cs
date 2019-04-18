using Discord;
using System.Threading.Tasks;
using Victoria;
using Victoria.Entities;

namespace Frequency2.Methods
{
	public static class EmbedMethods
	{
		public static async Task<Embed> GetEmbedQueue(LavaTrack track, LavaPlayer player, LavaTrack ptrack)
		{
			EmbedBuilder Embed = new EmbedBuilder
			{
				Title = "Playing Song",
				Description = $"[{track.Title}]({track.Uri.AbsoluteUri})"
			};

			Embed.AddField("Length", track.Length.ToString(), true);
			Embed.AddField("Tracks in Queue", (player.Queue.Count).ToString(), true);
			Embed.AddField("Previous Track", ptrack.Title, true);
			Embed.AddField("Next Track", (player.Queue.Count == 0) ? "No tracks" : (player.Queue.Peek() as LavaTrack).Title, true);

			Embed.ImageUrl = await track.FetchThumbnailAsync();
			return Embed.Build();
		}
		
	}
}
