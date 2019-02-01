using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.UpdateStream;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class CombatAttributes : StreamContent
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatAttributes.StreamAppliedListener OnStreamApplied;

		public float HPMax
		{
			get
			{
				this.CheckDirty();
				return this._hpMax;
			}
		}

		public float HPMaxPct
		{
			get
			{
				this.CheckDirty();
				return this._hpMaxPct;
			}
		}

		public float HPRegen
		{
			get
			{
				this.CheckDirty();
				return this._hpRegen;
			}
		}

		public float HPRegenPct
		{
			get
			{
				this.CheckDirty();
				return this._hpRegenPct;
			}
		}

		public float HPPartialRegenPct
		{
			get
			{
				this.CheckDirty();
				return this._hpPartialRegenPct;
			}
		}

		public float HPPureDamage
		{
			get
			{
				this.CheckDirty();
				return this._hpPureDamage;
			}
		}

		public float HPPureDamagePct
		{
			get
			{
				this.CheckDirty();
				return this._hpPureDamagePct;
			}
		}

		public float HPPureArmor
		{
			get
			{
				this.CheckDirty();
				return this._hpPureArmor;
			}
		}

		public float HPPureArmorPct
		{
			get
			{
				this.CheckDirty();
				return this._hpPureArmorPct;
			}
		}

		public float EPMax
		{
			get
			{
				this.CheckDirty();
				return this._epMax;
			}
		}

		public float EPMaxPct
		{
			get
			{
				this.CheckDirty();
				return this._epMaxPct;
			}
		}

		public float EPRegen
		{
			get
			{
				this.CheckDirty();
				return this._epRegen;
			}
		}

		public float EPRegenPct
		{
			get
			{
				this.CheckDirty();
				return this._epRegenPct;
			}
		}

		public float EPPartialRegenPct
		{
			get
			{
				this.CheckDirty();
				return this._epPartialRegenPct;
			}
		}

		public float AccelerationModPct
		{
			get
			{
				this.CheckDirty();
				return this._accelPct;
			}
		}

		public float AccelerationMod
		{
			get
			{
				this.CheckDirty();
				return this._accel;
			}
		}

		public float BackAccelModPct
		{
			get
			{
				this.CheckDirty();
				return this._backwardAccelPct;
			}
		}

		public float BackAccelMod
		{
			get
			{
				this.CheckDirty();
				return this._backwardAccel;
			}
		}

		public float BrakeAccelModPct
		{
			get
			{
				this.CheckDirty();
				return this._brakeAccelPct;
			}
		}

		public float BrakeAccelMod
		{
			get
			{
				this.CheckDirty();
				return this._brakeAccel;
			}
		}

		public float GripExtraFwdAccelModPct
		{
			get
			{
				this.CheckDirty();
				return this._gripExtraFwdAccelerationPct;
			}
		}

		public float GripExtraFwdAccelMod
		{
			get
			{
				this.CheckDirty();
				return this._gripExtraFwdAcceleration;
			}
		}

		public float GripExtraBackAccelModPct
		{
			get
			{
				this.CheckDirty();
				return this._gripExtraBackAccelerationPct;
			}
		}

		public float GripExtraBackAccelMod
		{
			get
			{
				this.CheckDirty();
				return this._gripExtraBackAcceleration;
			}
		}

		public float DragMod
		{
			get
			{
				this.CheckDirty();
				return this._drag;
			}
		}

		public float DragModPct
		{
			get
			{
				this.CheckDirty();
				return this._dragPct;
			}
		}

		public float DriftDragMod
		{
			get
			{
				this.CheckDirty();
				return this._driftDrag;
			}
		}

		public float DriftDragModPct
		{
			get
			{
				this.CheckDirty();
				return this._driftDragPct;
			}
		}

		public float LateralFriction
		{
			get
			{
				this.CheckDirty();
				return this._lateralFriction;
			}
		}

		public float LateralFrictionPct
		{
			get
			{
				this.CheckDirty();
				return this._lateralFrictionPct;
			}
		}

		public float HPLightDamage
		{
			get
			{
				this.CheckDirty();
				return this._hpLightDamage;
			}
		}

		public float HPLightDamagePct
		{
			get
			{
				this.CheckDirty();
				return this._hpLightDamagePct;
			}
		}

		public float HPLightArmor
		{
			get
			{
				this.CheckDirty();
				return this._hpLightArmor;
			}
		}

		public float HPLightArmorPct
		{
			get
			{
				this.CheckDirty();
				return this._hpLightArmorPct;
			}
		}

		public float HPHeavyDamage
		{
			get
			{
				this.CheckDirty();
				return this._hpHeavyDamage;
			}
		}

		public float HPHeavyDamagePct
		{
			get
			{
				this.CheckDirty();
				return this._hpHeavyDamagePct;
			}
		}

		public float HPHeavyArmor
		{
			get
			{
				this.CheckDirty();
				return this._hpHeavyArmor;
			}
		}

		public float HPHeavyArmorPct
		{
			get
			{
				this.CheckDirty();
				return this._hpHeavyArmorPct;
			}
		}

		public float HPRepairArmor
		{
			get
			{
				this.CheckDirty();
				return this._hpRepairArmor;
			}
		}

		public float HPRepairArmorPct
		{
			get
			{
				this.CheckDirty();
				return this._hpRepairArmorPct;
			}
		}

		public float FireRate
		{
			get
			{
				this.CheckDirty();
				return this._fireRate;
			}
		}

		public float FireRatePct
		{
			get
			{
				this.CheckDirty();
				return this._fireRatePct;
			}
		}

		public int ScrapBonus
		{
			get
			{
				this.CheckDirty();
				return this._scrapBonus;
			}
		}

		public float ScrapBonusPct
		{
			get
			{
				this.CheckDirty();
				return this._scrapBonusPct;
			}
		}

		public float PowerPct
		{
			get
			{
				this.CheckDirty();
				return this._powerPct;
			}
		}

		public float Mass
		{
			get
			{
				this.CheckDirty();
				return this._mass;
			}
		}

		public float MassPct
		{
			get
			{
				this.CheckDirty();
				return this._massPct;
			}
		}

		public float PushForcePct
		{
			get
			{
				this.CheckDirty();
				return this._pushForceModPct;
			}
		}

		public float PushReceivedPct
		{
			get
			{
				this.CheckDirty();
				return this._pushReceivedModPct;
			}
		}

		public float SceneryBouncinessPct
		{
			get
			{
				this.CheckDirty();
				return this._sceneryBouncinessModPct;
			}
		}

		public float TurningRadiusPct
		{
			get
			{
				this.CheckDirty();
				return this._turningRadiusPct;
			}
		}

		public float TurningRadius
		{
			get
			{
				this.CheckDirty();
				return this._turningRadius;
			}
		}

		public float MaxAngularPushPct
		{
			get
			{
				this.CheckDirty();
				return this._maxAngularPushPct;
			}
		}

		public float MaxAngularPush
		{
			get
			{
				this.CheckDirty();
				return this._maxAngularPush;
			}
		}

		public float ForcedAngularPush
		{
			get
			{
				this.CheckDirty();
				return this._forcedAngularPush;
			}
		}

		public float MaxAngularSpeed
		{
			get
			{
				this.CheckDirty();
				return this._maxAngularSpeed;
			}
		}

		public float MaxAngularSpeedPct
		{
			get
			{
				this.CheckDirty();
				return this._maxAngularSpeedPct;
			}
		}

		public float CooldownReductionGadget0
		{
			get
			{
				this.CheckDirty();
				return this._cooldownReductionGadget0;
			}
		}

		public float CooldownReductionGadget1
		{
			get
			{
				this.CheckDirty();
				return this._cooldownReductionGadget1;
			}
		}

		public float CooldownReductionGadget2
		{
			get
			{
				this.CheckDirty();
				return this._cooldownReductionGadget2;
			}
		}

		public float CooldownReductionGadgetB
		{
			get
			{
				this.CheckDirty();
				return this._cooldownReductionGadgetB;
			}
		}

		public float CooldownReductionGadget0Pct
		{
			get
			{
				this.CheckDirty();
				return this._cooldownReductionGadget0Pct;
			}
		}

		public float CooldownReductionGadget1Pct
		{
			get
			{
				this.CheckDirty();
				return this._cooldownReductionGadget1Pct;
			}
		}

		public float CooldownReductionGadget2Pct
		{
			get
			{
				this.CheckDirty();
				return this._cooldownReductionGadget2Pct;
			}
		}

		public float CooldownReductionGadgetBPct
		{
			get
			{
				this.CheckDirty();
				return this._cooldownReductionGadgetBPct;
			}
		}

		public float CrowdControlReduction
		{
			get
			{
				this.CheckDirty();
				return this._crowdControlReduction;
			}
		}

		public StatusKind CurrentStatus
		{
			get
			{
				this.CheckDirty();
				return this._currentStatus;
			}
		}

		public Vector3 ImmobilizedDirection
		{
			get
			{
				this.CheckDirty();
				return this._immobilizedDirection;
			}
		}

		public List<string> SupressedTags
		{
			get
			{
				this.CheckDirty();
				return this._supressedTags;
			}
		}

		public bool IsInvulnerable
		{
			get
			{
				return this.CurrentStatus.HasFlag(StatusKind.Invulnerable) || this.ForceInvincible;
			}
			set
			{
				if (this.ForceInvincible == value)
				{
					return;
				}
				this.SetDirty();
				this.ForceInvincible = value;
			}
		}

		public bool IsGadgetDisarmed(GadgetSlot slot, GadgetNatureKind nature)
		{
			if (nature != (GadgetNatureKind)0)
			{
				bool flag = (nature.HasFlag(GadgetNatureKind.Damage) && this._currentStatus.HasFlag(StatusKind.DamageDisarmed)) || (nature.HasFlag(GadgetNatureKind.Healing) && this._currentStatus.HasFlag(StatusKind.HealingDisarmed)) || (nature.HasFlag(GadgetNatureKind.SpeedBoost) && this._currentStatus.HasFlag(StatusKind.SpeedBoostDisarmed)) || (nature.HasFlag(GadgetNatureKind.Teleport) && this._currentStatus.HasFlag(StatusKind.TeleportDisarmed));
				if (flag)
				{
					return true;
				}
			}
			switch (slot)
			{
			case GadgetSlot.CustomGadget0:
				return this._currentStatus.HasFlag(StatusKind.Gadget0Disarmed) || this._currentStatus.HasFlag(StatusKind.Disarmed);
			case GadgetSlot.CustomGadget1:
				return this._currentStatus.HasFlag(StatusKind.Gadget1Disarmed) || this._currentStatus.HasFlag(StatusKind.Disarmed);
			case GadgetSlot.CustomGadget2:
				return this._currentStatus.HasFlag(StatusKind.Gadget2Disarmed);
			case GadgetSlot.BoostGadget:
			case GadgetSlot.GenericGadget:
				return this._currentStatus.HasFlag(StatusKind.Jammed);
			case GadgetSlot.PassiveGadget:
				return this._currentStatus.HasFlag(StatusKind.PassiveDisarmed);
			case GadgetSlot.BombGadget:
			{
				bool flag2 = GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this.Combat);
				return this._currentStatus.HasFlag((!flag2) ? StatusKind.GrabberBlocked : StatusKind.PassBlocked);
			}
			}
			return false;
		}

		public void SetDirty()
		{
			if (!this._isDirty)
			{
				GameHubBehaviour.Hub.Stream.CombatAttStream.Changed(this);
			}
			this._isDirty = true;
		}

		public CombatObject Combat
		{
			get
			{
				CombatObject result;
				if ((result = this._combat) == null)
				{
					result = (this._combat = base.GetComponent<CombatObject>());
				}
				return result;
			}
		}

		public CombatObject CurrentBanishCauserCombat
		{
			get
			{
				return this._currentBanishCauserCombat;
			}
		}

		public void CheckDirty()
		{
			if (!this._isDirty)
			{
				return;
			}
			this._isDirty = false;
			CombatController controller = this.Combat.Controller;
			float crowdControlReduction = this.CrowdControlReduction;
			bool flag = false;
			bool isUnstoppable = false;
			this.Clear();
			for (int i = 0; i < controller.PassiveAttrStatusList.Count; i++)
			{
				ModifierInstance modifierInstance = controller.PassiveAttrStatusList[i];
				if (!modifierInstance.BarrierHit || modifierInstance.Info.IgnoreBarrier)
				{
					this.ParseMod(modifierInstance, ref flag, ref isUnstoppable);
				}
			}
			for (int j = 0; j < controller.TimedAttrStatusList.Count; j++)
			{
				ModifierInstance modifierInstance2 = controller.TimedAttrStatusList[j];
				if (!modifierInstance2.BarrierHit || modifierInstance2.Info.IgnoreBarrier)
				{
					this.ParseMod(modifierInstance2, ref flag, ref isUnstoppable);
				}
			}
			for (int k = 0; k < this._untaggedMods.Count; k++)
			{
				ModifierInstance modifierInstance3 = this._untaggedMods[k];
				if (!modifierInstance3.BarrierHit || modifierInstance3.Info.IgnoreBarrier)
				{
					this.Apply(modifierInstance3, isUnstoppable);
				}
			}
			foreach (KeyValuePair<string, ModifierInstance> keyValuePair in this._max)
			{
				if (!this._supressedTags.Contains(keyValuePair.Key))
				{
					this.Apply(keyValuePair.Value, isUnstoppable);
				}
			}
			foreach (KeyValuePair<string, ModifierInstance> keyValuePair2 in this._min)
			{
				if (!this._supressedTags.Contains(keyValuePair2.Key))
				{
					this.Apply(keyValuePair2.Value, isUnstoppable);
				}
			}
			if (crowdControlReduction != this.CrowdControlReduction && flag)
			{
				controller.RefreshCrowdControlReduction();
			}
			this.Combat.AttributeChanged();
		}

		private void ParseMod(ModifierInstance mod, ref bool isCrowdControl, ref bool isUnstoppable)
		{
			mod.NotBeingApplied = true;
			if (mod.Data.Info.Attribute.IsCrowdControl(mod.Amount) || mod.Data.Status.IsCrowdControl())
			{
				isCrowdControl = true;
			}
			isUnstoppable |= mod.Data.Status.HasFlag(StatusKind.Unstoppable);
			if (mod.Info.Attribute == AttributeBuffKind.SupressTargetTag && !string.IsNullOrEmpty(mod.Info.TargetTag))
			{
				this._supressedTags.Add(mod.Info.TargetTag);
			}
			if (string.IsNullOrEmpty(mod.Info.Tag))
			{
				this._untaggedMods.Add(mod);
				return;
			}
			if (mod.Amount >= 0f)
			{
				ModifierInstance modifierInstance;
				this._max.TryGetValue(mod.Info.Tag, out modifierInstance);
				if (modifierInstance == null || mod.Amount > modifierInstance.Amount)
				{
					this._max[mod.Info.Tag] = mod;
				}
			}
			else
			{
				ModifierInstance modifierInstance2;
				this._min.TryGetValue(mod.Info.Tag, out modifierInstance2);
				if (modifierInstance2 == null || mod.Amount < modifierInstance2.Amount)
				{
					this._min[mod.Info.Tag] = mod;
				}
			}
		}

		private void Clear()
		{
			this._max.Clear();
			this._min.Clear();
			this._untaggedMods.Clear();
			this._supressedTags.Clear();
			this._hpMax.Set(0f);
			this._hpMaxPct.Set(0f);
			this._hpRegen.Set(0f);
			this._hpRegenPct.Set(0f);
			this._hpPartialRegenPct.Set(0f);
			this._hpPureDamage.Set(0f);
			this._hpPureDamagePct.Set(0f);
			this._hpPureArmor.Set(0f);
			this._hpPureArmorPct.Set(0f);
			this._epMax.Set(0f);
			this._epMaxPct.Set(0f);
			this._epRegen.Set(0f);
			this._epRegenPct.Set(0f);
			this._epPartialRegenPct.Set(0f);
			this._accelPct.Set(0f);
			this._accel.Set(0f);
			this._backwardAccelPct.Set(0f);
			this._backwardAccel.Set(0f);
			this._brakeAccelPct.Set(0f);
			this._brakeAccel.Set(0f);
			this._hpLightDamage.Set(0f);
			this._hpLightDamagePct.Set(0f);
			this._hpLightArmor.Set(0f);
			this._hpLightArmorPct.Set(0f);
			this._hpHeavyDamage.Set(0f);
			this._hpHeavyDamagePct.Set(0f);
			this._hpHeavyArmor.Set(0f);
			this._hpHeavyArmorPct.Set(0f);
			this._hpRepairArmor.Set(0f);
			this._hpRepairArmorPct.Set(0f);
			this._fireRate.Set(0f);
			this._fireRatePct.Set(0f);
			this._scrapBonus.Set(0);
			this._scrapBonusPct.Set(0f);
			this._gripExtraFwdAcceleration.Set(0f);
			this._gripExtraFwdAccelerationPct.Set(0f);
			this._gripExtraBackAcceleration.Set(0f);
			this._gripExtraBackAccelerationPct.Set(0f);
			this._dragPct.Set(0f);
			this._drag.Set(0f);
			this._driftDrag.Set(0f);
			this._driftDragPct.Set(0f);
			this._lateralFriction.Set(0f);
			this._lateralFrictionPct.Set(0f);
			this._currentStatus = StatusKind.None;
			this._cooldownReductionGadget0.Set(0f);
			this._cooldownReductionGadget1.Set(0f);
			this._cooldownReductionGadget2.Set(0f);
			this._cooldownReductionGadgetB.Set(0f);
			this._cooldownReductionGadget0Pct.Set(0f);
			this._cooldownReductionGadget1Pct.Set(0f);
			this._cooldownReductionGadget2Pct.Set(0f);
			this._cooldownReductionGadgetBPct.Set(0f);
			this._crowdControlReduction.Set(0f);
			this._powerPct.Set(0f);
			this._currentBanishCauserCombat = null;
			this._immobilizedDirection = Vector3.zero;
			this._mass.Set(0f);
			this._massPct.Set(0f);
			this._pushForceModPct.Set(0f);
			this._pushReceivedModPct.Set(0f);
			this._sceneryBouncinessModPct.Set(0f);
			this._turningRadiusPct.Set(0f);
			this._turningRadius.Set(0f);
			this._maxAngularPush.Set(0f);
			this._maxAngularPushPct.Set(0f);
			this._forcedAngularPush.Set(0f);
			this._maxAngularSpeed.Set(0f);
			this._maxAngularSpeedPct.Set(0f);
		}

		private void Apply(ModifierInstance mod, bool isUnstoppable)
		{
			mod.NotBeingApplied = false;
			switch (mod.Info.Attribute)
			{
			case AttributeBuffKind.HPMax:
				if (mod.Info.IsPercent)
				{
					this._hpMaxPct += mod.Amount;
				}
				else
				{
					this._hpMax += mod.Amount;
				}
				return;
			case AttributeBuffKind.HPRegen:
				if (mod.Info.IsPercent)
				{
					this._hpRegenPct += mod.Amount;
				}
				else
				{
					this._hpRegen += mod.Amount;
				}
				return;
			case AttributeBuffKind.HPPureDamageBuff:
				if (mod.Info.IsPercent)
				{
					this._hpPureDamagePct += mod.Amount;
				}
				else
				{
					this._hpPureDamage += mod.Amount;
				}
				return;
			case AttributeBuffKind.HPPureArmor:
				if (mod.Info.IsPercent)
				{
					this._hpPureArmorPct += mod.Amount;
				}
				else
				{
					this._hpPureArmor += mod.Amount;
				}
				return;
			case AttributeBuffKind.EPMax:
				if (mod.Info.IsPercent)
				{
					this._epMaxPct += mod.Amount;
				}
				else
				{
					this._epMax += mod.Amount;
				}
				return;
			case AttributeBuffKind.EPRegen:
				if (mod.Info.IsPercent)
				{
					this._epRegenPct += mod.Amount;
				}
				else
				{
					this._epRegen += mod.Amount;
				}
				return;
			case AttributeBuffKind.HPLightDamageBuff:
				if (mod.Info.IsPercent)
				{
					this._hpLightDamagePct += mod.Amount;
				}
				else
				{
					this._hpLightDamage += mod.Amount;
				}
				return;
			case AttributeBuffKind.HPLightArmor:
				if (mod.Info.IsPercent)
				{
					this._hpLightArmorPct += mod.Amount;
				}
				else
				{
					this._hpLightArmor += mod.Amount;
				}
				return;
			case AttributeBuffKind.FireRate:
				if (mod.Info.IsPercent)
				{
					this._fireRatePct += mod.Amount;
				}
				else
				{
					this._fireRate += mod.Amount;
				}
				return;
			case AttributeBuffKind.HPHeavyDamageBuff:
				if (mod.Data.Info.IsPercent)
				{
					this._hpHeavyDamagePct += mod.Amount;
				}
				else
				{
					this._hpHeavyDamage += mod.Amount;
				}
				return;
			case AttributeBuffKind.HPHeavyArmor:
				if (mod.Data.Info.IsPercent)
				{
					this._hpHeavyArmorPct += mod.Amount;
				}
				else
				{
					this._hpHeavyArmor += mod.Amount;
				}
				return;
			case AttributeBuffKind.ScrapBonus:
				if (mod.Data.Info.IsPercent)
				{
					this._scrapBonusPct += mod.Amount;
				}
				else
				{
					this._scrapBonus += (int)Math.Round((double)mod.Amount);
				}
				return;
			case AttributeBuffKind.CooldownReduction:
				if (mod.Info.IsPercent)
				{
					switch (mod.Info.TargetGadget)
					{
					case TargetGadget.Gadget0:
						this._cooldownReductionGadget0Pct += mod.Amount;
						break;
					case TargetGadget.Gadget1:
						this._cooldownReductionGadget1Pct += mod.Amount;
						break;
					case TargetGadget.Gadgets01:
						this._cooldownReductionGadget0Pct += mod.Amount;
						this._cooldownReductionGadget1Pct += mod.Amount;
						break;
					case TargetGadget.Gadget2:
						this._cooldownReductionGadget2Pct += mod.Amount;
						break;
					case TargetGadget.Gadgets12:
						this._cooldownReductionGadget1Pct += mod.Amount;
						this._cooldownReductionGadget2Pct += mod.Amount;
						break;
					case TargetGadget.GadgetBoost:
						this._cooldownReductionGadgetBPct += mod.Amount;
						break;
					case TargetGadget.All:
						this._cooldownReductionGadget0Pct += mod.Amount;
						this._cooldownReductionGadget1Pct += mod.Amount;
						this._cooldownReductionGadget2Pct += mod.Amount;
						this._cooldownReductionGadgetBPct += mod.Amount;
						break;
					}
				}
				else
				{
					switch (mod.Info.TargetGadget)
					{
					case TargetGadget.Gadget0:
						this._cooldownReductionGadget0 += mod.Amount;
						break;
					case TargetGadget.Gadget1:
						this._cooldownReductionGadget1 += mod.Amount;
						break;
					case TargetGadget.Gadgets01:
						this._cooldownReductionGadget0 += mod.Amount;
						this._cooldownReductionGadget1 += mod.Amount;
						break;
					case TargetGadget.Gadget2:
						this._cooldownReductionGadget2 += mod.Amount;
						break;
					case TargetGadget.Gadgets12:
						this._cooldownReductionGadget1 += mod.Amount;
						this._cooldownReductionGadget2 += mod.Amount;
						break;
					case TargetGadget.GadgetBoost:
						this._cooldownReductionGadgetB += mod.Amount;
						break;
					case TargetGadget.All:
						this._cooldownReductionGadget0 += mod.Amount;
						this._cooldownReductionGadget1 += mod.Amount;
						this._cooldownReductionGadget2 += mod.Amount;
						this._cooldownReductionGadgetB += mod.Amount;
						break;
					}
				}
				return;
			case AttributeBuffKind.CrowdControlReduction:
				this._crowdControlReduction += mod.Amount;
				return;
			case AttributeBuffKind.Power:
				this._powerPct += mod.Amount;
				return;
			case AttributeBuffKind.HpPartialRegen:
				if (mod.Info.IsPercent)
				{
					this._hpPartialRegenPct += mod.Amount;
				}
				return;
			case AttributeBuffKind.EpPartialRegen:
				if (mod.Info.IsPercent)
				{
					this._epPartialRegenPct += mod.Amount;
				}
				return;
			case AttributeBuffKind.ForwardAcceleration:
				if (mod.Amount > 0f || !this.CurrentStatus.HasFlag(StatusKind.Unstoppable))
				{
					if (mod.Info.IsPercent)
					{
						this._accelPct += (1f + this._accelPct) * mod.Amount;
					}
					else
					{
						this._accel += mod.Amount;
					}
				}
				return;
			case AttributeBuffKind.Drag:
				if (mod.Amount < 0f || !this.CurrentStatus.HasFlag(StatusKind.Unstoppable))
				{
					if (mod.Info.IsPercent)
					{
						this._dragPct += mod.Amount;
					}
					else
					{
						this._drag += mod.Amount;
					}
				}
				return;
			case AttributeBuffKind.PushForce:
				this._pushForceModPct += mod.Amount;
				return;
			case AttributeBuffKind.PushReceived:
				this._pushReceivedModPct += mod.Amount;
				return;
			case AttributeBuffKind.BackwardAcceleration:
				if (mod.Amount > 0f || !this.CurrentStatus.HasFlag(StatusKind.Unstoppable))
				{
					if (mod.Info.IsPercent)
					{
						this._backwardAccelPct += (1f + this._backwardAccelPct) * mod.Amount;
					}
					else
					{
						this._backwardAccel += mod.Amount;
					}
				}
				return;
			case AttributeBuffKind.BrakeAcceleration:
				if (mod.Info.IsPercent)
				{
					this._brakeAccelPct += (1f + this._brakeAccelPct) * mod.Amount;
				}
				else
				{
					this._brakeAccel += mod.Amount;
				}
				return;
			case AttributeBuffKind.GripExtraFwdAcceleration:
				if (mod.Amount > 0f || !this.CurrentStatus.HasFlag(StatusKind.Unstoppable))
				{
					if (mod.Info.IsPercent)
					{
						this._gripExtraFwdAccelerationPct += (1f + this._gripExtraFwdAccelerationPct) * mod.Amount;
					}
					else
					{
						this._gripExtraFwdAcceleration += mod.Amount;
					}
				}
				return;
			case AttributeBuffKind.GripExtraBackAcceleration:
				if (mod.Amount > 0f || !this.CurrentStatus.HasFlag(StatusKind.Unstoppable))
				{
					if (mod.Info.IsPercent)
					{
						this._gripExtraBackAccelerationPct += (1f + this._gripExtraBackAccelerationPct) * mod.Amount;
					}
					else
					{
						this._gripExtraBackAcceleration += mod.Amount;
					}
				}
				return;
			case AttributeBuffKind.SceneryBounciness:
				this._sceneryBouncinessModPct += mod.Amount;
				return;
			case AttributeBuffKind.TurningRadius:
				if (mod.Data.Info.IsPercent)
				{
					this._turningRadiusPct += mod.Amount;
				}
				else
				{
					this._turningRadius += mod.Amount;
				}
				return;
			case AttributeBuffKind.DriftDrag:
				if (mod.Amount < 0f || !this.CurrentStatus.HasFlag(StatusKind.Unstoppable))
				{
					if (mod.Info.IsPercent)
					{
						this._driftDragPct += mod.Amount;
					}
					else
					{
						this._driftDrag += mod.Amount;
					}
				}
				return;
			case AttributeBuffKind.LateralFriction:
				if (mod.Amount < 0f || !this.CurrentStatus.HasFlag(StatusKind.Unstoppable))
				{
					if (mod.Info.IsPercent)
					{
						this._lateralFrictionPct += mod.Amount;
					}
					else
					{
						this._lateralFriction += mod.Amount;
					}
				}
				return;
			case AttributeBuffKind.MaxAngularPush:
				if (mod.Data.Info.IsPercent)
				{
					this._maxAngularPushPct += mod.Amount;
				}
				else
				{
					this._maxAngularPush += mod.Amount;
				}
				return;
			case AttributeBuffKind.ForcedAngularPush:
			{
				Vector3 vector = Vector3.Cross(mod.OwnerForward, mod.GetDirectionNomalized());
				this._forcedAngularPush += mod.Amount * (float)Math.Sign(vector.y);
				return;
			}
			case AttributeBuffKind.MaxAngularSpeed:
				if (mod.Data.Info.IsPercent)
				{
					this._maxAngularSpeedPct += mod.Amount;
				}
				else
				{
					this._maxAngularSpeed += mod.Amount;
				}
				return;
			case AttributeBuffKind.Mass:
				if (mod.Data.Info.IsPercent)
				{
					this._massPct += mod.Amount;
				}
				else
				{
					this._mass += mod.Amount;
				}
				return;
			case AttributeBuffKind.HPRepairArmor:
				if (mod.Data.Info.IsPercent)
				{
					this._hpRepairArmorPct += mod.Amount;
				}
				else
				{
					this._hpRepairArmor += mod.Amount;
				}
				return;
			}
			if (mod.Data.Status != StatusKind.None)
			{
				this._currentStatus |= mod.Data.Status;
				if (mod.Data.Status.HasFlag(StatusKind.Banished))
				{
					this._currentBanishCauserCombat = mod.Causer;
				}
				if (mod.Data.Status.HasFlag(StatusKind.Immobilized))
				{
					this._immobilizedDirection = mod.GetDirection();
				}
				if (mod.Data.Status.HasFlag(StatusKind.Unstoppable))
				{
					this._drag = 0f;
					this._dragPct = 0f;
				}
			}
		}

		public override int GetStreamData(ref byte[] data, bool boForceSerialization)
		{
			this.CheckDirty();
			Pocketverse.BitStream stream = base.GetStream();
			this._hpMax.Serialize(stream, boForceSerialization);
			this._hpRegen.Serialize(stream, boForceSerialization);
			this._hpPureDamage.Serialize(stream, boForceSerialization);
			this._hpPureArmor.Serialize(stream, boForceSerialization);
			this._hpLightDamage.Serialize(stream, boForceSerialization);
			this._hpLightArmor.Serialize(stream, boForceSerialization);
			this._hpHeavyDamage.Serialize(stream, boForceSerialization);
			this._hpHeavyArmor.Serialize(stream, boForceSerialization);
			this._hpRepairArmor.Serialize(stream, boForceSerialization);
			this._epMax.Serialize(stream, boForceSerialization);
			this._epRegen.Serialize(stream, boForceSerialization);
			this._fireRate.Serialize(stream, boForceSerialization);
			this._hpMaxPct.Serialize(stream, 2, boForceSerialization);
			this._hpRegenPct.Serialize(stream, 2, boForceSerialization);
			this._hpPartialRegenPct.Serialize(stream, 2, boForceSerialization);
			this._hpPureDamagePct.Serialize(stream, 2, boForceSerialization);
			this._hpPureArmorPct.Serialize(stream, 2, boForceSerialization);
			this._hpLightDamagePct.Serialize(stream, 2, boForceSerialization);
			this._hpLightArmorPct.Serialize(stream, 2, boForceSerialization);
			this._hpHeavyDamagePct.Serialize(stream, 2, boForceSerialization);
			this._hpHeavyArmorPct.Serialize(stream, 2, boForceSerialization);
			this._hpRepairArmorPct.Serialize(stream, 2, boForceSerialization);
			this._epMaxPct.Serialize(stream, 2, boForceSerialization);
			this._epRegenPct.Serialize(stream, 2, boForceSerialization);
			this._epPartialRegenPct.Serialize(stream, 2, boForceSerialization);
			this._fireRatePct.Serialize(stream, 2, boForceSerialization);
			this._accel.Serialize(stream, boForceSerialization);
			this._backwardAccel.Serialize(stream, boForceSerialization);
			this._brakeAccel.Serialize(stream, boForceSerialization);
			this._turningRadius.Serialize(stream, boForceSerialization);
			this._maxAngularPush.Serialize(stream, boForceSerialization);
			this._maxAngularPushPct.Serialize(stream, boForceSerialization);
			this._maxAngularSpeed.Serialize(stream, boForceSerialization);
			this._maxAngularSpeedPct.Serialize(stream, boForceSerialization);
			this._forcedAngularPush.Serialize(stream, boForceSerialization);
			this._gripExtraFwdAcceleration.Serialize(stream, boForceSerialization);
			this._gripExtraBackAcceleration.Serialize(stream, boForceSerialization);
			this._drag.Serialize(stream, boForceSerialization);
			this._driftDrag.Serialize(stream, boForceSerialization);
			this._lateralFriction.Serialize(stream, boForceSerialization);
			this._lateralFrictionPct.Serialize(stream, boForceSerialization);
			this._accelPct.Serialize(stream, 2, boForceSerialization);
			this._backwardAccelPct.Serialize(stream, 2, boForceSerialization);
			this._brakeAccelPct.Serialize(stream, 2, boForceSerialization);
			this._turningRadiusPct.Serialize(stream, 2, boForceSerialization);
			this._gripExtraFwdAccelerationPct.Serialize(stream, 2, boForceSerialization);
			this._gripExtraBackAccelerationPct.Serialize(stream, 2, boForceSerialization);
			this._dragPct.Serialize(stream, 2, boForceSerialization);
			this._driftDragPct.Serialize(stream, boForceSerialization);
			this._mass.Serialize(stream, 2, boForceSerialization);
			this._massPct.Serialize(stream, 2, boForceSerialization);
			this._pushForceModPct.Serialize(stream, 2, boForceSerialization);
			this._pushReceivedModPct.Serialize(stream, 2, boForceSerialization);
			this._sceneryBouncinessModPct.Serialize(stream, 2, boForceSerialization);
			this._cooldownReductionGadget0.Serialize(stream, boForceSerialization);
			this._cooldownReductionGadget1.Serialize(stream, boForceSerialization);
			this._cooldownReductionGadget2.Serialize(stream, boForceSerialization);
			this._cooldownReductionGadgetB.Serialize(stream, boForceSerialization);
			this._scrapBonus.Serialize(stream, boForceSerialization);
			this._crowdControlReduction.Serialize(stream, boForceSerialization);
			this._cooldownReductionGadget0Pct.Serialize(stream, 2, boForceSerialization);
			this._cooldownReductionGadget1Pct.Serialize(stream, 2, boForceSerialization);
			this._cooldownReductionGadget2Pct.Serialize(stream, 2, boForceSerialization);
			this._cooldownReductionGadgetBPct.Serialize(stream, 2, boForceSerialization);
			this._scrapBonusPct.Serialize(stream, 2, boForceSerialization);
			this._powerPct.Serialize(stream, 2, boForceSerialization);
			stream.WriteBool(this.ForceInvincible);
			stream.WriteCompressedInt((int)this._currentStatus);
			return stream.CopyToArray(data);
		}

		public override void ApplyStreamData(byte[] data)
		{
			Pocketverse.BitStream streamFor = base.GetStreamFor(data);
			this._isDirty = false;
			this._hpMax.DeSerialize(streamFor);
			this._hpRegen.DeSerialize(streamFor);
			this._hpPureDamage.DeSerialize(streamFor);
			this._hpPureArmor.DeSerialize(streamFor);
			this._hpLightDamage.DeSerialize(streamFor);
			this._hpLightArmor.DeSerialize(streamFor);
			this._hpHeavyDamage.DeSerialize(streamFor);
			this._hpHeavyArmor.DeSerialize(streamFor);
			this._hpRepairArmor.DeSerialize(streamFor);
			this._epMax.DeSerialize(streamFor);
			this._epRegen.DeSerialize(streamFor);
			this._fireRate.DeSerialize(streamFor);
			this._hpMaxPct.DeSerialize(streamFor, 2);
			this._hpRegenPct.DeSerialize(streamFor, 2);
			this._hpPartialRegenPct.DeSerialize(streamFor, 2);
			this._hpPureDamagePct.DeSerialize(streamFor, 2);
			this._hpPureArmorPct.DeSerialize(streamFor, 2);
			this._hpLightDamagePct.DeSerialize(streamFor, 2);
			this._hpLightArmorPct.DeSerialize(streamFor, 2);
			this._hpHeavyDamagePct.DeSerialize(streamFor, 2);
			this._hpHeavyArmorPct.DeSerialize(streamFor, 2);
			this._hpRepairArmorPct.DeSerialize(streamFor, 2);
			this._epMaxPct.DeSerialize(streamFor, 2);
			this._epRegenPct.DeSerialize(streamFor, 2);
			this._epPartialRegenPct.DeSerialize(streamFor, 2);
			this._fireRatePct.DeSerialize(streamFor, 2);
			this._accel.DeSerialize(streamFor);
			this._backwardAccel.DeSerialize(streamFor);
			this._brakeAccel.DeSerialize(streamFor);
			this._turningRadius.DeSerialize(streamFor);
			this._maxAngularPush.DeSerialize(streamFor);
			this._maxAngularPushPct.DeSerialize(streamFor);
			this._maxAngularSpeed.DeSerialize(streamFor);
			this._maxAngularSpeedPct.DeSerialize(streamFor);
			this._forcedAngularPush.DeSerialize(streamFor);
			this._gripExtraFwdAcceleration.DeSerialize(streamFor);
			this._gripExtraBackAcceleration.DeSerialize(streamFor);
			this._drag.DeSerialize(streamFor);
			this._driftDrag.DeSerialize(streamFor);
			this._lateralFriction.DeSerialize(streamFor);
			this._lateralFrictionPct.DeSerialize(streamFor);
			this._accelPct.DeSerialize(streamFor, 2);
			this._backwardAccelPct.DeSerialize(streamFor, 2);
			this._brakeAccelPct.DeSerialize(streamFor, 2);
			this._turningRadiusPct.DeSerialize(streamFor, 2);
			this._gripExtraFwdAccelerationPct.DeSerialize(streamFor, 2);
			this._gripExtraBackAccelerationPct.DeSerialize(streamFor, 2);
			this._dragPct.DeSerialize(streamFor, 2);
			this._driftDragPct.DeSerialize(streamFor);
			this._mass.DeSerialize(streamFor, 2);
			this._massPct.DeSerialize(streamFor, 2);
			this._pushForceModPct.DeSerialize(streamFor, 2);
			this._pushReceivedModPct.DeSerialize(streamFor, 2);
			this._sceneryBouncinessModPct.DeSerialize(streamFor, 2);
			this._cooldownReductionGadget0.DeSerialize(streamFor);
			this._cooldownReductionGadget1.DeSerialize(streamFor);
			this._cooldownReductionGadget2.DeSerialize(streamFor);
			this._cooldownReductionGadgetB.DeSerialize(streamFor);
			this._scrapBonus.DeSerialize(streamFor);
			this._crowdControlReduction.DeSerialize(streamFor);
			this._cooldownReductionGadget0Pct.DeSerialize(streamFor, 2);
			this._cooldownReductionGadget1Pct.DeSerialize(streamFor, 2);
			this._cooldownReductionGadget2Pct.DeSerialize(streamFor, 2);
			this._cooldownReductionGadgetBPct.DeSerialize(streamFor, 2);
			this._scrapBonusPct.DeSerialize(streamFor, 2);
			this._powerPct.DeSerialize(streamFor, 2);
			this.ForceInvincible = streamFor.ReadBool();
			this._currentStatus = (StatusKind)streamFor.ReadCompressedInt();
			if (this.OnStreamApplied != null)
			{
				this.OnStreamApplied();
			}
		}

		public override void OnSendToCache()
		{
			base.OnSendToCache();
			this.Clear();
			this._isDirty = false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CombatAttributes));

		public bool ForceInvincible;

		private SDeltaSerializableValue<float> _hpMax;

		private SDeltaSerializableValue<float> _hpMaxPct;

		private SDeltaSerializableValue<float> _hpRegen;

		private SDeltaSerializableValue<float> _hpRegenPct;

		private SDeltaSerializableValue<float> _hpPartialRegenPct;

		private SDeltaSerializableValue<float> _hpPureDamage;

		private SDeltaSerializableValue<float> _hpPureDamagePct;

		private SDeltaSerializableValue<float> _hpPureArmor;

		private SDeltaSerializableValue<float> _hpPureArmorPct;

		private SDeltaSerializableValue<float> _epMax;

		private SDeltaSerializableValue<float> _epMaxPct;

		private SDeltaSerializableValue<float> _epRegen;

		private SDeltaSerializableValue<float> _epRegenPct;

		private SDeltaSerializableValue<float> _epPartialRegenPct;

		private SDeltaSerializableValue<float> _accelPct;

		private SDeltaSerializableValue<float> _accel;

		private SDeltaSerializableValue<float> _backwardAccelPct;

		private SDeltaSerializableValue<float> _backwardAccel;

		private SDeltaSerializableValue<float> _brakeAccelPct;

		private SDeltaSerializableValue<float> _brakeAccel;

		private SDeltaSerializableValue<float> _hpLightDamage;

		private SDeltaSerializableValue<float> _hpLightDamagePct;

		private SDeltaSerializableValue<float> _hpLightArmor;

		private SDeltaSerializableValue<float> _hpLightArmorPct;

		private SDeltaSerializableValue<float> _hpHeavyDamage;

		private SDeltaSerializableValue<float> _hpHeavyDamagePct;

		private SDeltaSerializableValue<float> _hpHeavyArmor;

		private SDeltaSerializableValue<float> _hpHeavyArmorPct;

		private SDeltaSerializableValue<float> _hpRepairArmor;

		private SDeltaSerializableValue<float> _hpRepairArmorPct;

		private SDeltaSerializableValue<float> _fireRate;

		private SDeltaSerializableValue<float> _fireRatePct;

		private SDeltaSerializableValue<int> _scrapBonus;

		private SDeltaSerializableValue<float> _scrapBonusPct;

		private SDeltaSerializableValue<float> _cooldownReductionGadget0;

		private SDeltaSerializableValue<float> _cooldownReductionGadget1;

		private SDeltaSerializableValue<float> _cooldownReductionGadget2;

		private SDeltaSerializableValue<float> _cooldownReductionGadgetB;

		private SDeltaSerializableValue<float> _cooldownReductionGadget0Pct;

		private SDeltaSerializableValue<float> _cooldownReductionGadget1Pct;

		private SDeltaSerializableValue<float> _cooldownReductionGadget2Pct;

		private SDeltaSerializableValue<float> _cooldownReductionGadgetBPct;

		private SDeltaSerializableValue<float> _crowdControlReduction;

		private SDeltaSerializableValue<float> _mass;

		private SDeltaSerializableValue<float> _massPct;

		private SDeltaSerializableValue<float> _powerPct;

		private SDeltaSerializableValue<float> _pushForceModPct;

		private SDeltaSerializableValue<float> _pushReceivedModPct;

		private SDeltaSerializableValue<float> _sceneryBouncinessModPct;

		private SDeltaSerializableValue<float> _turningRadiusPct;

		private SDeltaSerializableValue<float> _turningRadius;

		private SDeltaSerializableValue<float> _maxAngularPush;

		private SDeltaSerializableValue<float> _maxAngularPushPct;

		private SDeltaSerializableValue<float> _forcedAngularPush;

		private SDeltaSerializableValue<float> _maxAngularSpeed;

		private SDeltaSerializableValue<float> _maxAngularSpeedPct;

		private SDeltaSerializableValue<float> _gripExtraFwdAcceleration;

		private SDeltaSerializableValue<float> _gripExtraFwdAccelerationPct;

		private SDeltaSerializableValue<float> _gripExtraBackAcceleration;

		private SDeltaSerializableValue<float> _gripExtraBackAccelerationPct;

		private SDeltaSerializableValue<float> _dragPct;

		private SDeltaSerializableValue<float> _drag;

		private SDeltaSerializableValue<float> _driftDragPct;

		private SDeltaSerializableValue<float> _driftDrag;

		private SDeltaSerializableValue<float> _lateralFriction;

		private SDeltaSerializableValue<float> _lateralFrictionPct;

		private StatusKind _currentStatus;

		private CombatObject _currentBanishCauserCombat;

		public Vector3 _immobilizedDirection;

		public CombatObject _combat;

		private bool _isDirty;

		private readonly Dictionary<string, ModifierInstance> _min = new Dictionary<string, ModifierInstance>(10);

		private readonly Dictionary<string, ModifierInstance> _max = new Dictionary<string, ModifierInstance>(10);

		private readonly List<ModifierInstance> _untaggedMods = new List<ModifierInstance>(10);

		private readonly List<string> _supressedTags = new List<string>(4);

		public delegate void StreamAppliedListener();
	}
}
