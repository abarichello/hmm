using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	public class NGUIWidgetSize : MonoBehaviour
	{
		public void Update()
		{
			int frameCount = Time.frameCount;
			if (this.mUpdateFrame != frameCount)
			{
				this.mUpdateFrame = frameCount;
				this.widget.width = Mathf.RoundToInt(this.Width);
				this.widget.height = Mathf.RoundToInt(this.Height);
			}
		}

		public void OnEnable()
		{
			this.widget.width = Mathf.RoundToInt(this.Width);
			this.widget.height = Mathf.RoundToInt(this.Height);
		}

		public float Width;

		public float Height;

		public UIWidget widget;

		[NonSerialized]
		private int mUpdateFrame = -1;
	}
}
