using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using Swordfish.Common.exceptions;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkColliderToLayer : BasePerk
	{
		public override void PerkInitialized()
		{
			CombatObject targetCombat = this.Effect.GetTargetCombat(this.Target);
			if (!targetCombat)
			{
				return;
			}
			TeamKind teamKind = targetCombat.Team;
			PerkColliderToLayer.ColliderTarget colliderMode = this.ColliderMode;
			if (colliderMode == PerkColliderToLayer.ColliderTarget.EnemyBlockingLayer || colliderMode == PerkColliderToLayer.ColliderTarget.EnemyLayer || colliderMode == PerkColliderToLayer.ColliderTarget.EnemyPhysicsProjectileLayer)
			{
				teamKind = teamKind.GetEnemyTeam();
			}
			bool flag = this.ColliderMode == PerkColliderToLayer.ColliderTarget.BlockingLayer || this.ColliderMode == PerkColliderToLayer.ColliderTarget.EnemyBlockingLayer;
			bool flag2 = this.ColliderMode == PerkColliderToLayer.ColliderTarget.PlayerPhysicsProjectileLayer || this.ColliderMode == PerkColliderToLayer.ColliderTarget.EnemyPhysicsProjectileLayer;
			LayerManager.Layer layer;
			if (teamKind != TeamKind.Red)
			{
				if (teamKind != TeamKind.Blue)
				{
					throw new InvalidArgumentException(string.Format("Invalid usage of perkColliderToLayer mode={0} team={1}", this.ColliderMode, teamKind));
				}
				if (flag)
				{
					layer = LayerManager.Layer.BluBlocker;
				}
				else if (flag2)
				{
					layer = LayerManager.Layer.PhysicsProjectileBlue;
				}
				else
				{
					layer = LayerManager.Layer.PlayerBlu;
				}
			}
			else if (flag)
			{
				layer = LayerManager.Layer.RedBlocker;
			}
			else if (flag2)
			{
				layer = LayerManager.Layer.PhysicsProjectileRed;
			}
			else
			{
				layer = LayerManager.Layer.PlayerRed;
			}
			this.ChangeLayer(layer);
		}

		private void ChangeLayer(LayerManager.Layer layer)
		{
			for (int i = 0; i < this.Colliders.Length; i++)
			{
				Collider collider = this.Colliders[i];
				collider.gameObject.layer = (int)layer;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkColliderToLayer));

		public BasePerk.PerkTarget Target;

		public PerkColliderToLayer.ColliderTarget ColliderMode;

		public Collider[] Colliders;

		public enum ColliderTarget
		{
			PlayerLayer,
			EnemyLayer,
			BlockingLayer,
			EnemyBlockingLayer,
			PlayerPhysicsProjectileLayer,
			EnemyPhysicsProjectileLayer
		}
	}
}
