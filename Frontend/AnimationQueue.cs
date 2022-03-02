using System;
using System.Collections.Generic;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class AnimationQueue
	{
		public void Queue(Animation anim, string animationClipName, Action onAnimationStopped = null, float speedMultiplier = 1f)
		{
			AnimationQueue.QueuedObject queuedObject = new AnimationQueue.QueuedObject();
			queuedObject.Animation = anim;
			queuedObject.AnimationClipName = animationClipName;
			queuedObject.AnimationSpeedMultiplier = speedMultiplier;
			queuedObject.OnAnimationStopped = onAnimationStopped;
			this._animationsList.Add(queuedObject);
			if (anim.gameObject.activeInHierarchy && !this._animationsList[0].Animation.isPlaying)
			{
				this.PlayNextAnimation();
			}
		}

		private void PlayAnimation(AnimationQueue.QueuedObject queuedObject)
		{
			queuedObject.AnimationStarted = true;
			GUIUtils.PlayAnimation(queuedObject.Animation, false, queuedObject.AnimationSpeedMultiplier, queuedObject.AnimationClipName);
		}

		public void Update()
		{
			if (this._animationsList.Count <= 0 || this.IsPlaying())
			{
				return;
			}
			if (!this._animationsList[0].AnimationStarted)
			{
				if (this._animationsList[0].Animation.gameObject.activeInHierarchy)
				{
					this.PlayNextAnimation();
				}
				return;
			}
			this.RemoveCurrentAnimation();
			this.PlayNextAnimation();
		}

		public int QueueSize()
		{
			return this._animationsList.Count;
		}

		public bool IsPlaying()
		{
			return this._animationsList.Count > 0 && this._animationsList[0].Animation.isPlaying;
		}

		private void PlayNextAnimation()
		{
			if (this._animationsList.Count > 0)
			{
				this.PlayAnimation(this._animationsList[0]);
			}
		}

		public void StoploopinAnimation()
		{
			if (this._animationsList.Count <= 0 || !this._animationsList[0].Animation.isPlaying || this._animationsList[0].Animation[this._animationsList[0].AnimationClipName].clip.wrapMode != 2)
			{
				return;
			}
			this._animationsList[0].Animation.Stop(this._animationsList[0].AnimationClipName);
			this._animationsList[0].Animation.Rewind(this._animationsList[0].AnimationClipName);
			this._animationsList[0].Animation.Sample();
			this.RemoveCurrentAnimation();
			this.PlayNextAnimation();
		}

		private void RemoveCurrentAnimation()
		{
			if (this._animationsList.Count <= 0)
			{
				return;
			}
			AnimationQueue.QueuedObject queuedObject = this._animationsList[0];
			this._animationsList.RemoveAt(0);
			if (queuedObject.OnAnimationStopped != null)
			{
				queuedObject.OnAnimationStopped();
			}
		}

		public void Clear()
		{
			if (this._animationsList.Count <= 0)
			{
				return;
			}
			this._animationsList[0].Animation.Stop(this._animationsList[0].AnimationClipName);
			this._animationsList[0].Animation.Rewind(this._animationsList[0].AnimationClipName);
			this._animationsList[0].Animation.Sample();
			this._animationsList.Clear();
		}

		private readonly List<AnimationQueue.QueuedObject> _animationsList = new List<AnimationQueue.QueuedObject>();

		private class QueuedObject
		{
			public Animation Animation;

			public string AnimationClipName;

			public float AnimationSpeedMultiplier;

			public Action OnAnimationStopped;

			public bool AnimationStarted;
		}
	}
}
