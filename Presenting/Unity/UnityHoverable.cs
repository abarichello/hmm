using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityHoverable : IHoverable
	{
		public UnityHoverable(Selectable selectable)
		{
			this._selectable = selectable;
		}

		public IObservable<Unit> OnHoverIn()
		{
			return Observable.AsUnitObservable<PointerEventData>(ObservableTriggerExtensions.OnPointerEnterAsObservable(this._selectable));
		}

		public IObservable<Unit> OnHoverOut()
		{
			return Observable.AsUnitObservable<PointerEventData>(ObservableTriggerExtensions.OnPointerExitAsObservable(this._selectable));
		}

		public bool Interactable
		{
			get
			{
				return this._selectable.targetGraphic.raycastTarget;
			}
			set
			{
				this._selectable.targetGraphic.raycastTarget = value;
			}
		}

		[SerializeField]
		private Selectable _selectable;
	}
}
