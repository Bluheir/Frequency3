

using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Frequency2.Config;
using System.Collections.Concurrent;
using System;
using Microsoft.Extensions.DependencyInjection;
using Frequency2.Source;

namespace Frequency2
{
	class Program
	{
		private static void Main()
		=> new Frequency2Client().StartAsync().GetAwaiter().GetResult();
	}
}
