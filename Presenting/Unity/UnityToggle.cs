using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityToggle : IToggle
	{
		public UnityToggle(Toggle toggle)
		{
			this._toggle = toggle;
		}

		public Toggle Toggle
		{
			get
			{
				return this._toggle;
			}
		}

		public bool IsInteractable
		{
			get
			{
				return this._toggle.interactable;
			}
			set
			{
				this._toggle.interactable = value;
			}
		}

		public bool IsOn
		{
			get
			{
				return this._toggle.isOn;
			}
			set
			{
				this._toggle.isOn = value;
			}
		}

		public IObservable<Unit> OnToggleOn()
		{
			return Observable.Select<bool, Unit>(Observable.Where<bool>(Observable.Skip<bool>(UnityUIComponentExtensions.OnValueChangedAsObservable(this._toggle), 1), (bool value) => value), (bool value) => Unit.Default);
		}

		public IObservable<Unit> OnToggleOff()
		{
			return Observable.Select<bool, Unit>(Observable.Where<bool>(Observable.Skip<bool>(UnityUIComponentExtensions.OnValueChangedAsObservable(this._toggle), 1), (bool value) => !value), (bool value) => Unit.Default);
		}

		[SerializeField]
		private Toggle _toggle;
	}
}
