using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	public class NGUIPanelAlpha : MonoBehaviour
	{
		public void Update()
		{
			int frameCount = Time.frameCount;
			if (this.mUpdateFrame != frameCount)
			{
				this.mUpdateFrame = frameCount;
				this.panel.alpha = this.alpha;
			}
		}

		public void OnEnable()
		{
			this.panel.alpha = this.alpha;
		}

		public float alpha;

		public UIPanel panel;

		[NonSerialized]
		private int mUpdateFrame = -1;
	}
}
