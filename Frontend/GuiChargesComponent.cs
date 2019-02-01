using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class GuiChargesComponent
	{
		public void UpdateColors(bool enabled, bool atMaxCharges, bool forceUpdate)
		{
			if (enabled == this._enabled && atMaxCharges == this._atMaxCharges && !forceUpdate)
			{
				return;
			}
			if (this._baseSprite == null)
			{
				return;
			}
			this._enabled = enabled;
			this._atMaxCharges = atMaxCharges;
			GuiChargesComponent.ChargesComponentColors componentsColors = (!this._enabled) ? this._disabledColors : this._enabledColors;
			this.SetComponentsColors(componentsColors);
		}

		private void SetComponentsColors(GuiChargesComponent.ChargesComponentColors colors)
		{
			this._baseSprite.color = colors.BaseColor;
			this._outlineSprite.color = colors.OutlineColor;
			this._progressFillSprite.color = colors.ProgressFillColor;
			this._progressBgSprite.color = colors.ProgressBgColor;
			if (this._atMaxCharges)
			{
				this._chargesCountLabel.color = colors.MaxChargesCountColor;
				this._progressBorderSprite.color = colors.MaxChargesProgressBorderColor;
			}
			else
			{
				this._chargesCountLabel.color = colors.ChargesCountColor;
				this._progressBorderSprite.color = colors.ProgressBorderColor;
			}
		}

		[SerializeField]
		private UI2DSprite _baseSprite;

		[SerializeField]
		private UI2DSprite _outlineSprite;

		[SerializeField]
		private UILabel _chargesCountLabel;

		[SerializeField]
		private UI2DSprite _progressFillSprite;

		[SerializeField]
		private UI2DSprite _progressBorderSprite;

		[SerializeField]
		private UI2DSprite _progressBgSprite;

		[SerializeField]
		private GuiChargesComponent.ChargesComponentColors _enabledColors;

		[SerializeField]
		private GuiChargesComponent.ChargesComponentColors _disabledColors;

		private bool _enabled;

		private bool _atMaxCharges;

		[Serializable]
		private class ChargesComponentColors
		{
			public Color BaseColor;

			public Color OutlineColor;

			public Color ChargesCountColor;

			public Color MaxChargesCountColor;

			public Color ProgressFillColor;

			public Color ProgressBorderColor;

			public Color MaxChargesProgressBorderColor;

			public Color ProgressBgColor;
		}
	}
}
