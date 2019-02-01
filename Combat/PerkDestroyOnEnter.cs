using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnEnter : BasePerk, IPerkWithCollision
	{
		public override void PerkInitialized()
		{
			base.PerkInitialized();
			this._perkInitializedTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		public int Priority()
		{
			return 0;
		}

		public void OnHit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
			if (this.Effect.IsDead)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (GameHubBehaviour.Hub.GameTime.GetPlaybackTime() < this._perkInitializedTime + this.DelayToCheckHitMillis)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			if (!this.IgnoreScenery)
			{
				flag2 = LayerManager.IsSceneryOrBombBlocker(this.Effect, other);
			}
			CombatObject combat = CombatRef.GetCombat(other);
			bool flag3 = this.HitOtherProjectiles && 13 == other.gameObject.layer;
			switch (this.Mode)
			{
			case PerkDestroyOnEnter.ModeEnum.IgnoreAllButTarget:
				flag = (combat && combat.Id == this.Effect.Target);
				goto IL_149;
			case PerkDestroyOnEnter.ModeEnum.IgnoreAllButOwner:
				flag = (combat && combat.Id == this.Effect.Owner);
				goto IL_149;
			case PerkDestroyOnEnter.ModeEnum.IgnoreAll:
				goto IL_149;
			case PerkDestroyOnEnter.ModeEnum.IgnoreDead:
				flag = (this.Effect.CheckHit(combat) && combat && combat.IsAlive());
				goto IL_149;
			}
			flag = this.Effect.CheckHit(combat);
			IL_149:
			if (flag || flag2 || flag3)
			{
				Vector3 previousPosition = this.Effect.PreviousPosition;
				int targetId = (!combat) ? -1 : combat.Id.ObjId;
				BaseFX.EDestroyReason reason = (!flag2) ? BaseFX.EDestroyReason.HitIdentifiable : BaseFX.EDestroyReason.HitScenery;
				if (this.Effect.TriggerDestroy(targetId, previousPosition, flag2, other, hitNormal, reason, isBarrier))
				{
					base.enabled = false;
				}
			}
		}

		public void OnStay(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnEnter(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnExit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDestroyOnEnter));

		public bool HitOtherProjectiles;

		public bool IgnoreScenery;

		public PerkDestroyOnEnter.ModeEnum Mode = PerkDestroyOnEnter.ModeEnum.Default;

		protected int _perkInitializedTime;

		public int DelayToCheckHitMillis;

		public enum ModeEnum
		{
			Default = 1,
			IgnoreAllButTarget,
			IgnoreAllButOwner,
			OBSOLETEPutDefault,
			IgnoreAll,
			IgnoreDead
		}
	}
}
