using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class MortarEffect : GadgetBehaviour
	{
		public MortarEffectInfo MortarParentInto
		{
			get
			{
				return base.Info as MortarEffectInfo;
			}
		}

		private Vector3 MortarDummyPosition()
		{
			if (this._mortarDummyAux)
			{
				return this._mortarDummyAux.position;
			}
			this._mortarDummyAux = this.Combat.Dummy.GetDummy(this.MortarParentInto.MortarDummy, null);
			return this._mortarDummyAux.position;
		}

		protected override void Awake()
		{
			base.Awake();
			if (GameHubBehaviour.Hub)
			{
				this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (!base.Pressed)
			{
				this.CurrentCooldownTime = this.CurrentTime;
				return;
			}
			if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
			{
				return;
			}
			long num = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num;
			this._currentMortar = this.FireMortar();
			base.OnGadgetUsed(this._currentMortar);
		}

		private int FireMortar()
		{
			MortarEffectInfo mortarParentInto = this.MortarParentInto;
			EffectEvent effectEvent = base.GetEffectEvent(mortarParentInto.MortarEffect);
			Vector3 vector = this.MortarDummyPosition();
			float mortarRange = mortarParentInto.MortarRange;
			float mortarMoveSpeed = mortarParentInto.MortarMoveSpeed;
			float lifeTime = mortarParentInto.LifeTime;
			bool mortarUseMoveSpeed = mortarParentInto.MortarUseMoveSpeed;
			Vector3 a = base.Target;
			a.y = vector.y;
			a -= vector;
			Vector3 normalized = a.normalized;
			float num = a.magnitude;
			Vector3 target;
			if (num > mortarRange)
			{
				num = mortarRange;
				target = vector + normalized * mortarRange;
			}
			else
			{
				target = base.Target;
			}
			target.y = 0f;
			effectEvent.Origin = vector;
			effectEvent.Direction = normalized;
			effectEvent.Target = target;
			if (mortarUseMoveSpeed)
			{
				effectEvent.LifeTime = num / mortarMoveSpeed;
			}
			else
			{
				effectEvent.LifeTime = lifeTime;
			}
			effectEvent = this.SetAdditionalMortarData(effectEvent);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected virtual EffectEvent SetAdditionalMortarData(EffectEvent data)
		{
			return data;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MortarEffect));

		protected int _currentMortar;

		private Transform _mortarDummyAux;
	}
}
