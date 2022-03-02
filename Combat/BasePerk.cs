using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace HeavyMetalMachines.Combat
{
	public class BasePerk : GameHubBehaviour
	{
		protected Transform _trans
		{
			get
			{
				return this.Effect.AttachedTransform;
			}
		}

		protected virtual void Awake()
		{
			this.wasActive = base.enabled;
			if (!this.Effect)
			{
				this.Effect = base.GetComponent<BaseFX>();
			}
		}

		public virtual bool CanInitialize()
		{
			if (GameHubBehaviour.Hub.Net.IsClient() && this.Effect.Attached)
			{
				base.enabled = false;
				return false;
			}
			return true;
		}

		public virtual void PerkInitialized()
		{
			base.enabled = this.wasActive;
			this.Body = base.GetComponent<Rigidbody>();
		}

		public virtual void PerkUpdate()
		{
		}

		protected void OnDestroy()
		{
			this.Destroying();
		}

		protected virtual void Destroying()
		{
		}

		public virtual void PerkDestroyed(DestroyEffectMessage destroyEffectMessage)
		{
		}

		protected CombatObject GetTargetCombat(BaseFX baseFx, BasePerk.PerkTarget target)
		{
			CombatObject result = null;
			if (target != BasePerk.PerkTarget.Owner)
			{
				if (target != BasePerk.PerkTarget.Target)
				{
					if (target == BasePerk.PerkTarget.Effect)
					{
						result = CombatRef.GetCombat(GameHubBehaviour.Hub.ObjectCollection.GetObjectByKind(ContentKind.Wards.Byte(), baseFx.EventId));
					}
				}
				else
				{
					result = CombatRef.GetCombat(baseFx.Target);
				}
			}
			else
			{
				result = baseFx.Data.SourceCombat;
			}
			return result;
		}

		protected ModifierData[] GetModifiers(BasePerk.DamageSource damageSource)
		{
			switch (damageSource)
			{
			case BasePerk.DamageSource.StaticDamage:
			{
				ModifierData[] array = ModifierData.CreateData(this.StaticDamage, this.Effect.Gadget.Info);
				if (this.ConvolutedStaticDamage)
				{
					return ModifierData.CreateConvoluted(array, this.Effect.Gadget.CurrentHeat);
				}
				return array;
			}
			case BasePerk.DamageSource.EventModifiers:
				return ModifierData.CopyData(this.Effect.Data.Modifiers);
			case BasePerk.DamageSource.ExtraModifiers:
				return ModifierData.CopyData(this.Effect.Data.ExtraModifiers);
			default:
				return null;
			}
		}

		protected virtual void SweepTestCollisionDetection(List<BarrierUtils.CombatHit> combatHits)
		{
			combatHits.Clear();
			Vector3 position = this.Body.position;
			this.Body.position += BasePerk.Translation;
			RaycastHit[] array = this.Body.SweepTestAll(Vector3.down, 100f, 2);
			this.Body.position = position;
			foreach (RaycastHit raycastHit in array)
			{
				CombatObject combat = CombatRef.GetCombat(raycastHit.collider);
				if (!(combat == null))
				{
					combatHits.Add(new BarrierUtils.CombatHit
					{
						Combat = combat,
						Col = raycastHit.collider,
						Barrier = false
					});
				}
			}
		}

		protected Transform GetTargetTransform(BaseFX baseFx, BasePerk.PerkTarget target)
		{
			CombatObject targetCombat = this.GetTargetCombat(baseFx, target);
			return (!(targetCombat == null)) ? targetCombat.Transform : base.transform;
		}

		protected static Transform GetTargetDummy(Identifiable target, CDummy.DummyKind kind, string customDummyName)
		{
			if (target == null)
			{
				return null;
			}
			CDummy bitComponentInChildren = target.GetBitComponentInChildren<CDummy>();
			if (bitComponentInChildren == null)
			{
				return target.transform;
			}
			Transform dummy = bitComponentInChildren.GetDummy(kind, customDummyName, null);
			if (dummy == null)
			{
				return target.transform;
			}
			return dummy;
		}

		protected T GetComponentInTarget<T>(BaseFX baseFx, BasePerk.PerkTarget target, bool logInfoIfNotFound) where T : Component
		{
			CombatObject targetCombat = this.GetTargetCombat(baseFx, target);
			return targetCombat.GetComponent<T>();
		}

		public string PerkVFXCondition;

		[HideInInspector]
		[SerializeField]
		public BaseFX Effect;

		[Inject]
		private StateMachine _stateMachine;

		protected bool wasActive;

		protected Rigidbody Body;

		[Tooltip("Multiply damage by Current Heat of the Gadget")]
		public bool ConvolutedStaticDamage;

		[FormerlySerializedAs("Damage")]
		[Tooltip("Will only work if you select source = StaticDamage")]
		public ModifierInfo[] StaticDamage;

		private const float TranslationDistance = 100f;

		private static readonly Vector3 Translation = new Vector3(0f, 100f, 0f);

		public enum PerkTarget
		{
			Owner,
			Target,
			Effect,
			None
		}

		public enum DamageSource
		{
			StaticDamage,
			EventModifiers,
			ExtraModifiers
		}
	}
}
