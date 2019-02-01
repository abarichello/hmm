using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class DropBomb : BasicCannon, DamageAreaCallback.IDamageAreaCallbackListener
	{
		public DropBombInfo MyInfo
		{
			get
			{
				return base.Info as DropBombInfo;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			DropBombInfo myInfo = this.MyInfo;
			this._upgWardModifiers = ModifierData.CreateData(myInfo.WardModifiers, myInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgWardModifiers.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			DropBombInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.Effect);
			Transform transform = this.Combat.transform;
			float range = this.GetRange();
			Vector3 direction = transform.forward * -1f;
			direction.y = 0f;
			direction.Normalize();
			effectEvent.Origin = this.DummyPosition();
			effectEvent.Target = effectEvent.Origin;
			effectEvent.Range = range;
			effectEvent.Direction = direction;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public void OnDamageAreaCallback(DamageAreaCallback evt)
		{
			CombatObject combatObject = evt.DamagedPlayers.Find((CombatObject i) => i.IsTurret);
			if (combatObject != null)
			{
				this.AttachBomb(combatObject);
				return;
			}
			combatObject = evt.DamagedPlayers.Find((CombatObject i) => i.IsBuilding);
			if (combatObject != null)
			{
				this.AttachBomb(combatObject);
				return;
			}
			combatObject = evt.DamagedPlayers.Find((CombatObject i) => i.IsPlayer);
			if (combatObject != null)
			{
				this.AttachBomb(combatObject);
				return;
			}
			combatObject = evt.DamagedPlayers.Find((CombatObject i) => i.IsCreep);
			if (combatObject != null)
			{
				this.AttachBomb(combatObject);
				return;
			}
			combatObject = evt.DamagedPlayers.Find((CombatObject i) => i.IsWard);
			if (combatObject != null)
			{
				this.AttachBomb(combatObject);
				return;
			}
			this.StickToGround(evt.Effect.Data.Target);
		}

		private void AttachBomb(CombatObject combatObject)
		{
			DropBombInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.AttachEffect);
			Transform transform = combatObject.transform;
			float explosionRadius = myInfo.ExplosionRadius;
			effectEvent.Origin = transform.position;
			effectEvent.Target = transform.position;
			effectEvent.TargetId = combatObject.Id.ObjId;
			effectEvent.Range = explosionRadius;
			effectEvent.Modifiers = this._damage;
			effectEvent.ExtraModifiers = this._upgWardModifiers;
			effectEvent.LifeTime = base.LifeTime;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private void StickToGround(Vector3 position)
		{
			DropBombInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.GroundEffect);
			float explosionRadius = myInfo.ExplosionRadius;
			effectEvent.Origin = position;
			effectEvent.Target = position;
			effectEvent.Range = explosionRadius;
			effectEvent.Modifiers = this._damage;
			effectEvent.ExtraModifiers = this._upgWardModifiers;
			effectEvent.LifeTime = base.LifeTime;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(DropBomb));

		private ModifierData[] _upgWardModifiers;
	}
}
