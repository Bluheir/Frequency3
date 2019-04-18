using SQLite;

namespace Frequency2.Data
{
	public interface ISaveable<T>
	{
		[PrimaryKey]
		T Key { get; set; }
	}
}
