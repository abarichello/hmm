using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudTopBar : MonoBehaviour
	{
		public void OpenTopBar()
		{
			this.Windowanimation[this.Windowanimation.clip.name].normalizedSpeed = 1f;
			this.Windowanimation.Sample();
			this.Windowanimation.Play();
		}

		public void CloseTopBar()
		{
			this.Windowanimation[this.Windowanimation.clip.name].normalizedSpeed = -1f;
			this.Windowanimation.Sample();
			this.Windowanimation.Play();
		}

		[SerializeField]
		private Animation Windowanimation;
	}
}
