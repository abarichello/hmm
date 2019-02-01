using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Obsolete]
	public class CharactersItem : GameHubBehaviour
	{
		public static int CompareCharactersName(CharactersItem c1, CharactersItem c2)
		{
			return c1.gameObject.name.CompareTo(c2.gameObject.name);
		}

		public static int CompareInvertCharactersName(CharactersItem c1, CharactersItem c2)
		{
			return c2.gameObject.name.CompareTo(c1.gameObject.name);
		}

		public virtual void CleanUp()
		{
			this.carTexture.sprite2D = null;
			if (this.icon != null)
			{
				this.icon.sprite2D = null;
			}
		}

		public void SetSpriteNames(string iconSpriteName, string carSpriteName)
		{
			this._iconSpriteName = iconSpriteName;
			this._carSpriteName = carSpriteName;
		}

		public void LoadDynamicSprites()
		{
			if (!string.IsNullOrEmpty(this._iconSpriteName) && this.icon != null)
			{
				this.icon.SpriteName = this._iconSpriteName;
			}
			if (!string.IsNullOrEmpty(this._carSpriteName))
			{
				this.carTexture.SpriteName = this._carSpriteName;
			}
		}

		public GUIEventListener button;

		public HMMUI2DDynamicSprite icon;

		public HMMUI2DDynamicSprite carTexture;

		public UILabel characterName;

		public UILabel characterType;

		public ItemTypeScriptableObject CharacterItemTypeScriptableObject;

		public UILabel softPrice;

		public UILabel hardPrice;

		public UI2DSprite softIcon;

		public UI2DSprite hardIcon;

		public GameObject boughtGO;

		public GameObject unboughtGO;

		public GameObject notPurchasableGO;

		public UILabel categoryLabel;

		public UILabel descriptionLabel;

		public UIButton previewButton;

		public UI2DSprite Border;

		private string _iconSpriteName;

		private string _carSpriteName;
	}
}
