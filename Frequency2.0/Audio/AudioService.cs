using System.Threading.Tasks;
using Discord.Commands;
using Victoria;

namespace Frequency2.Audio
{
	public sealed class AudioService
	{
		public static LavaShardClient LavaClient { get; private set; }

		public static void Init(LavaShardClient client)
		{
			LavaClient = client;
		}
		public async Task JoinAsync(ICommandContext Context)
		{
			
		}
	}
}
