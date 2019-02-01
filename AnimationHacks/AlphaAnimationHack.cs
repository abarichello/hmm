using System;
using UnityEngine;

namespace HeavyMetalMachines.AnimationHacks
{
	public class AlphaAnimationHack : MonoBehaviour
	{
		private void LateUpdate()
		{
			if (Time.unscaledTime < this._nextUiUpdateTime && Application.isPlaying)
			{
				return;
			}
			if (Mathf.Approximately(this.Alpha, this._lastAlpha))
			{
				return;
			}
			this.Widget.alpha = this.Alpha;
			this._lastAlpha = this.Alpha;
		}

		[Range(0f, 1f)]
		public float Alpha;

		public UIWidget Widget;

		private float _lastAlpha;

		private float _nextUiUpdateTime = -1f;
	}
}
