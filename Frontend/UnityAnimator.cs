using System;
using HeavyMetalMachines.Presenting;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class UnityAnimator : IAnimator
	{
		public UnityAnimator(Animator animator)
		{
			this._animator = animator;
		}

		public bool IsPlaying
		{
			get
			{
				return this._animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f && this._animator.IsInTransition(0);
			}
		}

		public IObservable<Unit> Play(string propertyName)
		{
			return Observable.Do<Unit>(Observable.Delay<Unit>(Observable.DelayFrame<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._animator.SetBool(propertyName, true);
			}), 1, 0), TimeSpan.FromSeconds((double)this._animator.GetCurrentAnimatorStateInfo(0).length), Scheduler.MainThreadIgnoreTimeScale), delegate(Unit _)
			{
				this._animator.SetBool(propertyName, false);
			});
		}

		public void SetBoolean(string parameter, bool value)
		{
			this._animator.SetBool(parameter, value);
		}

		public void SetInteger(string parameter, int value)
		{
			this._animator.SetInteger(parameter, value);
		}

		public void Trigger(string parameter)
		{
			this._animator.SetTrigger(parameter);
		}

		[SerializeField]
		private Animator _animator;
	}
}
