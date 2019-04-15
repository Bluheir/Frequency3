using Discord;
using System;
using System.IO;
using System.Text;

namespace Frequency2._0.MainTypes
{
	public class Logger
	{
		public static Logger Instance { get; private set; }
		public StringBuilder Text { get; }
		public Func<LogMessage, string> Action { get; }

		public Logger(Func<LogMessage, string> action = null, bool add = true)
		{
			Func<LogMessage, string> func = x =>
			{
				string text = $"{DateTime.Now} at {x.Source}] {x.Message}";
				Console.WriteLine(text);
				return text;
			};
			Action = action ?? func;
			Text = new StringBuilder();
			if (add)
				Instance = this;
		}

		public void Log(LogMessage message, bool newline = true)
		=> Text.Append(Action(message) + (newline ? "\n" : ""));

		public void AddString(string text, bool newline = true)
		=> Text.Append(text + (newline ? "\n" : ""));
	}
}
