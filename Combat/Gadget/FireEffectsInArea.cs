using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class FireEffectsInArea : BasicCannon
	{
		public FireEffectsInAreaInfo MyInfo
		{
			get
			{
				return base.Info as FireEffectsInAreaInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.FireEffectRadius = new Upgradeable(this.MyInfo.FireEffectRadiusUpgrade, this.MyInfo.FireEffectRadius, this.MyInfo.UpgradesValues);
			this.AreaModifier = ModifierData.CreateData(this.MyInfo.AreaModifier, base.CannonInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.FireEffectRadius.SetLevel(upgradeName, level);
			this.AreaModifier.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			int result = -1;
			if (!string.IsNullOrEmpty(this.MyInfo.AreaEffect.Effect))
			{
				EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.AreaEffect);
				effectEvent.Origin = new Vector3(this.Combat.Transform.position.x, 0f, this.Combat.Transform.position.z);
				effectEvent.Range = this.GetRange();
				effectEvent.MoveSpeed = this._moveSpeed.Get();
				effectEvent.LifeTime = ((base.LifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.LifeTime);
				this.SetTargetAndDirection(this.Combat, this.MyInfo.Direction, effectEvent);
				effectEvent.Modifiers = this.AreaModifier;
				result = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			}
			if (this.FireEffectRadius == 0f)
			{
				result = this.FireThisCombat(this.Combat);
			}
			else
			{
				int num = 0;
				this.GetHittingCombatsInArea(this.Combat.transform.position, this.FireEffectRadius, 1077054464, this.MyInfo.AreaHitMask, ref this.m_cpoCombatObjects);
				for (int i = 0; i < this.m_cpoCombatObjects.Count; i++)
				{
					CombatObject combatObject = this.m_cpoCombatObjects[i];
					if (!this.MyInfo.OnlyMyTeam || this.Combat.Team == combatObject.Team)
					{
						if (this.Combat.Team != combatObject.Team)
						{
							num++;
						}
						result = this.FireThisCombat(combatObject);
					}
				}
				if (this.OnDoubleDamage != null && num > 1)
				{
					this.OnDoubleDamage();
				}
			}
			return result;
		}

		private int FireThisCombat(CombatObject combat)
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Origin = new Vector3(combat.Transform.position.x, 0f, combat.Transform.position.z);
			effectEvent.Range = this.GetRange();
			effectEvent.TargetId = combat.Id.ObjId;
			effectEvent.LifeTime = ((base.LifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.LifeTime);
			this.SetTargetAndDirection(combat, this.MyInfo.Direction, effectEvent);
			effectEvent.Modifiers = this._damage;
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(num);
			if (this.FireNormalAndExtraEffectsTogether.BoolGet())
			{
				EffectEvent effectEvent2 = base.GetEffectEvent(this.MyInfo.ExtraEffect);
				effectEvent2.MoveSpeed = this._moveSpeed.Get();
				effectEvent2.Origin = new Vector3(combat.Transform.position.x, 0f, combat.Transform.position.z);
				effectEvent2.Range = this.GetRange();
				effectEvent2.TargetId = combat.Id.ObjId;
				effectEvent2.LifeTime = ((this.ExtraLifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : this.ExtraLifeTime);
				this.SetTargetAndDirection(combat, this.MyInfo.Direction, effectEvent2);
				effectEvent2.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
				base.ExistingFiredEffectsAdd(GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent2));
			}
			return num;
		}

		protected override int FireExtraGadget()
		{
			return -1;
		}

		protected override int FireExtraGadgetOnDeath(DestroyEffectMessage destroyEvt)
		{
			if (base.Info.FireExtraOnEffectDeathOnlyIfTargetIdIsValid && destroyEvt.RemoveData.TargetId == -1)
			{
				return -1;
			}
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ExtraEffect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Origin = destroyEvt.RemoveData.Origin;
			effectEvent.Range = this.GetRange();
			effectEvent.TargetId = this.TargetId;
			effectEvent.LifeTime = ((this.ExtraLifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : this.ExtraLifeTime);
			this.SetTargetAndDirection(this.Combat, this.MyInfo.Direction, effectEvent);
			effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(num);
			return num;
		}

		private void SetTargetAndDirection(CombatObject combat, BasicCannonInfo.DirectionEnum direction, EffectEvent pData)
		{
			if (direction != BasicCannonInfo.DirectionEnum.Target)
			{
				if (direction == BasicCannonInfo.DirectionEnum.Forward)
				{
					pData.Direction = combat.Movement.LastVelocity;
					pData.Direction.y = 0f;
					pData.Direction.Normalize();
					pData.Target = pData.Origin + pData.Direction * pData.Range;
				}
			}
			else
			{
				pData.Target = base.Target;
				pData.Direction = base.CalcDirection(pData.Origin, pData.Target);
			}
			pData.Target = base.GetValidPosition(pData.Origin, pData.Target);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(FireEffectsInArea));

		protected ModifierData[] AreaModifier;

		protected Upgradeable FireEffectRadius;

		public Action OnDoubleDamage;

		private List<CombatObject> m_cpoCombatObjects = new List<CombatObject>(20);
	}
}
