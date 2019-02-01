using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMUIDragScrollView : UIDragScrollView
	{
		private void OnDrag(Vector2 delta)
		{
			if (!this.Drag)
			{
				return;
			}
			if (this.scrollView && NGUITools.GetActive(this))
			{
				this.scrollView.Drag();
			}
		}

		protected override void OnPress(bool pressed)
		{
			if (!this.Drag)
			{
				return;
			}
			base.OnPress(pressed);
		}

		public bool Drag;
	}
}
