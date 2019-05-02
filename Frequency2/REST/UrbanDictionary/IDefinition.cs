

namespace Frequency2.REST.UrbanDictionary
{
	public interface IDefinition
	{
		string Definition { get; }
		int Thumbs_Up { get; }
		string Author { get; }
		string Word { get; }
		int DefinitionId { get; }
		string Current_Vote { get; }
		string Written_On { get; }
		string Example { get; }
		int Thumbs_Down { get; }
	}
}
