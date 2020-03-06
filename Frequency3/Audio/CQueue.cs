using System;
using System.Collections;
using System.Collections.Generic;

namespace Frequency3.Audio
{
	public class CQueue<T> : IList<T>
	{
		private readonly List<T> _items;
		private Random _random;
		private readonly object _lockObj;

		public T First
		{
			get
			{
				lock (_lockObj)
				{
					if (_items.Count == 0)
					{
						return default;
					}

					return _items[0];
				}
			}
		}
		public T this[int index]
		{
			get
			{
				lock (_lockObj)
				{
					return _items[index];
				}
			}
			set
			{
				lock (_lockObj)
				{
					_items[index] = value;
				}
			}
		}
		public int Count
		{
			get
			{
				lock (_lockObj)
				{
					return _items.Count;
				}
			}
		}

		public int RandomSeed
		{
			set
			{
				lock (_lockObj)
				{
					_random = new Random(value);
				}
			}
		}

		public int Capacity
		{
			get
			{
				lock (_lockObj)
				{
					return _items.Capacity;
				}
			}
		}

		public bool IsReadOnly => ((IList<T>)_items).IsReadOnly;

		public CQueue()
		{
			_items = new List<T>();
			_random = new Random();
			_lockObj = new object();
		}
		public CQueue(IEnumerable<T> collection)
		{
			_items = new List<T>(collection);
			_random = new Random();
			_lockObj = new object();
		}

		public void RemoveAt(int index)
		{
			lock (_lockObj)
			{
				_items.RemoveAt(index);
			}
		}
		public void ResetRandom()
		{
			lock (_lockObj)
			{
				_random = new Random();
			}
		}
		public void Insert(int index, T item)
		{
			lock (_lockObj)
			{
				_items.Insert(index, item);
			}
		}
		public void Remove(T value)
		{
			lock (_lockObj)
			{
				_items.Remove(value);
			}
		}
		public void Reverse()
		{
			lock (_lockObj)
			{
				_items.Reverse();
			}
		}
		public void QueueRange(IEnumerable<T> collection)
		{
			lock (_lockObj)
			{
				_items.AddRange(collection);
			}
		}
		public bool IndexExists(int index)
		{
			lock (_lockObj)
			{
				return (index < Count && index >= 0);
			}
		}
		public bool TryGetItem(int index, out T value)
		{
			lock (_lockObj)
			{
				if (!IndexExists(index))
				{
					value = default;
					return false;
				}
				value = _items[index];
				return true;
			}
		}
		public bool TryRemoveAt(int index, out T value)
		{
			lock (_lockObj)
			{
				if (!IndexExists(index))
				{
					value = default;
					return false;
				}
				value = _items[index];
				_items.RemoveAt(index);
				return true;
			}
		}
		public bool TryRemoveAt(int index)
		{
			lock (_lockObj)
			{
				if (!IndexExists(index))
				{
					return false;
				}
				_items.RemoveAt(index);
				return true;
			}
		}
		public void RemoveRange(int from, int to)
		{
			lock (_lockObj)
			{
				if (to > Count)
				{
					return;
				}
				for (; from < to; from++)
				{
					RemoveAt(from);
				}
			}
		}
		public T Dequeue()
		{
			lock (_lockObj)
			{
				T value = First;
				_items.RemoveAt(0);
				return value;
			}
		}
		public void Shuffle()
		{
			lock (_lockObj)
			{
				if (_items.Count >= 2)
				{
					int n = _items.Count;
					while (n > 1)
					{
						n--;
						int k = _random.Next(n + 1);
						T value = _items[k];
						_items[k] = _items[n];
						_items[n] = value;
					}
				}
			}
		}

		public void Clear()
		{
			lock (_lockObj)
			{
				_items.Clear();
			}
		}
		public void Enqueue(T value)
		{
			lock (_lockObj)
			{
				_items.Add(value);
			}
		}
		public int IndexOf(T item)
		{
			lock (_lockObj)
			{
				return _items.IndexOf(item);
			}
		}

		public bool Contains(T item)
		{
			lock (_lockObj)
			{
				return _items.Contains(item);
			}
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (_lockObj)
			{
				_items.CopyTo(array, arrayIndex);
			}
		}
		public bool TryRemove(T item)
		{
			lock (_lockObj)
			{
				if (!_items.Contains(item))
				{
					return false;
				}
				_items.Remove(item);
				return true;
			}
		}

		void ICollection<T>.Add(T item)
		{
			Enqueue(item);
		}

		bool ICollection<T>.Remove(T item)
		{
			return TryRemove(item);
		}

		public IEnumerator<T> GetEnumerator()
		{
			lock (_lockObj)
			{
				return _items.GetEnumerator();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
