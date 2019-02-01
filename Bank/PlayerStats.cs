using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.UpdateStream;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Bank
{
	[Serializable]
	public class PlayerStats : StreamContent, IPlayerStats
	{
		public List<int> MissionsCompletedIndex
		{
			get
			{
				return this._missionsCompletedIndex;
			}
		}

		public int Kills
		{
			get
			{
				return this._kills;
			}
			set
			{
				this.SetAttributeAndNotifyStatsStream<SDeltaSerializableValue<int>>(ref this._kills, value);
			}
		}

		public int CreepKills
		{
			get
			{
				return this._creepKills;
			}
			set
			{
				this.SetAttributeAndNotifyStatsStream<SDeltaSerializableValue<int>>(ref this._creepKills, value);
			}
		}

		public int Deaths
		{
			get
			{
				return this._deaths;
			}
			set
			{
				if (this._deaths == value)
				{
					return;
				}
				this.UpdateDeaths(value);
				GameHubBehaviour.Hub.Stream.StatsStream.Changed(this);
			}
		}

		private void UpdateDeaths(int value)
		{
			this.Level = 1;
			this._deaths = value;
		}

		public int Assists
		{
			get
			{
				return this._assists;
			}
			set
			{
				this.SetAttributeAndNotifyStatsStream<SDeltaSerializableValue<int>>(ref this._assists, value);
			}
		}

		public int KillsAndAssists
		{
			get
			{
				return this.Kills + this.Assists;
			}
		}

		public int HighestKillingStreak
		{
			get
			{
				return this._highestKillingStreak;
			}
		}

		public int CurrentKillingStreak
		{
			get
			{
				return this._currentKillingStreak;
			}
			set
			{
				if (value > this._highestKillingStreak)
				{
					this._highestKillingStreak = value;
				}
				this._currentKillingStreak = value;
			}
		}

		public int HighestDeathStreak
		{
			get
			{
				return this._highestDeathStreak;
			}
		}

		public int CurrentDeathStreak
		{
			get
			{
				return this._currentDeathStreak;
			}
			set
			{
				if (value > this._highestDeathStreak)
				{
					this._highestDeathStreak = value;
				}
				this._currentDeathStreak = value;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event PlayerStats.MaxLevel ListenToMaxLevel;

		public int LevelCounter
		{
			get
			{
				return this._levelCounter;
			}
			set
			{
				this._levelCounter = value;
				this.UpdateLevel();
			}
		}

		public int Level
		{
			get
			{
				return this._level;
			}
			set
			{
				if (this._level == value)
				{
					return;
				}
				GameHubBehaviour.Hub.Stream.StatsStream.Changed(this);
				this.UpdateLevel(value);
			}
		}

		private void UpdateLevel(int value)
		{
			if (value > GameHubBehaviour.Hub.ScrapLevel.MaxLevel)
			{
				return;
			}
			if (value == 1)
			{
				this._levelCounter = 0;
			}
			if (value == GameHubBehaviour.Hub.ScrapLevel.MaxLevel && this.ListenToMaxLevel != null)
			{
				this.ListenToMaxLevel();
			}
			if (value > this._level && GameHubBehaviour.Hub.ScrapLevel.LevelUpModifiers != null && GameHubBehaviour.Hub.ScrapLevel.LevelUpModifiers.Length > 0)
			{
				ModifierData[] datas = ModifierData.CreateData(GameHubBehaviour.Hub.ScrapLevel.LevelUpModifiers);
				this.Combat.Controller.AddModifiers(datas, null, -1, false);
			}
			this.Combat.Data.UpdateLevel(value);
			this._level = value;
		}

		public int ScrapSpent
		{
			get
			{
				return this._scrapSpent;
			}
			set
			{
				this.SetAttributeAndNotifyStatsStream<SDeltaSerializableValue<int>>(ref this._scrapSpent, value);
			}
		}

		public float GetDamagePerMinuteDealt(IMatchStats matchStats)
		{
			return this._damageDealtToPlayers / (matchStats.GetMatchTimeSeconds() / 60f);
		}

		public float GetHealingPerMinuteProvided(IMatchStats matchStats)
		{
			return this._healingProvided / (matchStats.GetMatchTimeSeconds() / 60f);
		}

		public int TotalScrapCollected
		{
			get
			{
				return this._totalScrapCollected;
			}
			set
			{
				this._totalScrapCollected = value;
			}
		}

		public int ScrapCollected
		{
			get
			{
				return this._scrapCollected;
			}
			set
			{
				this._scrapCollected = value;
			}
		}

		public float DamageDealtToPlayers
		{
			get
			{
				return this._damageDealtToPlayers;
			}
			set
			{
				this._damageDealtToPlayers = value;
			}
		}

		public float DamageDealtToCreeps
		{
			get
			{
				return this._damageDealtToCreeps;
			}
			set
			{
				this._damageDealtToCreeps = value;
			}
		}

		public float DamageDealtToBuildings
		{
			get
			{
				return this._damageDealtToBuildings;
			}
			set
			{
				this._damageDealtToBuildings = value;
			}
		}

		public float DamageReceived
		{
			get
			{
				return this._damageReceived;
			}
			set
			{
				this._damageReceived = value;
			}
		}

		public float HealingProvided
		{
			get
			{
				return this._healingProvided;
			}
			set
			{
				this._healingProvided = value;
			}
		}

		public float HealingReceived
		{
			get
			{
				return this._healingReceived;
			}
			set
			{
				this._healingReceived = value;
			}
		}

		public int ScrapGainedFromKills
		{
			get
			{
				return this._scrapGainedFromKills;
			}
			set
			{
				this.SetAttributeAndNotifyStatsStream<int>(ref this._scrapGainedFromKills, value);
			}
		}

		public int ScrapLostFromKills
		{
			get
			{
				return this._scrapLostFromKills;
			}
			set
			{
				this.SetAttributeAndNotifyStatsStream<int>(ref this._scrapLostFromKills, value);
			}
		}

		public int ScrapGainedFromCreeps
		{
			get
			{
				return this._scrapGainedFromCreeps;
			}
			set
			{
				this.SetAttributeAndNotifyStatsStream<int>(ref this._scrapGainedFromCreeps, value);
			}
		}

		public int ScrapGainedFromTurrets
		{
			get
			{
				return this._scrapGainedFromTurrets;
			}
			set
			{
				this.SetAttributeAndNotifyStatsStream<int>(ref this._scrapGainedFromTurrets, value);
			}
		}

		public int ScrapGainedFromTime
		{
			get
			{
				return this._scrapGainedFromTime;
			}
			set
			{
				this.SetAttributeAndNotifyStatsStream<int>(ref this._scrapGainedFromTime, value);
			}
		}

		public TeamKind Team
		{
			get
			{
				return this.Combat.Team;
			}
		}

		public bool MatchWon { get; set; }

		public int NumberOfMedals { get; set; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action StreamDataReceivedObserver;

		public bool Disconnected
		{
			get
			{
				return this._disconnected;
			}
			set
			{
				this.SetAttributeAndNotifyStatsStream<bool>(ref this._disconnected, value);
			}
		}

		public void OnPlayerReconnected()
		{
			this.Disconnected = false;
			this._numberOfReconnects++;
		}

		public void OnPlayerDisconnected()
		{
			this.Disconnected = true;
			this._numberOfDisconnects++;
		}

		public int NumberOfDisconnects
		{
			get
			{
				return this._numberOfDisconnects;
			}
		}

		public int NumberOfReconnects
		{
			get
			{
				return this._numberOfReconnects;
			}
		}

		public long FirstScrapExpenseTimeMillis
		{
			get
			{
				return this._firstScrapExpenseTimeMillis;
			}
		}

		public int Scrap
		{
			get
			{
				return this._scrap + this._reliableScrap + this._timedScrap;
			}
		}

		public SDeltaSerializableValue<int> BombsDelivered { get; set; }

		public float TravelledDistance { get; set; }

		public SDeltaSerializableValue<float> BombPossessionTime { get; set; }

		public SDeltaSerializableValue<float> DebuffTime { get; set; }

		public int RoleCarrierKills { get; set; }

		public int RoleTacklerKills { get; set; }

		public int RoleSupportKills { get; set; }

		public int BombCarrierKills { get; set; }

		public int TimeBotControlled
		{
			get
			{
				return this.BotControlledChronometer.GetTime();
			}
		}

		public int Gadget0Count { get; set; }

		public int Gadget1Count { get; set; }

		public int Gadget2Count { get; set; }

		public int BombGadgetGrabberCount { get; set; }

		public int BoostGadgetCount { get; set; }

		public int BombGadgetPowerShotCount { get; set; }

		public float BombGadgetPowerShotHoldTime { get; set; }

		public int BombGadgetPowerShotScoreCount { get; set; }

		public int BombGadgetPowerShotPassCount { get; set; }

		public int BombGadgetPowerShotInterceptedCount { get; set; }

		public int ReverseCount { get; set; }

		public int BombLostDeathCount { get; set; }

		public int BombLostBlockerCount { get; set; }

		public int BombLostDropperCount { get; set; }

		public int BombLostGadgetCount { get; set; }

		public int BombTakenCount { get; set; }

		private void Start()
		{
			this.BotControlledChronometer = new TimeUtils.Chronometer(new Func<int>(GameHubBehaviour.Hub.GameTime.GetPlaybackTime));
			this._distanceUpdater = new TimedUpdater(1000, true, false);
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.ListenToBombDelivered;
				GameHubBehaviour.Hub.BombManager.ListenToBombDrop += this.ListenToBombDrop;
				GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged += this.OnBombCarrierChanged;
				this.Combat.CustomGadget0.ServerListenToGadgetUse += this.ListenToGadget0Use;
				this.Combat.CustomGadget1.ServerListenToGadgetUse += this.ListenToGadget1Use;
				this.Combat.CustomGadget2.ServerListenToGadgetUse += this.ListenToGadget2Use;
				this.Combat.BombGadget.ServerListenToGadgetUse += this.ListenToBombUse;
				this.Combat.BoostGadget.ServerListenToGadgetUse += this.ListenToBoostUse;
				CombatController.OnInstantModifierApplied += this.OnInstantModifierApplied;
				this.Combat.PlayerController.ServerListenToReverseUse += this.OnReverseUsed;
				this.Disconnected = (!this.Combat.Player.IsBot && !this.Combat.Player.Connected);
			}
		}

		private void SetAttributeAndNotifyStatsStream<T>(ref T attribute, T newValue) where T : struct, IEquatable<T>
		{
			if (attribute.Equals(newValue))
			{
				return;
			}
			attribute = newValue;
			GameHubBehaviour.Hub.Stream.StatsStream.Changed(this);
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.BombDelivery && GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned && GameHubBehaviour.Hub.BombManager.IsCarryingBomb(base.Id.ObjId))
			{
				this.BombPossessionTime += Time.deltaTime;
			}
			if (this._distanceUpdater.ShouldHalt())
			{
				return;
			}
			this.TravelledDistance += (this.Combat.Movement as CarMovement).SpeedZ;
		}

		private void ListenToGadget0Use()
		{
			this.Gadget0Count++;
		}

		private void ListenToGadget1Use()
		{
			this.Gadget1Count++;
		}

		private void ListenToGadget2Use()
		{
			this.Gadget2Count++;
		}

		private void ListenToBombUse()
		{
			this.BombGadgetGrabberCount++;
		}

		private void ListenToBoostUse()
		{
			this.BoostGadgetCount++;
		}

		private void OnReverseUsed()
		{
			this.ReverseCount++;
		}

		private void OnBombCarrierChanged(CombatObject carrier)
		{
			if (carrier != this.Combat)
			{
				return;
			}
			if (!GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this.Combat.Id.ObjId))
			{
				return;
			}
			this.BombTakenCount++;
		}

		private void ListenToBombDrop(BombInstance bombinstance, SpawnReason reason, int causer)
		{
			if (causer != this.Combat.Id.ObjId)
			{
				return;
			}
			if (reason != SpawnReason.TriggerDrop)
			{
				if (reason != SpawnReason.InputDrop)
				{
					if (reason != SpawnReason.Death)
					{
						if (reason == SpawnReason.BrokenLink)
						{
							this.BombLostBlockerCount++;
						}
					}
					else
					{
						this.BombLostDeathCount++;
					}
				}
				else
				{
					this.BombLostGadgetCount++;
				}
			}
			else
			{
				this.BombLostDropperCount++;
			}
		}

		private void ListenToBombDelivered(int causerid, TeamKind scoredTeam, Vector3 deliveryPosition)
		{
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.Replay)
			{
				return;
			}
			if (this.Combat.Id.ObjId != causerid)
			{
				return;
			}
			this.BombsDelivered++;
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.ListenToBombDelivered;
				GameHubBehaviour.Hub.BombManager.ListenToBombDrop -= this.ListenToBombDrop;
				GameHubBehaviour.Hub.BombManager.ListenToBombCarrierChanged -= this.OnBombCarrierChanged;
				CombatController.OnInstantModifierApplied -= this.OnInstantModifierApplied;
				this.Combat.CustomGadget0.ServerListenToGadgetUse -= this.ListenToGadget0Use;
				this.Combat.CustomGadget1.ServerListenToGadgetUse -= this.ListenToGadget1Use;
				this.Combat.CustomGadget2.ServerListenToGadgetUse -= this.ListenToGadget2Use;
				this.Combat.BombGadget.ServerListenToGadgetUse -= this.ListenToBombUse;
				this.Combat.BoostGadget.ServerListenToGadgetUse -= this.ListenToBoostUse;
				this.Combat.PlayerController.ServerListenToReverseUse -= this.OnReverseUsed;
			}
		}

		private void OnInstantModifierApplied(ModifierInstance mod, CombatObject causer, CombatObject target, float amount, int eventId)
		{
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreBoard.State.BombDelivery)
			{
				return;
			}
			CombatObject combatObject = (!mod.Info.LifeStealProvider) ? causer : mod.Info.LifeStealProvider;
			if (combatObject != null && combatObject.Id.ObjId == this.Combat.Id.ObjId)
			{
				if (mod.Info.Effect.IsHPDamage() && this.Combat.Id.ObjId != target.Id.ObjId)
				{
					if (amount < 0f)
					{
						PlayerStats.Log.WarnFormatStackTrace("Negative Damage! Ignoring value. ModifierInstance: {0} Amount: {1}", new object[]
						{
							mod.ToString(),
							amount
						});
					}
					else
					{
						this.DamageDealtToPlayers += amount;
					}
				}
				else if (mod.Info.Effect == EffectKind.HPRepair)
				{
					this.HealingProvided += amount;
				}
				return;
			}
			if (target.Id.ObjId == this.Combat.Id.ObjId)
			{
				if (mod.Info.Effect.IsHPDamage())
				{
					this.DamageReceived += amount;
				}
				else if (mod.Info.Effect == EffectKind.HPRepair && combatObject != null && combatObject.Id.ObjId != this.Combat.Id.ObjId)
				{
					this.HealingReceived += amount;
					return;
				}
			}
		}

		public void AddScrap(int amount, bool reliable, ScrapBank.ScrapReason reason)
		{
			GameHubBehaviour.Hub.Stream.StatsStream.Changed(this);
			if (reliable)
			{
				this._reliableScrap += amount;
			}
			else if (reason == ScrapBank.ScrapReason.time)
			{
				this._timedScrap += amount;
			}
			else
			{
				this._scrap += amount;
			}
			this.TotalScrapCollected += amount;
			switch (reason)
			{
			case ScrapBank.ScrapReason.kill:
				this.ScrapGainedFromKills += amount;
				break;
			case ScrapBank.ScrapReason.creep:
				this.ScrapGainedFromCreeps += amount;
				break;
			case ScrapBank.ScrapReason.time:
				this.ScrapGainedFromTime += amount;
				break;
			case ScrapBank.ScrapReason.turret:
				this.ScrapGainedFromTurrets += amount;
				break;
			}
		}

		public bool SpendScrap(int amount)
		{
			if (this.Scrap < amount)
			{
				return false;
			}
			this._timedScrap -= amount;
			if (this._timedScrap < 0)
			{
				this._scrap += this._timedScrap;
				this._timedScrap = 0;
				if (this._scrap < 0)
				{
					this._reliableScrap += this._scrap;
					this._scrap = 0;
				}
			}
			if (this._firstScrapExpenseTimeMillis == 0L)
			{
				this._firstScrapExpenseTimeMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
			GameHubBehaviour.Hub.Stream.StatsStream.Changed(this);
			return true;
		}

		public void RemoveScrap(int amount, ScrapBank.ScrapReason reason)
		{
			if (reason == ScrapBank.ScrapReason.kill)
			{
				this.ScrapLostFromKills += amount;
			}
			GameHubBehaviour.Hub.Stream.StatsStream.Changed(this);
			this._timedScrap -= amount;
			if (this._timedScrap < 0)
			{
				this._scrap += this._timedScrap;
				if (this._scrap < 0)
				{
					this._scrap = 0;
				}
				this._timedScrap = 0;
			}
		}

		private void UpdateLevel()
		{
			HeavyMetalMachines.Utils.Debug.Assert(GameHubBehaviour.Hub.ScrapLevel.KillsAndAssistsToLevelUp != 0, "ScrapLevels config:KillsAndAssistsToLevelUp can't be zero!!!", HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			this.Level = 1 + this.LevelCounter / GameHubBehaviour.Hub.ScrapLevel.KillsAndAssistsToLevelUp;
		}

		public override int GetStreamData(ref byte[] data, bool boForceSerialization)
		{
			Pocketverse.BitStream writeStream = StaticBitStream.GetWriteStream();
			this._StatsCache.UpdateCount();
			this._timedScrap.Serialize(writeStream, boForceSerialization);
			writeStream.WriteCompressedInt(this.TotalScrapCollected);
			if (boForceSerialization || this._StatsCache.SetValues(this._scrap, this._reliableScrap, this._level, this._scrapSpent, this.ScrapCollected, this._creepKills, this._kills, this._deaths, this._assists, this.BombsDelivered))
			{
				writeStream.WriteBool(true);
				this._scrap.Serialize(writeStream, boForceSerialization);
				this._reliableScrap.Serialize(writeStream, boForceSerialization);
				this._level.Serialize(writeStream, boForceSerialization);
				this._scrapSpent.Serialize(writeStream, boForceSerialization);
				this._scrapCollected.Serialize(writeStream, boForceSerialization);
				this._creepKills.Serialize(writeStream, boForceSerialization);
				this._kills.Serialize(writeStream, boForceSerialization);
				this._deaths.Serialize(writeStream, boForceSerialization);
				this._assists.Serialize(writeStream, boForceSerialization);
				this.BombsDelivered.Serialize(writeStream, boForceSerialization);
			}
			else
			{
				writeStream.WriteBool(false);
			}
			this._damageDealtToPlayers.Serialize(writeStream, boForceSerialization);
			this._damageReceived.Serialize(writeStream, boForceSerialization);
			this._healingProvided.Serialize(writeStream, boForceSerialization);
			this.BombPossessionTime.Serialize(writeStream, boForceSerialization);
			this.DebuffTime.Serialize(writeStream, boForceSerialization);
			writeStream.WriteBool(false);
			writeStream.WriteBool(this._disconnected);
			return writeStream.CopyToArray(data);
		}

		public override void ApplyStreamData(byte[] data)
		{
			Pocketverse.BitStream readStream = StaticBitStream.GetReadStream(data);
			this._timedScrap.DeSerialize(readStream);
			this.TotalScrapCollected = readStream.ReadCompressedInt();
			if (readStream.ReadBool())
			{
				this._scrap.DeSerialize(readStream);
				this._reliableScrap.DeSerialize(readStream);
				this._level.DeSerialize(readStream);
				this.UpdateLevel(this._level);
				this.ScrapSpent = this._scrapSpent.DeSerialize(readStream);
				this.ScrapCollected = this._scrapCollected.DeSerialize(readStream);
				this.CreepKills = this._creepKills.DeSerialize(readStream);
				this.Kills = this._kills.DeSerialize(readStream);
				this._deaths.DeSerialize(readStream);
				this.UpdateDeaths(this._deaths);
				this.Assists = this._assists.DeSerialize(readStream);
				this.BombsDelivered = this.BombsDelivered.DeSerialize(readStream);
			}
			this.DamageDealtToPlayers = this._damageDealtToPlayers.DeSerialize(readStream);
			this.DamageReceived = this._damageReceived.DeSerialize(readStream);
			this.HealingProvided = this._healingProvided.DeSerialize(readStream);
			this.BombPossessionTime = this.BombPossessionTime.DeSerialize(readStream);
			this.DebuffTime = this.DebuffTime.DeSerialize(readStream);
			this._disconnected = readStream.ReadBool();
			if (this.StreamDataReceivedObserver != null)
			{
				this.StreamDataReceivedObserver();
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PlayerStats));

		public CombatObject Combat;

		public ScrapLevels ScrapLevel;

		private List<int> _missionsCompletedIndex = new List<int>();

		private int _highestKillingStreak;

		private int _currentKillingStreak;

		public int LastKillingStreak;

		private int _highestDeathStreak;

		private int _currentDeathStreak;

		public Dictionary<int, int> CurrentPlayersDomination = new Dictionary<int, int>(5);

		private SDeltaSerializableValue<int> _scrap;

		private SDeltaSerializableValue<int> _reliableScrap;

		private SDeltaSerializableValue<int> _timedScrap;

		private int _totalScrapCollected;

		private SDeltaSerializableValue<int> _scrapCollected;

		private SDeltaSerializableValue<int> _kills;

		private SDeltaSerializableValue<int> _creepKills;

		private SDeltaSerializableValue<int> _deaths;

		private SDeltaSerializableValue<int> _assists;

		private SDeltaSerializableValue<int> _scrapSpent;

		private SDeltaSerializableValue<int> _level = 1;

		private int _levelCounter;

		private int _scrapGainedFromKills;

		private int _scrapLostFromKills;

		private int _scrapGainedFromCreeps;

		private int _scrapGainedFromTurrets;

		private int _scrapGainedFromTime;

		private bool _disconnected;

		private SDeltaSerializableValue<float> _damageDealtToPlayers;

		private float _damageDealtToBuildings;

		private SDeltaSerializableValue<float> _healingProvided;

		private float _healingReceived;

		private float _damageDealtToCreeps;

		private SDeltaSerializableValue<float> _damageReceived;

		private int _numberOfDisconnects;

		private int _numberOfReconnects;

		private long _firstScrapExpenseTimeMillis;

		private TimedUpdater _distanceUpdater;

		public TimeUtils.Chronometer BotControlledChronometer;

		private PlayerStats.StatsCache _StatsCache = new PlayerStats.StatsCache();

		public delegate void MaxLevel();

		public delegate void ScoreChangend(int playerThatScoredID);

		private class StatsCache
		{
			public void UpdateCount()
			{
				this.currentUpdateCount++;
			}

			public bool SetValues(int vScrap, int vReliableScrap, int vLevel, int vScrapSpent, int vScrapCollected, int vCreepKills, int vKills, int vDeaths, int vAssists, int vBombsDelivered)
			{
				if (vScrap != this.scrap || vReliableScrap != this.reliableScrap || vLevel != this.level || vScrapSpent != this.scrapSpent || vScrapCollected != this.scrapCollected || vCreepKills != this.creepKills || vKills != this.kills || vDeaths != this.deaths || vAssists != this.assists || vBombsDelivered != this.bombsDelivered || this.currentUpdateCount > this.fullUpdateFrequency)
				{
					this.scrap = vScrap;
					this.reliableScrap = vReliableScrap;
					this.level = vLevel;
					this.scrapSpent = vScrapSpent;
					this.scrapCollected = vScrapCollected;
					this.creepKills = vCreepKills;
					this.kills = vKills;
					this.deaths = vDeaths;
					this.assists = vAssists;
					this.bombsDelivered = vBombsDelivered;
					this.currentUpdateCount = 0;
					return true;
				}
				return false;
			}

			private int scrap;

			private int reliableScrap;

			private int level;

			private int scrapSpent;

			private int scrapCollected;

			private int creepKills;

			private int kills;

			private int deaths;

			private int assists;

			private int bombsDelivered;

			private int fullUpdateFrequency = 30;

			private int currentUpdateCount;
		}
	}
}
