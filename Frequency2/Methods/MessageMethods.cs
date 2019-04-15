namespace Frequency2.Methods
{
	public static class MessageMethods
	{
		public static string GetError(uint errorCode)
		{
			if (!Frequency2.Types.Commands.CommandErrorFix.Errors.TryGetValue(errorCode, out Types.Commands.CommandErrorFix value))
				return GetError(303);
			return $"Error: {errorCode} `{value}`";
		}
	}
}
