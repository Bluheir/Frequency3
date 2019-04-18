namespace Frequency2
{
	class Program
	{
		private static void Main()
		=> new Source.Frequency2Client().StartAsync().GetAwaiter().GetResult();
	}
}
