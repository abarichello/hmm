using System;
using System.Diagnostics;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudFeedbackExitEvent : MonoBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudFeedbackExitEvent.OnExitDelegate OnExitEvent;

		private void AnimationOnWindowExit()
		{
			if (this.OnExitEvent != null)
			{
				this.OnExitEvent(this);
			}
		}

		public HudFeedbackExitEvent.FeedbackType Type;

		public enum FeedbackType
		{
			Center,
			Left,
			Mega
		}

		public delegate void OnExitDelegate(HudFeedbackExitEvent exitEvent);
	}
}
