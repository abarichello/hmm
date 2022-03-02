using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public interface ISpriteCacheContainer<TKey>
	{
		void Add(TKey key, Sprite value);

		bool TryGetValue(TKey key, out Sprite value);

		Sprite Get(TKey key);
	}
}
