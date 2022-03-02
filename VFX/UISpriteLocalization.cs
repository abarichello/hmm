using System;
using Pocketverse;

namespace HeavyMetalMachines.VFX
{
	public class UISpriteLocalization : GameHubBehaviour
	{
		private void Start()
		{
			this._uiSprite = base.GetComponent<UISprite>();
			this.ApplyText();
		}

		private void ApplyText()
		{
			int num = -1;
			int num2 = -1;
			while (++num < this.Sprites.Length && num2 == -1)
			{
				if (this.Sprites[num].Language == Language.CurrentLanguage)
				{
					num2 = num;
				}
			}
			if (num2 == -1)
			{
				return;
			}
			this._uiSprite.spriteName = this.Sprites[num2].SpriteName;
		}

		private UISprite _uiSprite;

		public UISpriteLocalization.SritePerLanguage[] Sprites;

		[Serializable]
		public class SritePerLanguage
		{
			public string SpriteName;

			public LanguageCode Language;
		}
	}
}
