using Discord;


namespace Frequency2.Types.Messages
{
	public class Page
	{
		public Embed Embed { get; }
		public string Content { get; }

		public Page(Embed embed, string content = "")
		{
			Embed = embed;
			Content = content;
		}
	}
}
