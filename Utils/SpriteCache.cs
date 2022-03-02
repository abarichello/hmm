using System;
using System.Collections.Generic;
using Hoplon.Unity.Loading;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class SpriteCache : IDynamicAssetListener<Texture2D>
	{
		public SpriteCache(int capacity)
		{
			this._cache = new SpriteCacheDictionaryContainer<string>(capacity);
		}

		public Sprite Get(string key)
		{
			return this._cache.Get(key);
		}

		public bool TryGetValue(string key, out Sprite value)
		{
			return this._cache.TryGetValue(key, out value);
		}

		public void PreCacheAsync(string spriteName)
		{
			if (string.IsNullOrEmpty(spriteName))
			{
				return;
			}
			bool flag;
			if (!this._loadingAssets.TryGetValue(spriteName, out flag))
			{
				this._loadingAssets.Add(spriteName, true);
			}
			else if (flag)
			{
				return;
			}
			if (!Loading.TextureManager.GetAssetAsync(spriteName, this))
			{
				this._loadingAssets[spriteName] = false;
			}
		}

		public void OnAssetLoaded(string spriteName, Texture2D texture)
		{
			if (texture == null)
			{
				return;
			}
			Sprite value;
			if (this._cache.TryGetValue(spriteName, out value))
			{
				return;
			}
			value = Sprite.Create(texture, new Rect(0f, 0f, (float)texture.width, (float)texture.height), Vector2.zero);
			this._cache.Add(spriteName, value);
			this._loadingAssets[spriteName] = false;
		}

		private readonly ISpriteCacheContainer<string> _cache;

		private readonly Dictionary<string, bool> _loadingAssets = new Dictionary<string, bool>(1024);
	}
}
