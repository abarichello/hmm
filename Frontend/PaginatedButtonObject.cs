using System;
using System.Diagnostics;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PaginatedButtonObject : BaseMonoBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event PaginatedButtonObject.OnButtonClickDelegate _onButtonClickEvent;

		public void Configure(PaginatedButtonObject.OnButtonClickDelegate onButtonClickEvent)
		{
			this._onButtonClickEvent = onButtonClickEvent;
			if (this.Button)
			{
				EventDelegate.Add(this.Button.onClick, new EventDelegate.Callback(this.OnButtonClick));
			}
			if (this.ButtonToggledSprite)
			{
				this.ButtonToggledSprite.gameObject.SetActive(false);
			}
			if (this.ProgressBar)
			{
				this._tweenProgressBar = this.ProgressBar.GetComponent<NGUITweenProgressBar>();
				if (!this._tweenProgressBar)
				{
					this._tweenProgressBar = this.ProgressBar.gameObject.AddComponent<NGUITweenProgressBar>();
				}
			}
		}

		protected void Ondestroy()
		{
			this._onButtonClickEvent = null;
		}

		public void ResetProgressBar()
		{
			if (this.ProgressBar)
			{
				this.ProgressBar.value = 0f;
				this.ProgressBar.alpha = 0f;
			}
			if (this._tweenProgressBar)
			{
				this._tweenProgressBar.value = 0f;
				this._tweenProgressBar.from = 0f;
				this._tweenProgressBar.to = 0f;
			}
		}

		public void OnButtonClick()
		{
			this.ToggleButton(true);
			this._onButtonClickEvent();
		}

		public void ToggleButton(bool toggleOn)
		{
			if (this.ButtonToggledSprite)
			{
				this.ButtonToggledSprite.gameObject.SetActive(toggleOn);
			}
		}

		public void SetProgressBarAnimation(float from, float to, float time)
		{
			if (this._tweenProgressBar)
			{
				this.ProgressBar.alpha = 1f;
				this._tweenProgressBar.from = from;
				NGUITweenProgressBar.Begin(this._tweenProgressBar.gameObject, time, to);
			}
		}

		public void SetColor(Color color)
		{
			if (this.Label)
			{
				this.Label.color = color;
			}
			if (this.Button)
			{
				this.Button.defaultColor = color;
			}
		}

		public void SetButtonState(bool enable, Color color)
		{
			if (this.Button)
			{
				this.Button.gameObject.GetComponent<Collider>().enabled = enable;
			}
			this.SetColor(color);
			this.ResetProgressBar();
		}

		public UILabel Label;

		public UIButton Button;

		public UI2DSprite ButtonToggledSprite;

		public UIProgressBar ProgressBar;

		private NGUITweenProgressBar _tweenProgressBar;

		public delegate void OnButtonClickDelegate();
	}
}
