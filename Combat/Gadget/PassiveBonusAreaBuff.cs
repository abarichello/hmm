using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveBonusAreaBuff : GadgetBehaviour
	{
		public PassiveBonusAreaBuffInfo MyInfo
		{
			get
			{
				return base.Info as PassiveBonusAreaBuffInfo;
			}
		}

		public override void SetInfo(GadgetInfo gadget)
		{
			base.SetInfo(gadget);
			PassiveBonusAreaBuffInfo myInfo = this.MyInfo;
			this._buffs = ModifierData.CreateData(myInfo.Buff, myInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._buffs.SetLevel(upgradeName, level);
			this.DeactivateBuff();
			this.ActivateBuff();
		}

		public override void Activate()
		{
			base.Activate();
			this.ActivateBuff();
		}

		private void ActivateBuff()
		{
			if (!base.Activated)
			{
				return;
			}
			PassiveBonusAreaBuffInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.Effect);
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.Combat.transform.position;
			effectEvent.Target = Vector3.zero;
			effectEvent.Direction = this.Combat.transform.forward;
			effectEvent.Modifiers = this._buffs;
			this._currentPassiveBonusArea = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			this.DeactivateBuff();
		}

		private void DeactivateBuff()
		{
			if (this._currentPassiveBonusArea == -1)
			{
				return;
			}
			EffectRemoveEvent content = new EffectRemoveEvent
			{
				TargetEventId = this._currentPassiveBonusArea,
				TargetId = -1
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
			this._currentPassiveBonusArea = -1;
		}

		public override void OnObjectSpawned(SpawnEvent evt)
		{
			base.OnObjectSpawned(evt);
			this.ActivateBuff();
		}

		private ModifierData[] _buffs;

		private int _currentPassiveBonusArea;
	}
}
