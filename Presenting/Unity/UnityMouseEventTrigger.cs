using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityMouseEventTrigger : IMouseEventTrigger
	{
		public IObservable<Unit> OnMouseEnter
		{
			get
			{
				return this.CreateEventTriggerObservable(0);
			}
		}

		private IObservable<Unit> CreateEventTriggerObservable(EventTriggerType eventTriggerType)
		{
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				UnityAction<BaseEventData> unityAction = delegate(BaseEventData _)
				{
					observer.OnNext(Unit.Default);
				};
				EventTrigger.TriggerEvent triggerEvent = new EventTrigger.TriggerEvent();
				triggerEvent.AddListener(unityAction);
				EventTrigger.Entry entry = new EventTrigger.Entry
				{
					eventID = eventTriggerType,
					callback = triggerEvent
				};
				this._eventTrigger.triggers.Add(entry);
				return Disposable.Create(delegate()
				{
					<CreateEventTriggerObservable>c__AnonStorey._eventTrigger.triggers.Remove(entry);
				});
			});
		}

		[SerializeField]
		private EventTrigger _eventTrigger;
	}
}
