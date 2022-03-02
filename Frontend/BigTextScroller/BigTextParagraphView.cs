using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Frontend.BigTextScroller
{
	public class BigTextParagraphView : EnhancedScrollerCellView, IItemView
	{
		public IIdentifiable Model { get; set; }

		public float Height
		{
			get
			{
				return this._label.Height;
			}
		}

		public ILabel Label
		{
			get
			{
				return this._label;
			}
		}

		[SerializeField]
		private UnityLabel _label;
	}
}
