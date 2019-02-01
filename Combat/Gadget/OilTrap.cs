using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class OilTrap : SpreadingTrap
	{
		private OilTrapInfo MyInfo
		{
			get
			{
				return base.Info as OilTrapInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._upgCollisionModifiers = ModifierData.CreateData(this.MyInfo.CollisionModifiers, this.MyInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgCollisionModifiers.SetLevel(upgradeName, level);
		}

		protected override void StartSecondaryDrop(CombatObject combatObject)
		{
			base.StartSecondaryDrop(combatObject);
			if (combatObject.Effects.Find((BaseFX e) => e.Data.EffectInfo.Effect == this.MyInfo.CollisionEffect.Effect) != null)
			{
				return;
			}
			OilTrapInfo myInfo = this.MyInfo;
			Transform transform = combatObject.transform;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.CollisionEffect);
			effectEvent.Modifiers = ModifierData.CopyData(this._upgCollisionModifiers);
			effectEvent.LifeTime = myInfo.CollisionLifeTime;
			effectEvent.Direction = this.Combat.transform.forward;
			effectEvent.Origin = transform.position;
			effectEvent.Target = transform.position;
			effectEvent.TargetId = combatObject.Id.ObjId;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(OilTrap));

		protected ModifierData[] _upgCollisionModifiers;
	}
}
