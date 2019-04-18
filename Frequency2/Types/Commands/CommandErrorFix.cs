using System.Collections.Generic;

namespace Frequency2.Types.Commands
{
	public sealed class CommandErrorFix
	{
		public static IReadOnlyDictionary<uint, CommandErrorFix> Errors { get; } = new Dictionary<uint, CommandErrorFix>
		{
			{
				11,
				new CommandErrorFix("You are not in a voice channel", "This error is thrown when you try and make the bot join your voice channel when you aren't in a voice channel already.", "Be in a voice channel when executing this command")
			},
			{
				303,
				new CommandErrorFix("Unknown error", "This error is thrown if there is a bug in the code. Please contact the bot owner if you see this", "No fix")
			},
			{
				12,
				new CommandErrorFix("Invalid music permissions or I am already in a channel and you are not", "This error is thrown if you do not have permission to change the bots channel or change the current playing music, or the bot is in a channel and you have the permissions but you are not in a voice channel", "Get the permissions or be in a voice channel")
			},
			{
				14,
				new CommandErrorFix("Cannot do a bot owner only command", "This error is thrown when you attempt to execute a bot owner only command. As it suggests, only the bot owner can execute this command", "Hack the bot owners account and execute this command")
			},
			{
				15,
				new CommandErrorFix("Cannot find that error", "This error is thrown if you attempt to search an error and it doesn't exist", "Search a proper error")
			},
			{
				16,
				new CommandErrorFix("Too long prefix", "This error is thrown if you attempt to change your old prefix to a prefix longer than 10 characters", "Change your old prefix to a prefix shorter than 10 or less characters")
			},
			{
				17,
				new CommandErrorFix("Not playing a song", "This error is thrown when you attempt to pause a song when none is playing, or when you attempt to attempt to pause a song when I am not in a voice channel", "Execute that command when a song is playing")
			}
		};

		public string ShortDescription { get; }
		public string Description { get; }
		public string Fix { get; }
		
		public override string ToString()
		=> ShortDescription;
		private CommandErrorFix()
		{}
		private CommandErrorFix(string shortdescription, string description, string fix)
		{
			ShortDescription = shortdescription;
			Description = description;
			Fix = fix;
		}
		
	}

	
}
