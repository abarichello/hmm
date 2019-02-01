using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	public class NGUISimpleProgressBarAnchor : MonoBehaviour
	{
		public void Update()
		{
			if (this.ProgressBarTarget == null)
			{
				return;
			}
			if (this.ProgressBarTarget.foregroundWidget == null)
			{
				return;
			}
			float num = this.ProgressBarTarget.value * (float)this.ProgressBarTarget.foregroundWidget.width + this.ProgressBarTarget.transform.localPosition.x + this.ProgressBarTarget.foregroundWidget.transform.localPosition.x;
			if (this.lastXpos != num)
			{
				this.lastXpos = num;
				base.transform.localPosition = new Vector3(this.lastXpos, base.transform.localPosition.y, base.transform.localPosition.z);
			}
		}

		public float lastXpos = -1f;

		public UIProgressBar ProgressBarTarget;
	}
}
