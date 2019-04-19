using Discord.Commands;
using System.Collections.Generic;
using System.Linq;

namespace Frequency2.Methods
{
	public static class CommandsReflection
	{
		public static string GetFullName(CommandInfo Command)
		=> getfullname(Command, Command.Module);
		private static string getfullname(CommandInfo command, ModuleInfo module, string f = "")
		{
			if(module == null)
			{
				return f + command.Name;
			}
			if(module.Group == null)
			{
				return f + command.Name;
			}
			f = module.Group + " ";
			
			return f + getfullname(command, module.Parent);


		}

		public static CommandInfo GetCommand(string commandname)
		=>	Frequency2.Source.Frequency2Client.Instance.CommandInfos.GetValueOrDefault(commandname);
		

		
	}
}
