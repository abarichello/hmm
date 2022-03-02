using System;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class NGuiTextureProgressBar : IProgressBar
	{
		public float FillPercent
		{
			get
			{
				return this._texture.fillAmount;
			}
			set
			{
				this._texture.fillAmount = value;
			}
		}

		[SerializeField]
		private UITexture _texture;
	}
}
