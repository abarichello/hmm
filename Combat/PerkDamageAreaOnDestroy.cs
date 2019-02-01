using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageAreaOnDestroy : BaseDamageableAreaPerk
	{
		protected override void Awake()
		{
			base.Awake();
			this.Effect.OnPreEffectDestroyed += this.OnPreEffectDestroyed;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.Effect.OnPreEffectDestroyed -= this.OnPreEffectDestroyed;
		}

		private void OnPreEffectDestroyed()
		{
			this.GetHits(base.transform.position, ref this.HittingCombatObjects);
		}

		public override void OnDestroyEffect(DestroyEffect evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || (!this.DoDamageOnSceneryTriggerDestroy && evt.RemoveData.SrvWasScenery) || (!this.DoDamageOnLifetimeDestroy && !evt.RemoveData.SrvOtherCollider) || (!this.DoDamageOnOwnerDeath && !CombatRef.GetCombat(this.Effect.Owner).IsAlive()))
			{
				return;
			}
			base.OnDestroyEffect(evt);
			this._hitIdentifiableDeath = (evt.RemoveData.DestroyReason == BaseFX.EDestroyReason.HitIdentifiable);
			this.InternalDamageArea(evt.RemoveData.Origin, evt.RemoveData);
		}

		protected virtual void InternalDamageArea(Vector3 position, EffectRemoveEvent removeData)
		{
			if (this.ForceDamageSource)
			{
				CombatObject combat = CombatRef.GetCombat(removeData.TargetId);
				if (combat != null)
				{
					base.ApplyDamage(combat, combat, removeData.SrvWasBarrier);
					this.DamagedPlayers.Add(combat);
				}
			}
			for (int i = 0; i < this.HittingCombatObjects.Count; i++)
			{
				BarrierUtils.CombatHit combatHit = this.HittingCombatObjects[i];
				CombatObject combat2 = combatHit.Combat;
				if (!this.DamagedPlayers.Contains(combat2))
				{
					Vector3 normalized = (combat2.Transform.position - position).normalized;
					base.ApplyDamage(combat2, combat2, combatHit.Barrier, normalized, position);
					this.DamagedPlayers.Add(combat2);
				}
			}
			if (this._hitIdentifiableDeath && this.DamagedPlayers.Count == 0)
			{
				PerkDamageAreaOnDestroy.Log.ErrorFormat("transName:{0} position:{1} DamagedPlayerCount:{2} hitOld:{3} hitNew:{4}", new object[]
				{
					base.transform.name,
					position,
					this.DamagedPlayers.Count,
					this.HittingCombatObjects.Count,
					this.NewHittingCombatObjects
				});
				for (int j = 0; j < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; j++)
				{
					PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[j];
					if (playerData.Team != this.Effect.Gadget.Combat.Team)
					{
						Vector3 vector = playerData.CharacterInstance.transform.position - this.Effect.Owner.transform.position;
						PerkDamageAreaOnDestroy.Log.ErrorFormat("TargetName:{0}, targetPos:{1} effectPos:{2} sqrMag:{3}", new object[]
						{
							playerData.CharacterInstance.name,
							playerData.CharacterInstance.transform.position,
							base.transform.position,
							vector.sqrMagnitude
						});
					}
				}
			}
			if (this.IsDamageCallbackEnabled)
			{
				Mural.Post(new DamageAreaCallback(this.DamagedPlayers, position, this.Effect, this.TargetGadgetCallback), this.Effect.Gadget);
			}
			this.HittingCombatObjects.Clear();
			this.DamagedPlayers.Clear();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDamageAreaOnDestroy));

		public bool DoDamageOnSceneryTriggerDestroy = true;

		public bool DoDamageOnLifetimeDestroy = true;

		public bool DoDamageOnOwnerDeath = true;

		[Tooltip("Will force damage on source combat object even if it's not in range")]
		public bool ForceDamageSource;

		private bool _hitIdentifiableDeath;

		protected List<CombatObject> NewHittingCombatObjects = new List<CombatObject>(20);
	}
}
