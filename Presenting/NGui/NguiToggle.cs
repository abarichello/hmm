using System;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class NguiToggle : IToggle
	{
		public bool IsInteractable
		{
			get
			{
				return this._toggle.GetComponent<BoxCollider>().enabled;
			}
			set
			{
				this._toggle.TryToUpdateSpriteAlpha();
				this._toggle.GetComponent<BoxCollider>().enabled = value;
				UIButton[] components = this._toggle.GetComponents<UIButton>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].SetState((!value) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, true);
				}
			}
		}

		public bool IsOn
		{
			get
			{
				return this._toggle.value;
			}
			set
			{
				this._toggle.Set(value, false);
			}
		}

		public IObservable<Unit> OnToggleOn()
		{
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				EventDelegate eventHandler = new EventDelegate(delegate()
				{
					this.CallOnNextWhenToggleOn(observer);
				});
				this._toggle.onChange.Add(eventHandler);
				return Disposable.Create(delegate()
				{
					this._toggle.onChange.Remove(eventHandler);
				});
			});
		}

		public IObservable<Unit> OnToggleOff()
		{
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				EventDelegate eventHandler = new EventDelegate(delegate()
				{
					this.CallOnNextWhenToggleOff(observer);
				});
				this._toggle.onChange.Add(eventHandler);
				return Disposable.Create(delegate()
				{
					this._toggle.onChange.Remove(eventHandler);
				});
			});
		}

		private void CallOnNextWhenToggleOn(IObserver<Unit> observer)
		{
			if (this._toggle.value)
			{
				observer.OnNext(Unit.Default);
			}
		}

		private void CallOnNextWhenToggleOff(IObserver<Unit> observer)
		{
			if (!this._toggle.value)
			{
				observer.OnNext(Unit.Default);
			}
		}

		[SerializeField]
		private UIToggle _toggle;
	}
}
