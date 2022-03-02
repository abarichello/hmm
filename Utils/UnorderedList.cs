using System;
using System.Collections;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.Utils
{
	[Serializable]
	public class UnorderedList<T> : IEnumerable<T>, IEnumerable where T : class
	{
		public UnorderedList()
		{
			this._allObjectsList = new List<T>();
			this._disabledObjectsStack = new Stack<int>();
		}

		public UnorderedList(int listInitialSize)
		{
			this._listInitialSize = listInitialSize;
			this._allObjectsList = new List<T>(listInitialSize);
			this._disabledObjectsStack = new Stack<int>();
			for (int i = listInitialSize - 1; i >= 0; i--)
			{
				this._allObjectsList.Add((T)((object)null));
				this._disabledObjectsStack.Push(i);
			}
		}

		public void Add(T targetObject)
		{
			if (this._disabledObjectsStack.Count <= 0)
			{
				this._allObjectsList.Add(targetObject);
				return;
			}
			int num = this._disabledObjectsStack.Pop();
			if (num < 0 || num >= this._allObjectsList.Count)
			{
				return;
			}
			this._allObjectsList[num] = targetObject;
		}

		public void Remove(Predicate<T> match)
		{
			int num = this._allObjectsList.FindIndex(match);
			if (num == -1)
			{
				UnorderedList<T>.Log.ErrorFormat("Could not find object index using predicate.", new object[0]);
				return;
			}
			this.RemoveAt(num);
		}

		public void Remove(T targetObject)
		{
			int num = this.FindIndex(targetObject);
			if (num == -1)
			{
				UnorderedList<T>.Log.ErrorFormat("Could not find object index using targetObject.", new object[0]);
				return;
			}
			this.RemoveAt(num);
		}

		private int FindIndex(T targetObject)
		{
			int result = -1;
			for (int i = 0; i < this._allObjectsList.Count; i++)
			{
				if (this._allObjectsList[i] != null && this._allObjectsList[i] == targetObject)
				{
					result = i;
					break;
				}
			}
			return result;
		}

		private void RemoveAt(int index)
		{
			this._allObjectsList[index] = (T)((object)null);
			this._disabledObjectsStack.Push(index);
		}

		public void Clear()
		{
			this._disabledObjectsStack.Clear();
			for (int i = 0; i < this._allObjectsList.Count; i++)
			{
				if (this._allObjectsList[i] == null)
				{
					this.RemoveAt(i);
				}
				else
				{
					this.Remove(this._allObjectsList[i]);
				}
			}
		}

		public bool Contains(T item)
		{
			return item != null && this._allObjectsList.Contains(item);
		}

		public bool Exists(Predicate<T> match)
		{
			return this._allObjectsList.Exists(match);
		}

		public int Count()
		{
			return this._allObjectsList.Count - this._disabledObjectsStack.Count;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int index = 0; index < this._allObjectsList.Count; index++)
			{
				if (this._allObjectsList[index] != null)
				{
					yield return this._allObjectsList[index];
				}
			}
			yield break;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(UnorderedList<T>));

		private readonly List<T> _allObjectsList;

		private readonly Stack<int> _disabledObjectsStack;

		private readonly int _listInitialSize;
	}
}
