using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class PassiveExtraEffect : GadgetBehaviour
	{
		public PassiveExtraEffectInfo MyInfo
		{
			get
			{
				return base.Info as PassiveExtraEffectInfo;
			}
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._transform = this.Combat.transform;
		}

		public override void SetInfo(GadgetInfo gadget)
		{
			base.SetInfo(gadget);
			PassiveExtraEffectInfo myInfo = this.MyInfo;
			this._damage = ModifierData.CreateData(myInfo.Damage, myInfo);
		}

		public override void Activate()
		{
			base.Activate();
			this.Combat.GetGadget(this.MyInfo.TargetGadget).ServerListenToGadgetUse += this.OnEffectFired;
			PassiveExtraEffect.Log.DebugFormat("Activated, listener installed on={0}", new object[]
			{
				this.Combat.GetGadget(this.MyInfo.TargetGadget).Info.Name
			});
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			for (int i = 0; i < this._damage.Length; i++)
			{
				this._damage[i].SetLevel(upgradeName, level);
			}
		}

		private void OnEffectFired()
		{
			PassiveExtraEffectInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.Effect);
			effectEvent.Origin = (effectEvent.Target = this._transform.position);
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target + this._transform.forward);
			effectEvent.Modifiers = this._damage;
			effectEvent.TargetId = this.Combat.Id.ObjId;
			effectEvent.LifeTime = base.LifeTime;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PassiveExtraEffect));

		private ModifierData[] _damage;

		private Transform _transform;
	}
}
