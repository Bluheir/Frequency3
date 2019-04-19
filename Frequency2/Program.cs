namespace Frequency2
{
	class Program
	{
		private static void Main()
		=> Source.Frequency2Client.Instance.StartAsync().GetAwaiter().GetResult();
	}
}
