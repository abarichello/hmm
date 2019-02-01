using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	public class NGUIWidgetColor : MonoBehaviour
	{
		public void Update()
		{
			int frameCount = Time.frameCount;
			if (this.mUpdateFrame != frameCount)
			{
				this.mUpdateFrame = frameCount;
				this.widget.color = this.Color;
			}
		}

		public void OnEnable()
		{
			this.widget.color = this.Color;
		}

		public Color Color;

		public UIWidget widget;

		[NonSerialized]
		private int mUpdateFrame = -1;
	}
}
