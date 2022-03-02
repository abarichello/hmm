using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public abstract class HudFeedObject<T> : GameHubBehaviour where T : HudFeedObject<T>.HudFeedData
	{
		[HideInInspector]
		public bool IsActive
		{
			get
			{
				return this.Data != null;
			}
		}

		public abstract T Data { get; protected set; }

		public abstract void Setup(T data);

		public bool Visible
		{
			get
			{
				return this.CanvasGroup != null && this.CanvasGroup.alpha > 0f;
			}
		}

		public virtual void FeedUpdate(float timeoutDeltaInSec)
		{
			if (!this.Visible)
			{
				return;
			}
			if (this.Data == null)
			{
				if (!this.OutAnimation.isPlaying)
				{
					this.CanvasGroup.alpha = 0f;
				}
				return;
			}
			T data = this.Data;
			if (data.ShouldHide(timeoutDeltaInSec))
			{
				this.AnimateOut();
			}
		}

		public void AnimateIn()
		{
			if (!this.Visible)
			{
				return;
			}
			GUIUtils.AnimationSetFirstFrame(this.OutAnimation);
			GUIUtils.PlayAnimation(this.InAnimation, false, 1f, this.InAnimationClip.name);
		}

		public void AnimateOut()
		{
			this.Data = (T)((object)null);
			if (!this.Visible)
			{
				return;
			}
			GUIUtils.PlayAnimation(this.OutAnimation, false, 1f, this.OutAnimationClip.name);
		}

		public void AnimateDown()
		{
			if (!this.Visible)
			{
				return;
			}
			GUIUtils.AnimationSetLastFrame(this.InAnimation);
			GUIUtils.AnimationSetFirstFrame(this.OutAnimation);
			GUIUtils.PlayAnimation(this.DownAnimation, false, 1f, this.DownAnimationClip.name);
		}

		public Animation InAnimation;

		public Animation OutAnimation;

		public Animation DownAnimation;

		public AnimationClip InAnimationClip;

		public AnimationClip OutAnimationClip;

		public AnimationClip DownAnimationClip;

		public CanvasGroup CanvasGroup;

		public class HudFeedData
		{
			public HudFeedData()
			{
				this._startTimeInSec = Time.time;
			}

			public bool ShouldHide(float timeoutDeltaInSec)
			{
				return Time.time > this._startTimeInSec + timeoutDeltaInSec;
			}

			private readonly float _startTimeInSec;
		}
	}
}
