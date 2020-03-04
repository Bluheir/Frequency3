using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Frequency3.Helpers.ChrisDatabase
{
	public class ChrisDB<T>
	{
		private readonly byte[] _data;
		private readonly string filePath;
		private readonly T value;


		public ChrisDB(string path)
		{
			if (path == null)
				throw new ArgumentNullException($"Parameter {nameof(path)} cannot be null.");
			if(!File.Exists(path))
				throw new ArgumentException($"File {path} doesn't exist.");
			
			_data = File.ReadAllBytes(path);
		}
	}
}
