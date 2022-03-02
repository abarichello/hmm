using System;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class NGuiSlider : ISlider
	{
		public float FillPercent
		{
			get
			{
				return this._slider.value;
			}
			set
			{
				this._slider.value = value;
			}
		}

		public IObservable<float> OnValueChanged()
		{
			return Observable.Create<float>(delegate(IObserver<float> observer)
			{
				EventDelegate eventHandler = new EventDelegate(delegate()
				{
					observer.OnNext(this._slider.value);
				});
				this._slider.onChange.Add(eventHandler);
				return Disposable.Create(delegate()
				{
					this._slider.onChange.Remove(eventHandler);
				});
			});
		}

		public IObservable<Unit> OnPointerUp()
		{
			throw new NotImplementedException();
		}

		public IObservable<Unit> OnPointerDown()
		{
			throw new NotImplementedException();
		}

		[SerializeField]
		private UISlider _slider;
	}
}
