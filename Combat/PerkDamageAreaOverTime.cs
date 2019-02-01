using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageAreaOverTime : BasePerk, PerkAttachToObject.IEffectAttachListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			base.PerkInitialized();
			this._trans = base.transform;
			this._modifiers = base.GetModifiers(this.Source);
			this._lastPlaybackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			int num = playbackTime - this._lastPlaybackTime;
			this._lastPlaybackTime = playbackTime;
			float num2 = (float)num / 1000f;
			if (num2 < 0f)
			{
				PerkDamageAreaOverTime.Log.ErrorFormat("Negative Damage: amount {0} now {1} latPlaybackTime {2}", new object[]
				{
					num2,
					playbackTime,
					this._lastPlaybackTime
				});
			}
			this.DamageArea(num2);
		}

		private void DamageArea(float amount)
		{
			float num = (!this.UseGadgetRadius) ? this.Range : this.Effect.Gadget.Radius;
			if (!this.UseGadgetRadius && num < 0.1f)
			{
				num = this.Effect.Data.Range;
			}
			Vector3 center = this.GetCenter();
			Vector3 rayOrigin = this._trans.TransformDirection(this.DamageOriginOffset) + center;
			PerkDamageAreaOverTime.CollisionDetectionKind detectionKind = this.DetectionKind;
			if (detectionKind != PerkDamageAreaOverTime.CollisionDetectionKind.SweepTest)
			{
				if (detectionKind != PerkDamageAreaOverTime.CollisionDetectionKind.OverlapSphere)
				{
				}
				BarrierUtils.OverlapSphereThenFilter(center, num, this.Effect.Data.EffectInfo.PrioritizeBarrier, rayOrigin, 1077058560, this._areaHits);
			}
			else
			{
				this.SweepTestCollisionDetection(this._areaHits);
			}
			for (int i = 0; i < this._areaHits.Count; i++)
			{
				BarrierUtils.CombatHit combatHit = this._areaHits[i];
				CombatObject combat = combatHit.Combat;
				bool barrier = combatHit.Barrier;
				bool flag = this.Effect.CheckHit(combat);
				if (flag && combat && combat.Controller && !this._damagedCombats.Contains(combat))
				{
					float num2 = 1f;
					if (this.DamageByRange)
					{
						num2 = this.DamageToRange.Evaluate(Vector3.Distance(combat.Transform.position, center));
					}
					ModifierData[] array = ModifierData.CreateConvoluted(this._modifiers, num2 * amount);
					Vector3 normalized = (combat.Transform.position - center).normalized;
					BaseDamageablePerk.UpdateCustomDirection(this.Effect, array, combat, this.CustomDirection, normalized, center);
					combat.Controller.AddModifiers(array, this.Effect.Gadget.Combat, this.Effect.EventId, barrier);
					this._damagedCombats.Add(combat);
				}
			}
			if (this.IsDamageCallbackEnabled)
			{
				Mural.Post(new DamageAreaCallback(this._damagedCombats, center, this.Effect, this.TargetGadgetCallback), this.Effect.Gadget);
			}
			this._areaHits.Clear();
			this._damagedCombats.Clear();
		}

		private Vector3 GetCenter()
		{
			switch (this.Center)
			{
			case PerkDamageAreaOverTime.AreaCenter.EffectOrigin:
				return this.Effect.Data.Origin;
			case PerkDamageAreaOverTime.AreaCenter.EffectPosition:
				return this._trans.position;
			case PerkDamageAreaOverTime.AreaCenter.FollowTarget:
				if (!this.Effect.Target)
				{
					return Vector3.zero;
				}
				return this.Effect.Target.transform.position;
			}
			return this.Effect.Data.Target;
		}

		private void OnDrawGizmos()
		{
			if (!this.Effect || !this.Effect.Gadget || !this._trans)
			{
				return;
			}
			float num = (!this.UseGadgetRadius) ? this.Range : this.Effect.Gadget.Radius;
			if (!this.UseGadgetRadius && num == 0f)
			{
				num = this.Effect.Data.Range;
			}
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(this.GetCenter(), num);
		}

		public void OnAttachEffect(PerkAttachToObject.EffectAttachToTarget msg)
		{
			this._trans = msg.Target;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDamageAreaOnTick));

		public float Range;

		public bool UseGadgetRadius;

		public bool IsDamageCallbackEnabled;

		public bool DamageByRange;

		public AnimationCurve DamageToRange;

		public GadgetSlot TargetGadgetCallback;

		public BasePerk.DamageSource Source = BasePerk.DamageSource.EventModifiers;

		private ModifierData[] _modifiers;

		private new Transform _trans;

		public PerkDamageAreaOverTime.AreaCenter Center;

		public BaseDamageablePerk.ECustomDirection CustomDirection;

		public Vector3 DamageOriginOffset;

		private int _lastPlaybackTime;

		public PerkDamageAreaOverTime.CollisionDetectionKind DetectionKind;

		private readonly List<BarrierUtils.CombatHit> _areaHits = new List<BarrierUtils.CombatHit>(10);

		private readonly List<CombatObject> _damagedCombats = new List<CombatObject>();

		public enum CollisionDetectionKind
		{
			OverlapSphere,
			SweepTest
		}

		public enum AreaCenter
		{
			EffectTarget,
			EffectOrigin,
			EffectPosition,
			FollowTarget
		}
	}
}
