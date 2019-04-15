using Discord.Commands;
using System.Threading.Tasks;

namespace Frequency2.Modules
{
	public class General : ModuleBase<SocketCommandContext>
	{
		
		[Command("ping", RunMode = RunMode.Async)]
		public async Task Pong()
		{
			await Context.Channel.SendMessageAsync("Pong");
			
		}
	}
}