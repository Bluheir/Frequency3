using System.Collections.Generic;

namespace Frequency2.Types.Commands
{
	public abstract class CommandErrorFix
	{
		public static IReadOnlyDictionary<uint, CommandErrorFix> Errors { get; } = new Dictionary<uint, CommandErrorFix>
		{
			{
				11,
				new NewCommandError("You are not in a voice channel", "This error is thrown when you try and make the bot join your voice channel when you aren't in a voice channel already.", "Be in a voice channel when executing this command")
			},
			{
				303,
				new NewCommandError("Unknown error", "This error is thrown if there is a bug in the code. Please contact the bot owner if you see this", "No fix")
			},
			{
				12,
				new NewCommandError("Invalid music permissions", "This error is thrown if you do not have permission to change the bots channel or change the current playing music", "Get the permissions")
			}
		};

		public abstract string ShortDescription { get; }
		public abstract string Description { get; }
		public abstract string Fix { get; }
		

		public override string ToString()
		=> ShortDescription;

		private class NewCommandError : CommandErrorFix
		{
			public override string ShortDescription { get; }
			public override string Description { get; }
			public override string Fix { get; }
			

			public NewCommandError(string shortdescription, string description, string fix)
			{
				ShortDescription = shortdescription;
				Description = description;
				Fix = fix;
			}
		}
	}

	
}
