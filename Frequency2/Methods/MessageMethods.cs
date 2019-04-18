using Discord;

namespace Frequency2.Methods
{
	public static class MessageMethods
	{
		public static string GetError(uint errorCode)
		{
			if (!Frequency2.Types.Commands.CommandErrorFix.Errors.TryGetValue(errorCode, out Types.Commands.CommandErrorFix value))
				return GetError(303);
			return $":x: Error: {errorCode} `{value}`";
		}

		public static bool IsPrivate(this IMessage message)
		=> message.Channel is IDMChannel;
	}
}
