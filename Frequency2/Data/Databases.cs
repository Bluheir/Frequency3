using Frequency2.Data.Models;

namespace Frequency2.Data
{
	public static class Databases
	{
		public static DataBase<long, UserData> Users { get; internal set; }
	}
}
