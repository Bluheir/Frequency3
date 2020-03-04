using Discord;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Frequency3.Logging
{
	public class Logger
	{
		private readonly StringWriter _text;

		public static Logger Instance { get; private set; }
		public string FilePath { get; }
		public string Text => _text.ToString();
		public Func<LogMessage, string> Action { get; }

		public Logger(string filePath, Func<LogMessage, string>? action = null, bool add = false)
		{
			Func<LogMessage, string> func = x =>
			{
				if (x.Source == "Gateway" && !x.Message.Contains("Ready"))
					return "";
				string text = $"{DateTime.Now} at {x.Source}] {x.Message}";
				Console.WriteLine(text);
				return text;
			};
			Action = action ?? func;
			_text = new StringWriter();
			FilePath = filePath;
			Directory.CreateDirectory(FilePath);
			if (add)
				Instance = this;
		}

		public async Task LogAsync(LogMessage message, bool newline = true)
		{
			if (newline)
				await _text.WriteLineAsync(Action(message));
			else
				await _text.WriteLineAsync(Action(message));
		}
		public void Log(LogMessage message, bool newline = true)
		{
			if (newline)
				_text.WriteLine(Action(message));
			else
				_text.Write(Action(message));
		}
		public void AddString(string text, bool newline = true)
		{
			if (newline)
				_text.WriteLine(text);
			else
				_text.Write(text);
		}
		public async Task AddStringAsync(string text, bool newline = true)
		{
			if (newline)
				await _text.WriteLineAsync(text);
			else
				await _text.WriteAsync(text);
		}
		public async Task<bool> SaveLogAsync(string? filePath = null)
		{
			try
			{
				await File.WriteAllTextAsync((filePath ?? FilePath) + $@"/Log {Directory.GetFiles(filePath ?? FilePath).ToList().Count}.txt", this + "");
				return true;
			}
			catch
			{
				return false;
			}
		}
		public bool SaveLog(string? filePath = null)
		{
			try
			{
				File.WriteAllText((filePath ?? FilePath) + $@"/Log {Directory.GetFiles(filePath ?? FilePath).ToList().Count}.txt", this + "");
				return true;
			}
			catch
			{
				return false;
			}
		}
		public override string ToString()
		=> Text;
	}
}
