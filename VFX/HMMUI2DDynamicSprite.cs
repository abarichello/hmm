using System;
using HeavyMetalMachines.Utils;
using Hoplon.Unity.Loading;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMUI2DDynamicSprite : UI2DSprite, IDynamicAssetListener<Texture2D>
	{
		public string SpriteName
		{
			get
			{
				if (!string.IsNullOrEmpty(this._spriteName))
				{
					return this._spriteName;
				}
				if (base.sprite2D)
				{
					this._spriteName = base.sprite2D.name;
					return this._spriteName;
				}
				return string.Empty;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					this._spriteName = string.Empty;
					return;
				}
				this._spriteName = value.ToLower();
				if (!Loading.TextureManager.GetAssetAsync(this._spriteName, this))
				{
					Debug.Assert(false, string.Format("HMMUI2DDynamicSprite: Image/Bundle not found -> [{0},{1}]", base.name, this._spriteName), Debug.TargetTeam.GUI);
				}
			}
		}

		public void OnAssetLoaded(string textureName, Texture2D texture)
		{
			if (texture == null)
			{
				Debug.Assert(false, string.Format("[HMMUI2DDynamicSprite.OnTextureLoaded] Image failed to load -> {0}. GameObject: {1}", this._spriteName, base.gameObject.name), Debug.TargetTeam.GUI);
				return;
			}
			try
			{
				if (null == base.gameObject)
				{
					HMMUI2DDynamicSprite.Log.WarnFormat("[HMMUI2DDynamicSprite.OnTextureLoaded] QAHMM-18275 GamObject was destroyed. Sprite name: {0}.", new object[]
					{
						this._spriteName
					});
				}
				else
				{
					base.sprite2D = Sprite.Create(texture, new Rect(0f, 0f, (float)texture.width, (float)texture.height), Vector2.zero);
					if (this.ResizeAfterLoad)
					{
						base.SetDimensions((int)base.sprite2D.rect.width, (int)base.sprite2D.rect.height);
					}
				}
			}
			catch (NullReferenceException e)
			{
				HMMUI2DDynamicSprite.Log.WarnFormat("Exception on DynamicSprite, probably the same as issue QAHMM-19705. Sprite name: {0}", new object[]
				{
					this._spriteName
				});
				HMMUI2DDynamicSprite.Log.Warn("Need to find a way to reproduce this:", e);
			}
		}

		public string GetAssetName()
		{
			return this.SpriteName;
		}

		public void ClearSprite()
		{
			this._spriteName = string.Empty;
			base.sprite2D = null;
		}

		public Color gradientTop
		{
			get
			{
				return this.mGradientTop;
			}
			set
			{
				if (this.mGradientTop != value)
				{
					this.mGradientTop = value;
					if (this.mApplyGradient)
					{
						this.MarkAsChanged();
					}
				}
			}
		}

		public Color gradientBottom
		{
			get
			{
				return this.mGradientBottom;
			}
			set
			{
				if (this.mGradientBottom != value)
				{
					this.mGradientBottom = value;
					if (this.mApplyGradient)
					{
						this.MarkAsChanged();
					}
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HMMUI2DDynamicSprite));

		private string _spriteName;

		public bool ResizeAfterLoad;
	}
}
