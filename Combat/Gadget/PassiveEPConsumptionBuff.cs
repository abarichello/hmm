using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveEPConsumptionBuff : GadgetBehaviour
	{
		private PassiveEPConsumptionBuffInfo MyInfo
		{
			get
			{
				return base.Info as PassiveEPConsumptionBuffInfo;
			}
		}

		public override void SetInfo(GadgetInfo gadget)
		{
			base.SetInfo(gadget);
			PassiveEPConsumptionBuffInfo myInfo = this.MyInfo;
			this._buffs = ModifierData.CreateData(myInfo.Buff, myInfo);
			this.Combat.Controller.ListenToEP += this.OnEPSpent;
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			for (int i = 0; i < this._buffs.Length; i++)
			{
				this._buffs[i].SetLevel(upgradeName, level);
			}
		}

		private void OnEPSpent(CombatObject combat, float amount)
		{
			if (!base.Activated || amount <= 0f)
			{
				return;
			}
			PassiveEPConsumptionBuffInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = new EffectEvent();
			effectEvent.CopyInfo(myInfo.Effect);
			effectEvent.MoveSpeed = 0f;
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.Combat.transform.position;
			effectEvent.Target = Vector3.zero;
			effectEvent.Direction = this.Combat.transform.forward;
			effectEvent.Modifiers = ((!myInfo.DontMultiplyConvoluted) ? ModifierData.CreateConvoluted(this._buffs, (float)((int)amount)) : this._buffs);
			effectEvent.SourceGadget = this;
			effectEvent.SourceCombat = this.Combat;
			effectEvent.SourceSlot = base.Slot;
			effectEvent.SourceId = this.Parent.ObjId;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private ModifierData[] _buffs;
	}
}
