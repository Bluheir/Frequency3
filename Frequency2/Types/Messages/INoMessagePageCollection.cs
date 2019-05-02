using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Frequency2.Types.Messages
{
	public interface INoMessagePageCollection : 
		IEnumerable<Page>,
		IEnumerable,
		IReadOnlyList<Page>,
		IReadOnlyCollection<Page>
	{
		IReadOnlyList<Page> Pages { get; }
		int CurrentPage { get; }
		new int Count { get; }

	}
}
