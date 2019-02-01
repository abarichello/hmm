using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("NGUI/Interaction/Button Offset - HMM")]
	public class HMMUIToggleOffset : MonoBehaviour
	{
		public UIToggle Toggle
		{
			get
			{
				if (!this.toggle)
				{
					this.toggle = base.GetComponent<UIToggle>();
				}
				return this.toggle;
			}
			set
			{
				this.toggle = value;
			}
		}

		private void Start()
		{
			if (!this.mStarted)
			{
				this.mStarted = true;
				if (this.tweenTarget == null)
				{
					this.tweenTarget = base.transform;
				}
				this.Toggle = base.GetComponent<UIToggle>();
				EventDelegate.Add(this.Toggle.onChange, new EventDelegate.Callback(this.OnToggleChanged));
			}
		}

		private void OnDisable()
		{
			if (!this.Toggle.isColliderEnabled)
			{
				return;
			}
			if (this.mStarted && this.tweenTarget != null)
			{
				TweenPosition component = this.tweenTarget.GetComponent<TweenPosition>();
				if (component != null)
				{
					component.value = Vector3.zero;
					component.enabled = false;
				}
			}
		}

		public void OnToggleFocus()
		{
			if (this.mIsHover)
			{
				if (!this.Toggle.value)
				{
					TweenPosition.Begin(this.tweenTarget.gameObject, this.duration, this.hover).method = UITweener.Method.EaseInOut;
				}
				else
				{
					TweenPosition.Begin(this.tweenTarget.gameObject, this.duration, this.pressed).method = UITweener.Method.EaseInOut;
				}
			}
			else if (!this.Toggle.value)
			{
				TweenPosition.Begin(this.tweenTarget.gameObject, this.duration, Vector3.zero).method = UITweener.Method.EaseInOut;
			}
			else
			{
				TweenPosition.Begin(this.tweenTarget.gameObject, this.duration, this.pressed).method = UITweener.Method.EaseInOut;
			}
		}

		private void OnToggleChanged()
		{
			this.mIsPressed = this.Toggle.value;
			if (this.mIsPressed)
			{
				TweenPosition.Begin(this.tweenTarget.gameObject, this.duration, this.pressed).method = UITweener.Method.EaseInOut;
			}
			else
			{
				TweenPosition.Begin(this.tweenTarget.gameObject, this.duration, Vector3.zero).method = UITweener.Method.EaseInOut;
			}
			this.mIsPressed = false;
		}

		private void OnHover(bool isHover)
		{
			this.mIsHover = isHover;
			this.OnToggleFocus();
		}

		private UIToggle toggle;

		public Transform tweenTarget;

		public Vector3 hover = Vector3.zero;

		public Vector3 pressed = new Vector3(2f, -2f);

		public float duration = 0.2f;

		private bool mStarted;

		private bool mIsHover;

		private bool mIsPressed;
	}
}
