using System;
using HeavyMetalMachines.Presenting;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	public class NGUIPanelAlpha : MonoBehaviour, IAlpha
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

		public float Alpha
		{
			get
			{
				return this.alpha;
			}
			set
			{
				this.alpha = value;
			}
		}

		public float alpha;

		public UIPanel panel;

		[NonSerialized]
		private int mUpdateFrame = -1;
	}
}
