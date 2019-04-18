using System;
using System.Collections.Generic;
using System.Text;

namespace Frequency2.Data.Models
{
	public class UserData : ISaveable<long>
	{
		public long Key { get; set; }
		public bool SendCompMessage { get; set; }
		public string Prefix { get; set; }

	}
}
