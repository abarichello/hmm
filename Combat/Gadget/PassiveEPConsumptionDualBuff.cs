using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveEPConsumptionDualBuff : GadgetBehaviour
	{
		public PassiveEPConsumptionDualBuffInfo MyInfo
		{
			get
			{
				return base.Info as PassiveEPConsumptionDualBuffInfo;
			}
		}

		public override void SetInfo(GadgetInfo gadget)
		{
			base.SetInfo(gadget);
			PassiveEPConsumptionDualBuffInfo myInfo = this.MyInfo;
			this._primaryBuff = ModifierData.CreateData(myInfo.PrimaryBuff, myInfo);
			this._secondaryBuff = ModifierData.CreateData(myInfo.SecondaryBuff, myInfo);
			this.Combat.Controller.ListenToEP += this.OnEPSpent;
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._primaryBuff.SetLevel(upgradeName, level);
			this._secondaryBuff.SetLevel(upgradeName, level);
		}

		private void OnEPSpent(CombatObject combat, float amount)
		{
			if (!base.Activated)
			{
				return;
			}
			PassiveEPConsumptionDualBuffInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = new EffectEvent();
			effectEvent.CopyInfo(myInfo.PrimaryEffect);
			effectEvent.MoveSpeed = 0f;
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.Combat.transform.position;
			effectEvent.Target = Vector3.zero;
			effectEvent.Direction = this.Combat.transform.forward;
			effectEvent.Modifiers = this._primaryBuff;
			effectEvent.SourceGadget = this;
			effectEvent.SourceCombat = this.Combat;
			effectEvent.SourceSlot = base.Slot;
			effectEvent.SourceId = this.Parent.ObjId;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			EffectEvent effectEvent2 = new EffectEvent();
			effectEvent2.CopyInfo(myInfo.SecondaryEffect);
			effectEvent2.MoveSpeed = 0f;
			effectEvent2.Range = this.GetRange();
			effectEvent2.Origin = this.Combat.transform.position;
			effectEvent2.Target = Vector3.zero;
			effectEvent2.Direction = this.Combat.transform.forward;
			effectEvent2.Modifiers = ((!myInfo.DontMultiplyConvoluted) ? ModifierData.CreateConvoluted(this._secondaryBuff, (float)((int)amount)) : this._secondaryBuff);
			effectEvent2.SourceGadget = this;
			effectEvent2.SourceCombat = this.Combat;
			effectEvent2.SourceSlot = base.Slot;
			effectEvent2.SourceId = this.Parent.ObjId;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent2);
		}

		private ModifierData[] _primaryBuff;

		private ModifierData[] _secondaryBuff;
	}
}
