using Discord;
using System;
using System.IO;
using System.Text;

namespace Frequency2.Source
{
	public class Logger
	{
		public static Logger Instance { get; private set; }

		public string Text => _text.ToString();

		private readonly StringBuilder _text;
		public Func<LogMessage, string> Action { get; }

		public Logger(Func<LogMessage, string> action = null, bool add = false)
		{
			Func<LogMessage, string> func = x =>
			{
				string text = $"{DateTime.Now} at {x.Source}] {x.Message}";
				Console.WriteLine(text);
				return text;
			};
			Action = action ?? func;
			_text = new StringBuilder();
			if (add)
				Instance = this;
		}

		public void Log(LogMessage message, bool newline = true)
		=> _text.Append(Action(message) + (newline ? "\n" : ""));

		public void AddString(string text, bool newline = true)
		=> _text.Append(text + (newline ? "\n" : ""));

		public override string ToString()
		=> Text;
	}
}
