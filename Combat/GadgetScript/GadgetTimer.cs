using System;
using System.Collections.Generic;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public class GadgetTimer : MonoBehaviour
	{
		public void ScheduleEvent(IEventContext eventContext)
		{
			List<IEventContext> list;
			if (!this._scheduledEvents.TryGetValue(eventContext.CreationTime, out list))
			{
				list = new List<IEventContext>();
				this._scheduledEvents.Add(eventContext.CreationTime, list);
			}
			list.Add(eventContext);
			this._events.Add(eventContext.Id, eventContext);
			this._nextSchedule = this._scheduledEvents.Keys[0];
		}

		public void CancelScheduledEvent(int id)
		{
			IEventContext eventContext;
			if (this._events.TryGetValue(id, out eventContext))
			{
				this._events.Remove(id);
				List<IEventContext> list = this._scheduledEvents[eventContext.CreationTime];
				if (list.Count == 1)
				{
					this._scheduledEvents.Remove(eventContext.CreationTime);
				}
				else
				{
					list.Remove(eventContext);
				}
				GadgetEvent.Free((GadgetEvent)eventContext);
				if (this._scheduledEvents.Count == 0)
				{
					this._nextSchedule = int.MaxValue;
				}
				else
				{
					this._nextSchedule = this._scheduledEvents.Keys[0];
				}
			}
		}

		public void CancelAllEvents()
		{
			foreach (IEventContext eventContext in this._events.Values)
			{
				GadgetEvent.Free((GadgetEvent)eventContext);
			}
			this._nextSchedule = int.MaxValue;
			this._scheduledEvents.Clear();
			this._events.Clear();
		}

		public void SetGadgetContext(IHMMGadgetContext gadget)
		{
			this._gadget = gadget;
		}

		private void Update()
		{
			while (this._scheduledEvents.Count > 0 && this._gadget.CurrentTime >= this._nextSchedule)
			{
				List<IEventContext> list = this._scheduledEvents[this._nextSchedule];
				this._scheduledEvents.Remove(this._nextSchedule);
				for (int i = 0; i < list.Count; i++)
				{
					this._events.Remove(list[i].Id);
				}
				for (int j = 0; j < list.Count; j++)
				{
					this._gadget.TriggerEvent(list[j]);
				}
				if (this._scheduledEvents.Count > 0)
				{
					this._nextSchedule = this._scheduledEvents.Keys[0];
				}
				else
				{
					this._nextSchedule = int.MaxValue;
				}
			}
		}

		private IHMMGadgetContext _gadget;

		private SortedList<int, List<IEventContext>> _scheduledEvents = new SortedList<int, List<IEventContext>>();

		private Dictionary<int, IEventContext> _events = new Dictionary<int, IEventContext>();

		private int _nextSchedule;
	}
}
