using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("NGUI/Interaction/Button Offset - HMM")]
	public class HMMUIButtonOffset : MonoBehaviour
	{
		private void Start()
		{
			if (!this.mStarted)
			{
				this.mStarted = true;
				if (this.tweenTarget == null)
				{
					this.tweenTarget = base.transform;
				}
			}
		}

		private void OnEnable()
		{
			if (this.mStarted)
			{
				this.OnHover(UICamera.IsHighlighted(base.gameObject));
			}
			this._uiButton = base.GetComponent<UIButton>();
		}

		private void OnDisable()
		{
			if (this.mStarted && this.tweenTarget != null)
			{
				TweenPosition component = this.tweenTarget.GetComponent<TweenPosition>();
				if (component != null)
				{
					component.value = Vector3.zero;
					component.enabled = false;
				}
			}
			this._uiButton = null;
		}

		private void OnPress(bool isPressed)
		{
			if (base.enabled)
			{
				if (this._uiButton.state != UIButtonColor.State.Pressed)
				{
					return;
				}
				if (!this.mStarted)
				{
					this.Start();
				}
				TweenPosition.Begin(this.tweenTarget.gameObject, this.duration, (!isPressed) ? ((!UICamera.IsHighlighted(base.gameObject)) ? Vector3.zero : this.hover) : this.pressed).method = UITweener.Method.EaseInOut;
			}
		}

		private void OnHover(bool isOver)
		{
			if (base.enabled)
			{
				if (!this.mStarted)
				{
					this.Start();
				}
				TweenPosition.Begin(this.tweenTarget.gameObject, this.duration, (!isOver) ? Vector3.zero : this.hover).method = UITweener.Method.EaseInOut;
			}
		}

		private void OnSelect(bool isSelected)
		{
			if (base.enabled && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
			{
				this.OnHover(isSelected);
			}
		}

		public Transform tweenTarget;

		public Vector3 hover = Vector3.zero;

		public Vector3 pressed = new Vector3(2f, -2f);

		public float duration = 0.2f;

		private bool mStarted;

		private UIButton _uiButton;
	}
}
