using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class CaterpillarPassiveBasicAttackVariation : GadgetBehaviour
	{
		private CaterpillarPassiveBasicAttackVariationInfo MyInfo
		{
			get
			{
				return base.Info as CaterpillarPassiveBasicAttackVariationInfo;
			}
		}

		private BasicAttack BasicAttack
		{
			get
			{
				if (this._basicAttack)
				{
					return this._basicAttack;
				}
				this._basicAttack = (this.Combat.CustomGadget0 as BasicAttack);
				return this._basicAttack;
			}
			set
			{
				this._basicAttack = value;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._upgModifiers = ModifierData.CreateData(this.MyInfo.Modifiers, this.MyInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgModifiers.SetLevel(upgradeName, level);
		}

		protected override void Awake()
		{
			base.Awake();
			this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		public override void Activate()
		{
			base.Activate();
			this.BasicAttack.ListenToBasicAttackFired += this.OnBasicAttackFired;
			this._myTransform = this.Combat.transform;
		}

		private void OnBasicAttackFired(ref bool cancelbasicattack)
		{
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
			{
				return;
			}
			long num = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num;
			this.FireGadget();
			cancelbasicattack = true;
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
			this.CurrentCooldownTime = this.CurrentTime;
		}

		protected override int FireGadget()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
			effectEvent.SourceGadget = this.BasicAttack;
			effectEvent.SourceSlot = this.BasicAttack.Slot;
			effectEvent.Modifiers = this._upgModifiers;
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this._myTransform.position;
			effectEvent.Target = this.BasicAttack.Target;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			effectEvent.MoveSpeed = this.MyInfo.MoveSpeed;
			effectEvent = this.BasicAttack.IncreaseRangeForPlayers(effectEvent);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private ModifierData[] _upgModifiers;

		private BasicAttack _basicAttack;

		private Transform _myTransform;
	}
}
