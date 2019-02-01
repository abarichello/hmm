using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class AreaAttackBoost : BasicCannon, DamageTakenCallback.IDamageTakenCallbackListener
	{
		public AreaAttackBoostInfo MyInfo
		{
			get
			{
				return base.Info as AreaAttackBoostInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.Cap = this.MyInfo.HitEffect.EffectPreCacheCount;
			this.m_cTimeoutQueue = new Queue<float>(this.Cap);
		}

		protected override int FireGadget()
		{
			AreaAttackBoostInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.Effect);
			Transform transform = this.Combat.transform;
			Vector3 position = transform.position;
			Vector3 forward = transform.forward;
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = position;
			effectEvent.Direction = forward;
			effectEvent.LifeTime = base.LifeTime;
			this._currentEffetct = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			return this._currentEffetct;
		}

		private new void Update()
		{
			float time = Time.time;
			while (this.m_cTimeoutQueue.Count > 0)
			{
				float num = this.m_cTimeoutQueue.Peek();
				if (time <= num + 1.75f)
				{
					break;
				}
				this.m_cTimeoutQueue.Dequeue();
			}
		}

		public void OnDamageTakenCallback(DamageTakenCallback evt)
		{
			if (!evt.TakerCombatObject)
			{
				return;
			}
			if (evt.ListenerEffectId != this._currentEffetct)
			{
				return;
			}
			if (!evt.TakerCombatObject.IsAlive())
			{
				return;
			}
			AreaAttackBoostInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.HitEffect);
			effectEvent.Origin = evt.TakerCombatObject.transform.position;
			effectEvent.TargetId = evt.TakerCombatObject.Id.ObjId;
			int eventId = -1;
			if (this.m_cTimeoutQueue.Count < this.Cap)
			{
				eventId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
				this.m_cTimeoutQueue.Enqueue(Time.time);
			}
			evt.TakerCombatObject.Controller.AddModifiers(this._damage, this.Combat, eventId, false);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(AreaAttackBoost));

		private int _currentEffetct;

		private int Cap = 30;

		private const float fTimeOut = 1.75f;

		private Queue<float> m_cTimeoutQueue;
	}
}
