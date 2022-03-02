using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class SpriteCacheDictionaryContainer<TKey> : ISpriteCacheContainer<TKey>
	{
		public SpriteCacheDictionaryContainer(int capacity)
		{
			this._container = new Dictionary<TKey, Sprite>();
		}

		public void Add(TKey key, Sprite value)
		{
			this._container.Add(key, value);
		}

		public bool TryGetValue(TKey key, out Sprite value)
		{
			return this._container.TryGetValue(key, out value);
		}

		public Sprite Get(TKey key)
		{
			return this._container[key];
		}

		private readonly Dictionary<TKey, Sprite> _container;
	}
}
