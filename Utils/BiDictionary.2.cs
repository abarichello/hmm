using System;
using System.Collections;
using System.Collections.Generic;

namespace HeavyMetalMachines.Utils
{
	public class BiDictionary<T>
	{
		public T this[T key]
		{
			get
			{
				return this._dict[key];
			}
			set
			{
				this._dict[key] = value;
				this._dict[value] = key;
			}
		}

		public void Add(T key, T value)
		{
			this._dict.Add(key, value);
			this._dict.Add(value, key);
		}

		public bool ContainsKey(T key)
		{
			return this._dict.ContainsKey(key);
		}

		public ICollection<T> Keys
		{
			get
			{
				return this._dict.Keys;
			}
		}

		public bool Remove(T key)
		{
			this._dict.Remove(this._dict[key]);
			return this._dict.Remove(key);
		}

		public bool TryGetValue(T key, out T value)
		{
			return this._dict.TryGetValue(key, out value);
		}

		public ICollection<T> Values
		{
			get
			{
				return this._dict.Values;
			}
		}

		public IEnumerator GetEnumerator()
		{
			return this._dict.GetEnumerator();
		}

		public void Clear()
		{
			this._dict.Clear();
		}

		public int Count
		{
			get
			{
				return this._dict.Count;
			}
		}

		private readonly Dictionary<T, T> _dict = new Dictionary<T, T>();
	}
}
