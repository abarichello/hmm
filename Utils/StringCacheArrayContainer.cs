using System;

namespace HeavyMetalMachines.Utils
{
	public class StringCacheArrayContainer : IStringCacheContainer<int>
	{
		public StringCacheArrayContainer(int capacity)
		{
			this._capacity = capacity;
			this._container = new string[capacity];
		}

		public void Add(int key, string value)
		{
			if (key < 0 || key >= this._capacity)
			{
				throw new IndexOutOfRangeException();
			}
			this._container[key] = value;
		}

		public bool TryGetValue(int key, out string value)
		{
			if (key < 0 || key >= this._capacity)
			{
				value = string.Empty;
				return false;
			}
			value = this._container[key];
			return !string.IsNullOrEmpty(value);
		}

		public string Get(int key)
		{
			return this._container[key];
		}

		private readonly int _capacity;

		private readonly string[] _container;
	}
}
