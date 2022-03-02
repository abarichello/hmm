using System;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityAnimation : IAnimation
	{
		public UnityAnimation(Animation animation, string stateName)
		{
			this._animation = animation;
			this._stateName = stateName;
		}

		public IObservable<Unit> Play()
		{
			return this.PlayAnimation();
		}

		public AnimationClip Clip
		{
			get
			{
				return this._animation.GetClip(this._stateName);
			}
		}

		public Animation AnimationComponent
		{
			get
			{
				return this._animation;
			}
		}

		private IObservable<Unit> PlayAnimation()
		{
			AnimationClip clip = this.Clip;
			float length = clip.length;
			return Observable.DoOnCancel<Unit>(Observable.Delay<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				AnimationClip clip2 = this.Clip;
				if (!clip2.legacy)
				{
					throw new InvalidOperationException(string.Format("Animation is not legacy: {0}", clip2));
				}
				this._animation.Play(this._stateName);
			}), TimeSpan.FromSeconds((double)length), Scheduler.MainThreadIgnoreTimeScale), delegate()
			{
				if (this._animation == null)
				{
					return;
				}
				this.ResetToLastFrame();
			});
		}

		public void ResetToFirstFrame()
		{
			this._animation.Play(this._stateName);
			AnimationState animationState = this._animation[this._stateName];
			animationState.time = 0f;
			this._animation.Sample();
			this._animation.Stop(this._stateName);
		}

		public void ResetToLastFrame()
		{
			if (this._animation == null)
			{
				return;
			}
			this._animation.Play(this._stateName);
			AnimationState animationState = this._animation[this._stateName];
			animationState.time = animationState.length;
			this._animation.Sample();
			this._animation.Stop(this._stateName);
		}

		public bool IsPlaying
		{
			get
			{
				return this._animation.isPlaying;
			}
		}

		[SerializeField]
		private Animation _animation;

		[SerializeField]
		private string _stateName;
	}
}
