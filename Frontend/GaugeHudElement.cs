using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class GaugeHudElement : MonoBehaviour
	{
		public void SetValue(int min, int max, int current)
		{
			float fillAmount = (float)(current - min) / (float)(max - min);
			this._slider.fillAmount = fillAmount;
			this._label.text = current.ToString();
		}

		public void PlayAnimation(string animationName)
		{
			this._animation.Play(animationName);
		}

		[SerializeField]
		private UI2DSprite _slider;

		[SerializeField]
		private UILabel _label;

		[SerializeField]
		private Animation _animation;
	}
}
