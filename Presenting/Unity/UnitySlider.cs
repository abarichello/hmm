using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnitySlider : ISlider
	{
		public UnitySlider(Slider slider)
		{
			this._slider = slider;
		}

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
			return Observable.Skip<float>(UnityUIComponentExtensions.OnValueChangedAsObservable(this._slider), 1);
		}

		public IObservable<Unit> OnPointerUp()
		{
			return Observable.AsUnitObservable<PointerEventData>(Observable.Where<PointerEventData>(ObservableTriggerExtensions.OnPointerUpAsObservable(this._slider), (PointerEventData pointerEventData) => pointerEventData.button == 0));
		}

		public IObservable<Unit> OnPointerDown()
		{
			return Observable.AsUnitObservable<PointerEventData>(Observable.Where<PointerEventData>(ObservableTriggerExtensions.OnPointerDownAsObservable(this._slider), (PointerEventData pointerEventData) => pointerEventData.button == 0));
		}

		[SerializeField]
		private Slider _slider;
	}
}
