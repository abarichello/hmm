using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class UiButtonSpriteSwapper
	{
		public void SetColorHighlighted()
		{
			this.SetSprite(this.HighlightedSprite);
		}

		public void SetColorNormal()
		{
			this.SetSprite(this.NormalSprite);
		}

		public void SetColorPressed()
		{
			this.SetSprite(this.PressedSprite);
		}

		public void SetColorDisabled()
		{
			this.SetSprite(this.DisabledSprite);
		}

		private void SetSprite(Sprite sprite)
		{
			if (null != sprite)
			{
				this.Target.sprite = sprite;
			}
		}

		public Image Target;

		public Sprite NormalSprite;

		public Sprite HighlightedSprite;

		public Sprite PressedSprite;

		public Sprite DisabledSprite;
	}
}
