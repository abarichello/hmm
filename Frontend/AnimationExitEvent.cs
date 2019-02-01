using System;
using System.Diagnostics;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class AnimationExitEvent : MonoBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event AnimationExitEvent.OnExitDelegate OnExitEvent;

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
