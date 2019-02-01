using System;
using System.Diagnostics;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class RepeatButton : MonoBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event RepeatButton.onRepeatButtonDelegate onRepeatButtonEvent;

		private void OnPress(bool isPressed)
		{
			this._isPressed = isPressed;
			this._nextClickTime = Time.realtimeSinceStartup + this._eventTriggerInterval;
		}

		private void Update()
		{
			if (this._isPressed && Time.realtimeSinceStartup < this._nextClickTime)
			{
				this._nextClickTime = Time.realtimeSinceStartup + this._eventTriggerInterval;
				if (this.onRepeatButtonEvent != null)
				{
					this.onRepeatButtonEvent();
				}
			}
		}

		[SerializeField]
		private float _eventTriggerInterval = 0.1f;

		private bool _isPressed;

		private float _nextClickTime;

		public delegate void onRepeatButtonDelegate();
	}
}
