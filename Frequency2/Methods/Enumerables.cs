using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Victoria.Queue;
using System.Reflection;

namespace Frequency2.Methods
{
	public static class Enumerables
	{
		public static LavaQueue<T> AddFirst<T>(this LavaQueue<T> queue, T item) where T : IQueueObject
		{
			LinkedList<T> items = (LinkedList<T>)queue.GetType().GetField("_linked", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(queue);
			items.AddFirst(item);
			return queue;
		}

		public static LavaQueue<T> SetValue<T>(this LavaQueue<T> queue, IEnumerable<T> values) where T : IQueueObject
		{
			queue.GetType().GetField("_linked", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(queue, values);
			return queue;
		}
		public static LavaQueue<T> Insert<T>(this LavaQueue<T> queue, int index, T item) where T : IQueueObject
		{
			LinkedList<T> items = (LinkedList<T>)queue.GetType().GetField("_linked", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(queue);
			items.Insert(index, item);
			queue.GetType().GetField("_linked", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(queue, items );
			return queue;
		}
		public static void Insert<T>(this LinkedList<T> list, int index, T data)
		{
			List<T> items = list.ToList();
			list.Clear();

			if (index >= 0)
			{
				for (int i = 0; i < items.Count; i++)
				{
					if (index == i)
					{
						list.AddLast(data);
						i++;
					}
					list.AddLast(items[i]);
				}

			}
		}
	}
}
