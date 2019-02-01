using System;
using System.Collections;
using System.Collections.Generic;

namespace HeavyMetalMachines.Utils
{
	public class BiDictionary<T1, T2>
	{
		public void Add(T1 key, T2 value)
		{
			this._first2Second.Add(key, value);
			this._second2First.Add(value, key);
		}

		public bool ContainsKey(T1 key)
		{
			return this._first2Second.ContainsKey(key);
		}

		public bool ContainsKey(T2 key)
		{
			return this._second2First.ContainsKey(key);
		}

		public ICollection<T1> Keys
		{
			get
			{
				return this._first2Second.Keys;
			}
		}

		public bool Remove(T1 key)
		{
			this._second2First.Remove(this._first2Second[key]);
			return this._first2Second.Remove(key);
		}

		public bool Remove(T2 key)
		{
			this._first2Second.Remove(this._second2First[key]);
			return this._second2First.Remove(key);
		}

		public bool TryGetValue(T1 key, out T2 value)
		{
			return this._first2Second.TryGetValue(key, out value);
		}

		public bool TryGetValue(T2 key, out T1 value)
		{
			return this._second2First.TryGetValue(key, out value);
		}

		public ICollection<T2> Values
		{
			get
			{
				return this._first2Second.Values;
			}
		}

		public T2 this[T1 key]
		{
			get
			{
				return this._first2Second[key];
			}
			set
			{
				this._first2Second[key] = value;
			}
		}

		public T1 this[T2 key]
		{
			get
			{
				return this._second2First[key];
			}
			set
			{
				this._second2First[key] = value;
			}
		}

		public IEnumerator GetEnumerator()
		{
			return this._first2Second.GetEnumerator();
		}

		public void Clear()
		{
			this._first2Second.Clear();
			this._second2First.Clear();
		}

		public int Count
		{
			get
			{
				return this._first2Second.Count;
			}
		}

		private readonly Dictionary<T1, T2> _first2Second = new Dictionary<T1, T2>();

		private readonly Dictionary<T2, T1> _second2First = new Dictionary<T2, T1>();
	}
}
