using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMSpriteFill : MonoBehaviour
	{
		private void Update()
		{
			this.Sprite.fillAmount = this.FillValue;
		}

		public float FillValue;

		public UIBasicSprite Sprite;
	}
}
