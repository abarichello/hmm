using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageOnStay : BaseDamageablePerk, IPerkWithCollision
	{
		public int Priority()
		{
			return -2;
		}

		protected override void OnPerkInitialized()
		{
			this._hitSet.Clear();
			this._modifiers = base.GetModifiers(this.Source);
			this._lastPlaybackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this._shouldSkipFirstFrame = true;
		}

		private void FixedUpdate()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this._shouldSkipFirstFrame)
			{
				this._shouldSkipFirstFrame = false;
				return;
			}
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			int num = playbackTime - this._lastPlaybackTime;
			this._lastPlaybackTime = playbackTime;
			float baseAmount = (float)num / 1000f;
			ModifierData[] modifiers = ModifierData.CreateConvoluted(this._modifiers, baseAmount);
			List<int> list = new List<int>(this._hitSet);
			for (int i = 0; i < list.Count; i++)
			{
				CombatObject combat = CombatRef.GetCombat(list[i]);
				base.ApplyDamage(combat, combat, false, modifiers);
			}
			this._hitSet.Clear();
		}

		public void OnStay(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
			if (this.Effect.IsDead || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.ServerHitEnter(other, isBarrier);
		}

		public void OnHit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
		}

		public void OnEnter(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
		}

		public void OnExit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
		}

		private void ServerHitEnter(Collider other, bool isBarrier)
		{
			if (this.SingleTarget && this.Damaged.Count > 0)
			{
				return;
			}
			CombatObject combatObject = null;
			CombatObject combatObject2 = null;
			bool flag = false;
			switch (this.Mode)
			{
			case PerkDamageOnEnter.ModeEnum.DamageOtherAndOwner:
				combatObject = (combatObject2 = CombatRef.GetCombat(other));
				flag = this.Effect.CheckHit(combatObject2);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageTargetOnEnterScenery:
				combatObject = (combatObject2 = CombatRef.GetCombat(this.Effect.Target));
				flag = (9 == other.gameObject.layer);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageOtherIgnoreTarget:
				combatObject = (combatObject2 = CombatRef.GetCombat(other));
				flag = (combatObject2 && combatObject2.Id != this.Effect.Target && this.Effect.CheckHit(combatObject2));
				break;
			case PerkDamageOnEnter.ModeEnum.DamageTarget:
				combatObject = CombatRef.GetCombat(this.Effect.Target);
				combatObject2 = CombatRef.GetCombat(other);
				flag = this.Effect.CheckHit(combatObject2);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageTargetIgnoreOther:
				combatObject = (combatObject2 = CombatRef.GetCombat(other));
				flag = (combatObject2 && combatObject2.Id == this.Effect.Target && this.Effect.CheckHit(combatObject2));
				break;
			case PerkDamageOnEnter.ModeEnum.DamageOwner:
				combatObject = CombatRef.GetCombat(this.Effect.Owner);
				combatObject2 = CombatRef.GetCombat(other);
				flag = this.Effect.CheckHit(combatObject2);
				break;
			case PerkDamageOnEnter.ModeEnum.DamageOther:
				combatObject = (combatObject2 = CombatRef.GetCombat(other));
				if (combatObject && combatObject.Id.ObjId != this.Effect.Owner.ObjId)
				{
					flag = this.Effect.CheckHit(combatObject2);
				}
				break;
			}
			if (flag && combatObject && combatObject2)
			{
				this._hitSet.Add(combatObject2.Id.ObjId);
			}
		}

		[Header("Use this mode, forget Target on this perk")]
		public PerkDamageOnEnter.ModeEnum Mode = PerkDamageOnEnter.ModeEnum.DamageOtherAndOwner;

		private readonly HashSet<int> _hitSet = new HashSet<int>();

		private ModifierData[] _modifiers;

		private int _lastPlaybackTime;

		private bool _shouldSkipFirstFrame;
	}
}
