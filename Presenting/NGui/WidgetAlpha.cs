using System;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class WidgetAlpha : IAlpha
	{
		public float Alpha
		{
			get
			{
				return this._widget.alpha;
			}
			set
			{
				this._widget.alpha = value;
			}
		}

		[SerializeField]
		private UIWidget _widget;
	}
}
