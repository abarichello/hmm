using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class CallbackOnTriggerEnter : BasicCannon, TriggerEnterCallback.ITriggerEnterCallbackListener
	{
		private CallbackOnTriggerEnterInfo MyInfo
		{
			get
			{
				return base.Info as CallbackOnTriggerEnterInfo;
			}
		}

		protected void Start()
		{
			MonoBehaviour[] components = base.GetComponents<MonoBehaviour>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] != this && components[i] is TriggerEnterCallback.ITriggerEnterCallbackListener)
				{
					this.TargetGadget = (components[i] as TriggerEnterCallback.ITriggerEnterCallbackListener);
				}
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			this.TrailModifier = ModifierData.CreateData(this.MyInfo.TrailModifier, this.MyInfo);
			this.TrailDropper = new Trail(GameHubBehaviour.Hub, new Func<FXInfo, EffectEvent>(base.GetEffectEvent), this.MyInfo.TrailEffect, this.MyInfo.TrailMustFollowCar);
			this.TrailDropper.SetLevel(this.TrailModifier, this.GetRange(), this._moveSpeed.Get(), (float)this.MyInfo.TrailColliderRadius, this.MyInfo.TrailPiecesLifeTime, this.MyInfo.TrailPiecesDropIntervalMillis);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.TrailModifier.SetLevel(upgradeName, level);
			this.TrailDropper.SetLevel(this.TrailModifier, this.GetRange(), this._moveSpeed.Get(), (float)this.MyInfo.TrailColliderRadius, this.MyInfo.TrailPiecesLifeTime, this.MyInfo.TrailPiecesDropIntervalMillis);
		}

		protected override int FireGadget()
		{
			this.TrailDropper.RunGadgetFixedUpdate();
			if (this._currentEffectId > 0)
			{
				return this._currentEffectId;
			}
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.DummyPosition();
			effectEvent.Target = base.Target;
			effectEvent.TargetId = this.TargetId;
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			this._currentEffectId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this.TrailDropper.FireCannon(effectEvent.Origin, effectEvent.Direction, this._currentEffectId, this.Combat);
			return this._currentEffectId;
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			this._currentEffectId = -1;
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			if (evt.RemoveData.TargetEventId != this._currentEffectId)
			{
				return;
			}
			this.TrailDropper.OnDestroyEffect(evt);
			this._currentEffectId = -1;
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
			evt.Gadget = (GadgetBehaviour)this.TargetGadget;
			this.TargetGadget.OnTriggerEnterCallback(evt);
		}

		protected Trail TrailDropper;

		protected ModifierData[] TrailModifier;

		public TriggerEnterCallback.ITriggerEnterCallbackListener TargetGadget;

		private int _currentEffectId;
	}
}
