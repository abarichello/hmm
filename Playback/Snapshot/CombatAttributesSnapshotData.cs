using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class CombatAttributesSnapshotData : ICombatAttributesSerialData, ISnapshotStreamContent, IBaseStreamSerialData<ICombatAttributesSerialData>
	{
		public CombatAttributesSnapshotData()
		{
		}

		public CombatAttributesSnapshotData(CombatAttributesSnapshotData other)
		{
			this.Version = other.Version;
			this.Apply(other);
		}

		public float HPMax
		{
			get
			{
				return this._hpMax;
			}
		}

		public float HPMaxPct
		{
			get
			{
				return this._hpMaxPct;
			}
		}

		public float HPRegen
		{
			get
			{
				return this._hpRegen;
			}
		}

		public float HPRegenPct
		{
			get
			{
				return this._hpRegenPct;
			}
		}

		public float HPPartialRegenPct
		{
			get
			{
				return this._hpPartialRegenPct;
			}
		}

		public float HPPureDamage
		{
			get
			{
				return this._hpPureDamage;
			}
		}

		public float HPPureDamagePct
		{
			get
			{
				return this._hpPureDamagePct;
			}
		}

		public float HPPureArmor
		{
			get
			{
				return this._hpPureArmor;
			}
		}

		public float HPPureArmorPct
		{
			get
			{
				return this._hpPureArmorPct;
			}
		}

		public float EPMax
		{
			get
			{
				return this._epMax;
			}
		}

		public float EPMaxPct
		{
			get
			{
				return this._epMaxPct;
			}
		}

		public float EPRegen
		{
			get
			{
				return this._epRegen;
			}
		}

		public float EPRegenPct
		{
			get
			{
				return this._epRegenPct;
			}
		}

		public float EPPartialRegenPct
		{
			get
			{
				return this._epPartialRegenPct;
			}
		}

		public float AccelerationModPct
		{
			get
			{
				return this._accelerationModPct;
			}
		}

		public float AccelerationMod
		{
			get
			{
				return this._accelerationMod;
			}
		}

		public float BackAccelModPct
		{
			get
			{
				return this._backAccelModPct;
			}
		}

		public float BackAccelMod
		{
			get
			{
				return this._backAccelMod;
			}
		}

		public float BrakeAccelModPct
		{
			get
			{
				return this._brakeAccelModPct;
			}
		}

		public float BrakeAccelMod
		{
			get
			{
				return this._brakeAccelMod;
			}
		}

		public float GripExtraFwdAccelModPct
		{
			get
			{
				return this._gripExtraFwdAccelModPct;
			}
		}

		public float GripExtraFwdAccelMod
		{
			get
			{
				return this._gripExtraFwdAccelMod;
			}
		}

		public float GripExtraBackAccelModPct
		{
			get
			{
				return this._gripExtraBackAccelModPct;
			}
		}

		public float GripExtraBackAccelMod
		{
			get
			{
				return this._gripExtraBackAccelMod;
			}
		}

		public float DragMod
		{
			get
			{
				return this._dragMod;
			}
		}

		public float DragModPct
		{
			get
			{
				return this._dragModPct;
			}
		}

		public float DriftDragMod
		{
			get
			{
				return this._driftDragMod;
			}
		}

		public float DriftDragModPct
		{
			get
			{
				return this._driftDragModPct;
			}
		}

		public float LateralFriction
		{
			get
			{
				return this._lateralFriction;
			}
		}

		public float LateralFrictionPct
		{
			get
			{
				return this._lateralFrictionPct;
			}
		}

		public float HPLightDamage
		{
			get
			{
				return this._hpLightDamage;
			}
		}

		public float HPLightDamagePct
		{
			get
			{
				return this._hpLightDamagePct;
			}
		}

		public float HPLightArmor
		{
			get
			{
				return this._hpLightArmor;
			}
		}

		public float HPLightArmorPct
		{
			get
			{
				return this._hpLightArmorPct;
			}
		}

		public float HPHeavyDamage
		{
			get
			{
				return this._hpHeavyDamage;
			}
		}

		public float HPHeavyDamagePct
		{
			get
			{
				return this._hpHeavyDamagePct;
			}
		}

		public float HPHeavyArmor
		{
			get
			{
				return this._hpHeavyArmor;
			}
		}

		public float HPHeavyArmorPct
		{
			get
			{
				return this._hpHeavyArmorPct;
			}
		}

		public float HPRepairArmor
		{
			get
			{
				return this._hpRepairArmor;
			}
		}

		public float HPRepairArmorPct
		{
			get
			{
				return this._hpRepairArmorPct;
			}
		}

		public float FireRate
		{
			get
			{
				return this._fireRate;
			}
		}

		public float FireRatePct
		{
			get
			{
				return this._fireRatePct;
			}
		}

		public int ScrapBonus
		{
			get
			{
				return this._scrapBonus;
			}
		}

		public float ScrapBonusPct
		{
			get
			{
				return this._scrapBonusPct;
			}
		}

		public float PowerPct
		{
			get
			{
				return this._powerPct;
			}
		}

		public float Mass
		{
			get
			{
				return this._mass;
			}
		}

		public float MassPct
		{
			get
			{
				return this._massPct;
			}
		}

		public float PushForcePct
		{
			get
			{
				return this._pushForcePct;
			}
		}

		public float PushReceivedPct
		{
			get
			{
				return this._pushReceivedPct;
			}
		}

		public float SceneryBouncinessPct
		{
			get
			{
				return this._sceneryBouncinessPct;
			}
		}

		public float TurningRadiusPct
		{
			get
			{
				return this._turningRadiusPct;
			}
		}

		public float TurningRadius
		{
			get
			{
				return this._turningRadius;
			}
		}

		public float MaxAngularPushPct
		{
			get
			{
				return this._maxAngularPushPct;
			}
		}

		public float MaxAngularPush
		{
			get
			{
				return this._maxAngularPush;
			}
		}

		public float ForcedAngularPush
		{
			get
			{
				return this._forcedAngularPush;
			}
		}

		public float MaxAngularSpeed
		{
			get
			{
				return this._maxAngularSpeed;
			}
		}

		public float MaxAngularSpeedPct
		{
			get
			{
				return this._maxAngularSpeedPct;
			}
		}

		public float CooldownReductionGadget0
		{
			get
			{
				return this._cooldownReductionGadget0;
			}
		}

		public float CooldownReductionGadget1
		{
			get
			{
				return this._cooldownReductionGadget1;
			}
		}

		public float CooldownReductionGadget2
		{
			get
			{
				return this._cooldownReductionGadget2;
			}
		}

		public float CooldownReductionGadgetB
		{
			get
			{
				return this._cooldownReductionGadgetB;
			}
		}

		public float CooldownReductionGadget0Pct
		{
			get
			{
				return this._cooldownReductionGadget0Pct;
			}
		}

		public float CooldownReductionGadget1Pct
		{
			get
			{
				return this._cooldownReductionGadget1Pct;
			}
		}

		public float CooldownReductionGadget2Pct
		{
			get
			{
				return this._cooldownReductionGadget2Pct;
			}
		}

		public float CooldownReductionGadgetBPct
		{
			get
			{
				return this._cooldownReductionGadgetBPct;
			}
		}

		public float CrowdControlReduction
		{
			get
			{
				return this._crowdControlReduction;
			}
		}

		public StatusKind CurrentStatus
		{
			get
			{
				return this._currentStatus;
			}
		}

		public bool ForceInvincible
		{
			get
			{
				return this._forceInvincible;
			}
		}

		public HashSet<int> Status
		{
			get
			{
				return this._status;
			}
		}

		public short Version { get; set; }

		public void Apply(ICombatAttributesSerialData other)
		{
			this._hpMax = other.HPMax;
			this._hpMaxPct = other.HPMaxPct;
			this._hpRegen = other.HPRegen;
			this._hpRegenPct = other.HPRegenPct;
			this._hpPartialRegenPct = other.HPPartialRegenPct;
			this._hpPureDamage = other.HPPureDamage;
			this._hpPureDamagePct = other.HPPureDamagePct;
			this._hpPureArmor = other.HPPureArmor;
			this._hpPureArmorPct = other.HPPureArmorPct;
			this._epMax = other.EPMax;
			this._epMaxPct = other.EPMaxPct;
			this._epRegen = other.EPRegen;
			this._epRegenPct = other.EPRegenPct;
			this._epPartialRegenPct = other.EPPartialRegenPct;
			this._accelerationModPct = other.AccelerationModPct;
			this._accelerationMod = other.AccelerationMod;
			this._backAccelModPct = other.BackAccelModPct;
			this._backAccelMod = other.BackAccelMod;
			this._brakeAccelModPct = other.BrakeAccelModPct;
			this._brakeAccelMod = other.BrakeAccelMod;
			this._gripExtraFwdAccelModPct = other.GripExtraFwdAccelModPct;
			this._gripExtraFwdAccelMod = other.GripExtraFwdAccelMod;
			this._gripExtraBackAccelModPct = other.GripExtraBackAccelModPct;
			this._gripExtraBackAccelMod = other.GripExtraBackAccelMod;
			this._dragMod = other.DragMod;
			this._dragModPct = other.DragModPct;
			this._driftDragMod = other.DriftDragMod;
			this._driftDragModPct = other.DriftDragModPct;
			this._lateralFriction = other.LateralFriction;
			this._lateralFrictionPct = other.LateralFrictionPct;
			this._hpLightDamage = other.HPLightDamage;
			this._hpLightDamagePct = other.HPLightDamagePct;
			this._hpLightArmor = other.HPLightArmor;
			this._hpLightArmorPct = other.HPLightArmorPct;
			this._hpHeavyDamage = other.HPHeavyDamage;
			this._hpHeavyDamagePct = other.HPHeavyDamagePct;
			this._hpHeavyArmor = other.HPHeavyArmor;
			this._hpHeavyArmorPct = other.HPHeavyArmorPct;
			this._hpRepairArmor = other.HPRepairArmor;
			this._hpRepairArmorPct = other.HPRepairArmorPct;
			this._fireRate = other.FireRate;
			this._fireRatePct = other.FireRatePct;
			this._scrapBonus = other.ScrapBonus;
			this._scrapBonusPct = other.ScrapBonusPct;
			this._powerPct = other.PowerPct;
			this._mass = other.Mass;
			this._massPct = other.MassPct;
			this._pushForcePct = other.PushForcePct;
			this._pushReceivedPct = other.PushReceivedPct;
			this._sceneryBouncinessPct = other.SceneryBouncinessPct;
			this._turningRadiusPct = other.TurningRadiusPct;
			this._turningRadius = other.TurningRadius;
			this._maxAngularPushPct = other.MaxAngularPushPct;
			this._maxAngularPush = other.MaxAngularPush;
			this._forcedAngularPush = other.ForcedAngularPush;
			this._maxAngularSpeed = other.MaxAngularSpeed;
			this._maxAngularSpeedPct = other.MaxAngularSpeedPct;
			this._cooldownReductionGadget0 = other.CooldownReductionGadget0;
			this._cooldownReductionGadget1 = other.CooldownReductionGadget1;
			this._cooldownReductionGadget2 = other.CooldownReductionGadget2;
			this._cooldownReductionGadgetB = other.CooldownReductionGadgetB;
			this._cooldownReductionGadget0Pct = other.CooldownReductionGadget0Pct;
			this._cooldownReductionGadget1Pct = other.CooldownReductionGadget1Pct;
			this._cooldownReductionGadget2Pct = other.CooldownReductionGadget2Pct;
			this._cooldownReductionGadgetBPct = other.CooldownReductionGadgetBPct;
			this._crowdControlReduction = other.CrowdControlReduction;
			this._currentStatus = other.CurrentStatus;
			this._forceInvincible = other.ForceInvincible;
			this._status.Clear();
			foreach (int item in other.Status)
			{
				this._status.Add(item);
			}
		}

		public void ApplyStreamData(byte[] data)
		{
			BitStream readStream = StaticBitStream.GetReadStream(data);
			DeltaSerializableValueReader.ReadFloatField(ref this._hpMax, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._hpRegen, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._hpPureDamage, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._hpPureArmor, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._hpLightDamage, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._hpLightArmor, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._hpHeavyDamage, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._hpHeavyArmor, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._hpRepairArmor, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._epMax, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._epRegen, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._fireRate, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._hpMaxPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._hpRegenPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._hpPartialRegenPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._hpPureDamagePct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._hpPureArmorPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._hpLightDamagePct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._hpLightArmorPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._hpHeavyDamagePct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._hpHeavyArmorPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._hpRepairArmorPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._epMaxPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._epRegenPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._epPartialRegenPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._fireRatePct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._accelerationMod, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._backAccelMod, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._brakeAccelMod, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._turningRadius, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._maxAngularPush, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._maxAngularPushPct, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._maxAngularSpeed, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._maxAngularSpeedPct, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._forcedAngularPush, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._gripExtraFwdAccelMod, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._gripExtraBackAccelMod, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._dragMod, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._driftDragMod, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._lateralFriction, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._lateralFrictionPct, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._accelerationModPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._backAccelModPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._brakeAccelModPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._turningRadiusPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._gripExtraFwdAccelModPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._gripExtraBackAccelModPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._dragModPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._driftDragModPct, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._mass, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._massPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._pushForcePct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._pushReceivedPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._sceneryBouncinessPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._cooldownReductionGadget0, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._cooldownReductionGadget1, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._cooldownReductionGadget2, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._cooldownReductionGadgetB, readStream);
			DeltaSerializableValueReader.ReadIntField(ref this._scrapBonus, readStream);
			DeltaSerializableValueReader.ReadFloatField(ref this._crowdControlReduction, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._cooldownReductionGadget0Pct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._cooldownReductionGadget1Pct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._cooldownReductionGadget2Pct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._cooldownReductionGadgetBPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._scrapBonusPct, 2, readStream);
			DeltaSerializableValueReader.ReadFloatPrecisionField(ref this._powerPct, 2, readStream);
			this._forceInvincible = readStream.ReadBool();
			this._currentStatus = (StatusKind)readStream.ReadCompressedInt();
			this._status.Clear();
			int[] array = readStream.ReadIntArray();
			for (int i = 0; i < array.Length; i++)
			{
				this._status.Add(array[i]);
			}
		}

		private float _hpMax;

		private float _hpMaxPct;

		private float _hpRegen;

		private float _hpRegenPct;

		private float _hpPartialRegenPct;

		private float _hpPureDamage;

		private float _hpPureDamagePct;

		private float _hpPureArmor;

		private float _hpPureArmorPct;

		private float _epMax;

		private float _epMaxPct;

		private float _epRegen;

		private float _epRegenPct;

		private float _epPartialRegenPct;

		private float _accelerationModPct;

		private float _accelerationMod;

		private float _backAccelModPct;

		private float _backAccelMod;

		private float _brakeAccelModPct;

		private float _brakeAccelMod;

		private float _gripExtraFwdAccelModPct;

		private float _gripExtraFwdAccelMod;

		private float _gripExtraBackAccelModPct;

		private float _gripExtraBackAccelMod;

		private float _dragMod;

		private float _dragModPct;

		private float _driftDragMod;

		private float _driftDragModPct;

		private bool _forceInvincible;

		private StatusKind _currentStatus;

		private float _crowdControlReduction;

		private float _cooldownReductionGadgetBPct;

		private float _cooldownReductionGadget2Pct;

		private float _cooldownReductionGadget1Pct;

		private float _cooldownReductionGadget0Pct;

		private float _cooldownReductionGadgetB;

		private float _cooldownReductionGadget2;

		private float _cooldownReductionGadget1;

		private float _cooldownReductionGadget0;

		private float _maxAngularSpeedPct;

		private float _maxAngularSpeed;

		private float _forcedAngularPush;

		private float _maxAngularPush;

		private float _maxAngularPushPct;

		private float _turningRadius;

		private float _turningRadiusPct;

		private float _sceneryBouncinessPct;

		private float _pushReceivedPct;

		private float _pushForcePct;

		private float _massPct;

		private float _mass;

		private float _powerPct;

		private float _scrapBonusPct;

		private int _scrapBonus;

		private float _fireRatePct;

		private float _fireRate;

		private float _hpRepairArmorPct;

		private float _hpRepairArmor;

		private float _hpHeavyArmorPct;

		private float _hpHeavyArmor;

		private float _hpHeavyDamagePct;

		private float _hpHeavyDamage;

		private float _hpLightArmorPct;

		private float _hpLightArmor;

		private float _hpLightDamagePct;

		private float _hpLightDamage;

		private float _lateralFrictionPct;

		private float _lateralFriction;

		private readonly HashSet<int> _status = new HashSet<int>();
	}
}
