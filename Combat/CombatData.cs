using System;
using System.Diagnostics;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Playback.Snapshot;
using HeavyMetalMachines.UpdateStream;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class CombatData : StreamContent, IObjectSpawnListener, ICombatDataSerialData, IBaseStreamSerialData<ICombatDataSerialData>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatData.ChangeListener OnHPChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatData.ChangeListener OnHPTempChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatData.ChangeListener OnEPChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatData.LevelChangeListener OnLevelChanged;

		public float HP
		{
			get
			{
				return this._hp;
			}
			set
			{
				if (this._hp != value)
				{
					GameHubBehaviour.Hub.Stream.CombatDataStream.Changed(this);
					this._hp = value;
					if (this.OnHPChanged != null)
					{
						this.OnHPChanged(value);
					}
				}
				this._hp = value;
			}
		}

		public float HPTemp
		{
			get
			{
				return this._hpTemp;
			}
			private set
			{
				if (this._hpTemp != value)
				{
					GameHubBehaviour.Hub.Stream.CombatDataStream.Changed(this);
					this._hpTemp = value;
					if (this.OnHPTempChanged != null)
					{
						this.OnHPTempChanged(value);
					}
				}
				this._hpTemp = value;
			}
		}

		public void SetHpTemp(float value)
		{
			if (value > this.HPTemp && this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.HpTempBlocked))
			{
				return;
			}
			this.HPTemp = value;
		}

		public void SetHPTempDelay(float seconds)
		{
			this._tempHPDelay = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + (long)(seconds * 1000f);
		}

		public float EP
		{
			get
			{
				return this._ep;
			}
			set
			{
				if (this._ep != value)
				{
					if (GameHubBehaviour.Hub)
					{
						GameHubBehaviour.Hub.Stream.CombatDataStream.Changed(this);
					}
					this._ep = value;
					if (this.OnEPChanged != null)
					{
						this.OnEPChanged(value);
					}
				}
				this._ep = value;
			}
		}

		public void UpdateLevel(int level)
		{
			level--;
			this._levelHPMax = (float)level * this.Info.HPMaxPercentageBonusPerLevel * (float)this.Info.HPMax;
			this._levelEPMax = (float)level * this.Info.EPMaxPercentageBonusPerLevel * (float)this.Info.EPMax;
			this._levelHPRegen = (float)level * this.Info.HPRegenPercentageBonusPerLevel * this.Info.HPRegen;
			this._levelEPRegen = (float)level * this.Info.EPRegenPercentageBonusPerLevel * this.Info.EPRegen;
			this._levelPowerPct = (float)level * this.Info.PowerBonusPerLevel;
			this._levelHPPureArmor = (float)level * this.Info.HPPureArmorPerLevel;
			this._levelHPPureArmorPct = (float)level * this.Info.HPPureArmorPctPerLevel;
			this._levelHPLightArmor = (float)level * this.Info.HPLightArmorPerLevel;
			this._levelHPLightArmorPct = (float)level * this.Info.HPLightArmorPctPerLevel;
			this._levelHPHeavyArmor = (float)level * this.Info.HPHeavyArmorPerLevel;
			this._levelHPHeavyArmorPct = (float)level * this.Info.HPHeavyArmorPctPerLevel;
			this._levelHPRepairArmor = (float)level * this.Info.HPRepairArmorPerLevel;
			this._levelHPRepairArmorPct = (float)level * this.Info.HPRepairArmorPctPerLevel;
			if (this.OnLevelChanged != null)
			{
				this.OnLevelChanged();
			}
		}

		public int HPMax
		{
			get
			{
				return (int)Mathf.Max(1f, (float)this.Info.HPMax + this.Combat.Attributes.HPMax + this._levelHPMax + (float)this.Info.HPMax * this.Combat.Attributes.HPMaxPct);
			}
		}

		public int EPMax
		{
			get
			{
				return (int)Mathf.Max(1f, (float)this.Info.EPMax + this.Combat.Attributes.EPMax + this._levelEPMax + (float)this.Info.EPMax * this.Combat.Attributes.EPMaxPct);
			}
		}

		public float HPRegen
		{
			get
			{
				return this.Info.HPRegen + this.Combat.Attributes.HPRegen + this._levelHPRegen + this.Info.HPRegen * this.Combat.Attributes.HPRegenPct + (float)this.Info.HPMax * this.Combat.Attributes.HPPartialRegenPct;
			}
		}

		public float EPRegen
		{
			get
			{
				return this.Info.EPRegen + this.Combat.Attributes.EPRegen + this._levelEPRegen + this.Info.EPRegen * this.Combat.Attributes.EPRegenPct + (float)this.Info.EPMax * this.Combat.Attributes.EPPartialRegenPct;
			}
		}

		public float PowerPct
		{
			get
			{
				return this.Combat.Attributes.PowerPct + this._levelPowerPct;
			}
		}

		public int HPPureArmor
		{
			get
			{
				return (int)(this.Info.HPPureArmor + this.Combat.Attributes.HPPureArmor + this._levelHPPureArmor);
			}
		}

		public float HPPureArmorPct
		{
			get
			{
				return Mathf.Max(-1f, this.Info.HPPureArmorPct + this.Combat.Attributes.HPPureArmorPct + this._levelHPPureArmorPct);
			}
		}

		public int HPLightArmor
		{
			get
			{
				return (int)(this.Info.HPLightArmor + this.Combat.Attributes.HPLightArmor + this._levelHPLightArmor);
			}
		}

		public float HPLightArmorPct
		{
			get
			{
				return Mathf.Max(-1f, this.Info.HPLightArmorPct + this.Combat.Attributes.HPLightArmorPct + this._levelHPLightArmorPct);
			}
		}

		public int HPLightArmorFinal
		{
			get
			{
				return (int)((float)this.HPLightArmor + (float)this.HPLightArmor * this.HPLightArmorPct);
			}
		}

		public int HPHeavyArmor
		{
			get
			{
				return (int)(this.Info.HPHeavyArmor + this.Combat.Attributes.HPHeavyArmor + this._levelHPHeavyArmor);
			}
		}

		public float HPHeavyArmorPct
		{
			get
			{
				return Mathf.Max(-1f, this.Info.HPHeavyArmorPct + this.Combat.Attributes.HPHeavyArmorPct + this._levelHPHeavyArmorPct);
			}
		}

		public int HPHeavyArmorFinal
		{
			get
			{
				return (int)((float)this.HPHeavyArmor + (float)this.HPHeavyArmor * this.HPHeavyArmorPct);
			}
		}

		public int HPRepaisArmor
		{
			get
			{
				return (int)(this.Info.HPRepairArmor + this.Combat.Attributes.HPRepairArmor + this._levelHPRepairArmor);
			}
		}

		public float HPRepairArmorPct
		{
			get
			{
				return Mathf.Max(-1f, this.Info.HPRepairArmorPct + this.Combat.Attributes.HPRepairArmorPct + this._levelHPRepairArmorPct);
			}
		}

		public int HPRepairArmorFinal
		{
			get
			{
				return (int)((float)this.HPRepaisArmor + (float)this.HPRepaisArmor * this.HPRepairArmorPct);
			}
		}

		public float CurrentHPPercent
		{
			get
			{
				return (this.HPMax <= 0) ? this.HP : (this.HP / (float)this.HPMax);
			}
		}

		public GadgetData GadgetData
		{
			get
			{
				if (this._gadgetData == null)
				{
					this._gadgetData = base.gameObject.GetComponent<GadgetData>();
				}
				return this._gadgetData;
			}
		}

		public bool IsFullHP()
		{
			return this.CurrentHPPercent >= 1f;
		}

		public bool IsFullEP()
		{
			return (float)this.EPMax == this.EP;
		}

		public bool IsAlive()
		{
			if (this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Dead))
			{
				return this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.Indestructible);
			}
			return this.HP > 0f;
		}

		public bool CanSpendEP(float epCost)
		{
			return this.EP >= epCost;
		}

		public void SetInfo(CombatInfo info)
		{
			this.Info = info;
			this._levelHPMax = 0f;
			this._levelHPRegen = 0f;
			this._levelEPMax = 0f;
			this._levelEPRegen = 0f;
			this._levelPowerPct = 0f;
			this._levelHPPureArmor = 0f;
			this._levelHPPureArmorPct = 0f;
			this._levelHPLightArmor = 0f;
			this._levelHPLightArmorPct = 0f;
			this._levelHPHeavyArmor = 0f;
			this._levelHPHeavyArmorPct = 0f;
		}

		public void ResetHP()
		{
			this.HP = (float)this.HPMax;
		}

		public void ResetEP()
		{
			this.EP = 0f;
		}

		private void Awake()
		{
			if (!this.Combat)
			{
				this.Combat = base.GetComponent<CombatObject>();
			}
			this._regenUpdater = new TimedUpdater
			{
				PeriodMillis = 50
			};
			this.ResetHP();
			this.ResetEP();
		}

		public void OnObjectUnspawned(UnspawnEvent evt)
		{
			this.HP = 0f;
		}

		public void OnObjectSpawned(SpawnEvent msg)
		{
			this.ResetHP();
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (!this.IsAlive() || GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.BombDelivery)
			{
				this._lastTimeUpdate = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
				this._regenUpdater.Reset();
				return;
			}
			if (this._regenUpdater.ShouldHalt())
			{
				return;
			}
			float num = (float)(GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._lastTimeUpdate) * HudUtils.MillisToSeconds;
			this._lastTimeUpdate = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			int hpmax = this.HPMax;
			float num2 = this.HPRegen - this.HPRegen * ((float)this.Combat.Data.HPRepairArmorFinal * this.Combat.Data.Info.ArmorModifier);
			if (this.HP != (float)hpmax && num2 != 0f)
			{
				float num3 = num2 * num;
				if (!this.Combat.Attributes.CurrentStatus.HasFlag(StatusKind.HpUnhealable))
				{
					this.HP += num3;
				}
			}
			if (this.HP > (float)hpmax)
			{
				this.HP = (float)hpmax;
			}
			if (this.HPTemp > 0f && this._tempHPDelay < (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime())
			{
				float num4 = this.Info.HPTempDegradation + this.HPTemp * this.Info.HPTempDegradationPct;
				this.HPTemp -= num4 * num;
			}
			if (this.HPTemp < 0f)
			{
				this.HPTemp = 0f;
			}
			int epmax = this.EPMax;
			float epregen = this.EPRegen;
			if (this.EP != (float)epmax && epregen != 0f)
			{
				float num5 = epregen * num;
				this.EP += num5;
				if (this.EP > (float)epmax)
				{
					this.EP = (float)epmax;
				}
			}
		}

		public override int GetStreamData(ref byte[] data, bool boForceSerialization)
		{
			BitStream stream = base.GetStream();
			stream.WriteCompressedFloat(this.HP);
			stream.WriteCompressedFloat(this.HPTemp);
			stream.WriteCompressedFloat(this.EP);
			return stream.CopyToArray(data);
		}

		public override void ApplyStreamData(byte[] data)
		{
			BitStream streamFor = base.GetStreamFor(data);
			this.HP = streamFor.ReadCompressedFloat();
			this.HPTemp = streamFor.ReadCompressedFloat();
			this.EP = streamFor.ReadCompressedFloat();
		}

		public void Apply(ICombatDataSerialData other)
		{
			this.HP = other.HP;
			this.HPTemp = other.HPTemp;
			this.EP = other.EP;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CombatData));

		private long _tempHPDelay;

		private float _levelHPMax;

		private float _levelEPMax;

		private float _levelHPRegen;

		private float _levelEPRegen;

		private float _levelPowerPct;

		private float _levelHPPureArmor;

		private float _levelHPPureArmorPct;

		private float _levelHPLightArmor;

		private float _levelHPLightArmorPct;

		private float _levelHPHeavyArmor;

		private float _levelHPHeavyArmorPct;

		private float _levelHPRepairArmor;

		private float _levelHPRepairArmorPct;

		private GadgetData _gadgetData;

		public CombatObject Combat;

		public CombatInfo Info;

		private const int RegenFreq = 50;

		private TimedUpdater _regenUpdater;

		private float _hp;

		private float _hpTemp;

		private float _ep;

		private int _lastTimeUpdate;

		public delegate void ChangeListener(float val);

		public delegate void LevelChangeListener();
	}
}
