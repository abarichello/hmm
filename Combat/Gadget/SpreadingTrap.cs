using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class SpreadingTrap : DropperGadget, DamageAreaCallback.IDamageAreaCallbackListener, TriggerEnterCallback.ITriggerEnterCallbackListener
	{
		private SpreadingTrapInfo MyInfo
		{
			get
			{
				return base.Info as SpreadingTrapInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._upgPrimaryModifiers = ModifierData.CreateData(this.MyInfo.PrimaryModifiers, this.MyInfo);
			this._upgSecondaryModiers = ModifierData.CreateData(this.MyInfo.SecondaryModifiers, this.MyInfo);
			this._upgSecondaryDropTime = new Upgradeable(this.MyInfo.SecondaryDropTimeUpgrade, this.MyInfo.SecondaryDropTime, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgPrimaryModifiers.SetLevel(upgradeName, level);
			this._upgSecondaryModiers.SetLevel(upgradeName, level);
			this._upgSecondaryDropTime.SetLevel(upgradeName, level);
		}

		protected override void DropTrap(DropperGadget.TrapDropper trapDropper, Vector3 position)
		{
			if (trapDropper.CombatObject == this.Combat)
			{
				this.DropPrimaryTrap(trapDropper, position);
			}
			else
			{
				this.DropSecondaryTrap(trapDropper, position);
			}
		}

		private void DropPrimaryTrap(DropperGadget.TrapDropper trapDropper, Vector3 position)
		{
			SpreadingTrapInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.PrimaryEffect);
			effectEvent.Modifiers = ModifierData.CopyData(this._upgPrimaryModifiers);
			effectEvent.Range = myInfo.PrimaryRadius;
			effectEvent.Direction = this.Combat.transform.forward;
			effectEvent.LifeTime = base._currentLifeTime;
			effectEvent.Origin = position;
			effectEvent.Target = position;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private void DropSecondaryTrap(DropperGadget.TrapDropper trapDropper, Vector3 position)
		{
			CombatObject combatObject = trapDropper.CombatObject;
			SpreadingTrapInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.SecondaryEffect);
			effectEvent.Modifiers = ModifierData.CopyData(this._upgSecondaryModiers);
			effectEvent.Range = myInfo.SecondaryRadius;
			effectEvent.Direction = combatObject.transform.forward;
			effectEvent.LifeTime = base._currentLifeTime;
			effectEvent.Origin = position;
			effectEvent.Target = position;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public void OnDamageAreaCallback(DamageAreaCallback evt)
		{
			if (evt.Effect.Gadget != this)
			{
				return;
			}
			for (int i = 0; i < evt.DamagedPlayers.Count; i++)
			{
				CombatObject combatObject = evt.DamagedPlayers[i];
				this.StartSecondaryDrop(combatObject);
			}
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
			if (evt.Gadget != this)
			{
				return;
			}
			CombatObject combat = evt.Combat;
			this.StartSecondaryDrop(combat);
		}

		protected virtual void StartSecondaryDrop(CombatObject combatObject)
		{
			if (this._trapDroppers.Find((DropperGadget.TrapDropper t) => t.CombatObject == combatObject) == null)
			{
				base.StartDrop(combatObject, this._upgSecondaryDropTime, this.MyInfo.SecondaryDropDelay);
			}
		}

		protected ModifierData[] _upgPrimaryModifiers;

		protected ModifierData[] _upgSecondaryModiers;

		protected Upgradeable _upgSecondaryDropTime;
	}
}
