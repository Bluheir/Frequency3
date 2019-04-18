using System.Threading.Tasks;
using System.IO;
using SQLite;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Frequency2.Data
{

	public sealed class DataBase<TKey, TSaveable>
			where TSaveable : class, ISaveable<TKey>, new()
	{

		
		public string FilePath { get; private set; }
		private Func<TKey, TSaveable> _defaultVal;
		private SQLiteAsyncConnection database;

		public static async Task<DataBase<TKey, TSaveable>> Async(string filepath, Func<TKey, TSaveable> defaultValue)
		{
			bool fileOriginallyExists = File.Exists(filepath);
			var retVal = new DataBase<TKey, TSaveable>()
			{
				FilePath = filepath,
				_defaultVal = defaultValue,
			};
			
			retVal.database = new SQLiteAsyncConnection(filepath);

			
			await retVal.database.CreateTableAsync<TSaveable>();
			return retVal;
		}
	
		private DataBase(){}

		public async Task<TSaveable> GetValue(TKey key)
		{
			return await database.Table<TSaveable>()
				.Where(x => x.Key.Equals(key))
				.FirstOrDefaultAsync();
		}

		public async Task<bool> ContainsKey(TKey key)
		=> await GetValue(key) != null;
		public async Task<TSaveable> GetOrAddValue(TKey key)
		{
			if(!await ContainsKey(key))
			{
				await database.InsertAsync(_defaultVal(key), typeof(TSaveable));
				return _defaultVal(key);
			}
			return await GetValue(key);
		}

		public async Task<List<TSaveable>> GetValues()
		{
			return await database.Table<TSaveable>().ToListAsync();
		}

		public async Task SaveAsync(params TSaveable[] values)
		{
			foreach(var value in values)
			{
				await database.Table<TSaveable>().DeleteAsync(x => x.Key.Equals(value.Key));
				await database.InsertAsync(value);
			}
		}
		
	}
	//*/
}
