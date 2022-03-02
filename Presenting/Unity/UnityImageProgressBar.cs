using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityImageProgressBar : IProgressBar
	{
		public UnityImageProgressBar(Image image)
		{
			this._image = image;
		}

		public float FillPercent
		{
			get
			{
				return this._image.fillAmount;
			}
			set
			{
				this._image.fillAmount = value;
			}
		}

		[SerializeField]
		private Image _image;
	}
}
