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

		public void FeedUpdate(float timeoutDeltaInSec)
		{
			if (this.Data == null)
			{
				if (!this.OutAnimation.isPlaying)
				{
					base.gameObject.SetActive(false);
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
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			GUIUtils.AnimationSetFirstFrame(this.OutAnimation);
			GUIUtils.PlayAnimation(this.InAnimation, false, 1f, string.Empty);
		}

		public void AnimateOut()
		{
			this.Data = (T)((object)null);
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			GUIUtils.PlayAnimation(this.OutAnimation, false, 1f, string.Empty);
		}

		public void AnimateDown()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			GUIUtils.AnimationSetLastFrame(this.InAnimation);
			GUIUtils.AnimationSetFirstFrame(this.OutAnimation);
			GUIUtils.PlayAnimation(this.DownAnimation, false, 1f, string.Empty);
		}

		public Animation InAnimation;

		public Animation OutAnimation;

		public Animation DownAnimation;

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
