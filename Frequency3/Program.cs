using Frequency3.Core;

namespace Frequency3
{
	class Program
	{
		private static void Main()
		=> new Frequency3Client().StartAsync().GetAwaiter().GetResult();
	}
}
