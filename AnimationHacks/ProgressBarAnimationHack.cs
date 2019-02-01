using System;
using UnityEngine;

namespace HeavyMetalMachines.AnimationHacks
{
	public class ProgressBarAnimationHack : MonoBehaviour
	{
		private void Update()
		{
			if (this.Value != this._lastValue)
			{
				this.ProgressBar.value = this.Value;
				this._lastValue = this.Value;
			}
			if (this.Alpha != this._lastAlpha)
			{
				this.ProgressBar.alpha = this.Alpha;
				this._lastAlpha = this.Alpha;
			}
		}

		[Range(0f, 1f)]
		public float Value;

		[Range(0f, 1f)]
		public float Alpha;

		public UIProgressBar ProgressBar;

		private float _lastValue;

		private float _lastAlpha;
	}
}
