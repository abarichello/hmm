using System;
using UnityEngine;

namespace HeavyMetalMachines.AnimationHacks
{
	[ExecuteInEditMode]
	public class AlphaAnimationPanelHack : MonoBehaviour
	{
		private void LateUpdate()
		{
			if (this.Panel == null)
			{
				return;
			}
			if (Mathf.Approximately(this.Alpha, this._lastAlpha))
			{
				return;
			}
			this.Panel.alpha = this.Alpha;
			this._lastAlpha = this.Alpha;
		}

		[Range(0f, 1f)]
		public float Alpha;

		public UIPanel Panel;

		private float _lastAlpha;
	}
}
