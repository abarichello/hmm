using System;
using System.Diagnostics;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudGadgetAnimationEvent : MonoBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudGadgetAnimationEvent.OnExitDelegate OnExitEvent;

		private void AnimationOnWindowExit()
		{
			if (this.OnExitEvent != null)
			{
				this.OnExitEvent();
			}
		}

		public delegate void OnExitDelegate();
	}
}
