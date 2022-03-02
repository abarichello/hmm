using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnitySelectable : ISelectable
	{
		public IObservable<Unit> OnSelect()
		{
			return Observable.AsUnitObservable<BaseEventData>(ObservableTriggerExtensions.OnSelectAsObservable(this._selectable));
		}

		public IObservable<Unit> OnDeselect()
		{
			return Observable.AsUnitObservable<BaseEventData>(ObservableTriggerExtensions.OnDeselectAsObservable(this._selectable));
		}

		public bool Interactable
		{
			get
			{
				return this._selectable.IsInteractable();
			}
			set
			{
				this._selectable.interactable = value;
			}
		}

		[SerializeField]
		private Selectable _selectable;
	}
}
