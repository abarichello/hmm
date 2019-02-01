using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish.Logs;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Combat
{
	[RemoteClass]
	public class BombManager : GameHubBehaviour, ICleanupListener, IBitComponent
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<Identifiable> ServerOnBombCarrierIdentifiableChanged;

		public BombScoreController ScoreController
		{
			get
			{
				BombScoreController result;
				if ((result = this._scoreController) == null)
				{
					result = (this._scoreController = new BombScoreController(this));
				}
				return result;
			}
		}

		public BombGridController GridController
		{
			get
			{
				BombGridController result;
				if ((result = this._gridController) == null)
				{
					result = (this._gridController = new BombGridController(this));
				}
				return result;
			}
		}

		public int Round
		{
			get
			{
				return this.ScoreBoard.Round;
			}
		}

		public BombScoreBoard.State CurrentBombGameState
		{
			get
			{
				return this.ScoreBoard.CurrentState;
			}
		}

		public bool AnyTeamHasScored
		{
			get
			{
				return this.ScoreBoard.BombScoreBlue + this.ScoreBoard.BombScoreRed > 0;
			}
		}

		public bool IsGameFinished()
		{
			return this.Rules.BombScoreTarget == this.ScoreBoard.BombScoreBlue || this.Rules.BombScoreTarget == this.ScoreBoard.BombScoreRed;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BombManager.BombSpawnTrigger ListenToClientBombCreation;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BombManager.BombDroppedListener ClientListenToBombDrop;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BombManager.BombCarrierChanged ListenToBombCarrierChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BombManager.BombDroppedListener ListenToBombDrop;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BombManager.BombDelivery ListenToBombDelivery;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<PickupRemoveEvent> ListenToBombUnspawn;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<BombScoreBoard.State> ListenToPhaseChange;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ListenToMatchUpdate;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int> ListenToScoreFeedback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<TeamKind> ListenToBombAlmostDeliveredTriggerEnter;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<TeamKind> ListenToBombLastCurveTriggerEnter;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<TeamKind> ListenToBombFirstCurveTriggerEnter;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<TeamKind> ListenToBombTrackEntryTriggerEnter;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ListenToBombAlmostDeliveredTriggerExit;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ListenToOvertimeStarted;

		public BombInstance ActiveBomb
		{
			get
			{
				return this._bombInstance;
			}
		}

		public bool SlowMotionEnabled { get; private set; }

		private void Awake()
		{
			this._timeScaleInitial = Time.timeScale;
			GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned += this.OnAllPlayersSpawned;
			GameHubBehaviour.Hub.Events.Bots.ListenToAllPlayersSpawned += this.OnAllBotsSpawned;
			SceneManager.sceneLoaded += this.OnSceneLoaded;
			this.waitForBombExplodeInSeconds = new WaitForSeconds((float)this.ActiveBomb.GetBombInfo().ExplodeInSeconds);
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
		{
			this.ScoreController.OnLevelLoaded();
			if (GameHubBehaviour.Hub.Match.ArenaIndex > -1)
			{
				GameArenaInfo gameArenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex];
				this.Rules.ShopPhaseSeconds = (float)gameArenaInfo.ShopPhaseSeconds;
				this.Rules.ReplayDelaySeconds = (float)gameArenaInfo.ReplayDelaySeconds;
			}
			this.ResetScoreBoard();
		}

		private void OnDestroy()
		{
			this.ListenToClientBombCreation = null;
			this.ClientListenToBombDrop = null;
			this.ListenToBombCarrierChanged = null;
			this.ListenToBombDrop = null;
			this.ListenToBombDelivery = null;
			this.ListenToBombUnspawn = null;
			this.ListenToPhaseChange = null;
			this.ListenToMatchUpdate = null;
			this.ListenToScoreFeedback = null;
			this.ListenToBombAlmostDeliveredTriggerEnter = null;
			this.ListenToBombLastCurveTriggerEnter = null;
			this.ListenToBombFirstCurveTriggerEnter = null;
			this.ListenToBombTrackEntryTriggerEnter = null;
			this.ListenToBombAlmostDeliveredTriggerExit = null;
			GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned -= this.OnAllPlayersSpawned;
			GameHubBehaviour.Hub.Events.Bots.ListenToAllPlayersSpawned -= this.OnAllBotsSpawned;
			SceneManager.sceneLoaded -= this.OnSceneLoaded;
		}

		private void OnAllPlayersSpawned()
		{
			if (!GameHubBehaviour.Hub.Events.Bots.CarCreationFinished || !GameHubBehaviour.Hub.Events.Players.CarCreationFinished)
			{
				return;
			}
			this.GridController.OnPlayersAndBotsSpawnFinish();
		}

		public void OnAllBotsSpawned()
		{
			if (!GameHubBehaviour.Hub.Events.Bots.CarCreationFinished || !GameHubBehaviour.Hub.Events.Players.CarCreationFinished)
			{
				return;
			}
			this.GridController.OnPlayersAndBotsSpawnFinish();
		}

		[RemoteMethod]
		public void OnGridGameStarted()
		{
			this.GridController.OnGridGameStarted();
		}

		[RemoteMethod]
		public void OnGridGameFinished(byte playerAdress, float finalValue)
		{
			this.GridController.OnGridGameFinished(playerAdress, finalValue);
		}

		[RemoteMethod]
		public void OnPlayerUpdatedGridProgress(byte playerAddress, int clientProgress)
		{
			this.GridController.OnPlayerUpdatedGridProgress(playerAddress, clientProgress);
		}

		private void ResetScoreBoard()
		{
			this.ScoreBoard.BombScoreRed = 0;
			this.ScoreBoard.BombScoreBlue = 0;
			this.ScoreBoard.CurrentState = BombScoreBoard.State.Warmup;
			this.ScoreBoard.Timeout = 0L;
			this.ScoreBoard.ResetRounds();
			GameHubBehaviour.Hub.Global.LockAllPlayers = true;
		}

		public void WarmupAlmostDone()
		{
			this.ScoreBoard.CurrentState = BombScoreBoard.State.PreBomb;
			if (this.ListenToPhaseChange != null)
			{
				this.ListenToPhaseChange(this.ScoreBoard.CurrentState);
			}
			this.ScoreBoard.Dirty = true;
		}

		public void OnBombCarrierChanged(int targetId)
		{
			Identifiable objectForId = this.GetObjectForId(targetId);
			CombatObject combatObject = (!(objectForId != null)) ? null : objectForId.GetBitComponent<CombatObject>();
			if (this.ListenToBombCarrierChanged != null)
			{
				this.ListenToBombCarrierChanged(combatObject);
			}
			if (this.ServerOnBombCarrierIdentifiableChanged != null)
			{
				this.ServerOnBombCarrierIdentifiableChanged(objectForId);
			}
			this.CheckIfBombDisputeFinishedWithAWinner(combatObject);
		}

		public bool GetBombPosition(ref Vector3 bombPosition)
		{
			if (!this.ActiveBomb.IsSpawned)
			{
				return false;
			}
			BombVisualController instance = BombVisualController.GetInstance(false);
			bombPosition = instance.transform.position;
			return true;
		}

		public Transform GetBombTransform()
		{
			if (!this.ActiveBomb.IsSpawned)
			{
				return null;
			}
			BombVisualController instance = BombVisualController.GetInstance(false);
			return instance.transform;
		}

		public void DisableBombGrabber(CombatObject obj)
		{
			this._disabledObjects.Add(obj);
		}

		public void EnableBombGrabber(CombatObject obj)
		{
			this._disabledObjects.Remove(obj);
		}

		public bool IsGrabberDisabled(CombatObject obj)
		{
			return this._disabledObjects.Contains(obj);
		}

		public bool IsPickDisabled(CombatObject obj)
		{
			if (!this._checkedForDropper)
			{
				this._checkedForDropper = true;
				float radius = ((SphereCollider)this.BombMovement.GetComponent<Collider>()).radius;
				Collider[] array = Physics.OverlapSphere(this.BombMovement.Combat.Transform.position, radius, 16384);
				for (int i = 0; i < array.Length; i++)
				{
					BombDropper component = array[i].GetComponent<BombDropper>();
					if (component != null)
					{
						this._disabledTeams.Add((component.TeamOwner != TeamKind.Blue) ? TeamKind.Blue : TeamKind.Red);
					}
				}
			}
			return this._disabledObjects.Contains(obj) || this._disabledTeams.Contains(obj.Team);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnDisputeStarted;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<TeamKind> OnDisputeFinished;

		public bool CompeteForBomb(CombatObject obj)
		{
			if (this.IsPickDisabled(obj))
			{
				return false;
			}
			if (obj.Team == TeamKind.Blue)
			{
				this._competingBlueObjects.Add(obj);
			}
			else if (obj.Team == TeamKind.Red)
			{
				this._competingRedObjects.Add(obj);
			}
			if (this.IsOtherTeamCompetingForBomb(obj) && !this.IsDisputeStarted && !this.SlowMotionEnabled && !this.IsSomeoneCarryingBomb())
			{
				this.DispatchReliable(GameHubBehaviour.Hub.SendAll).DisputeStarted();
				this.DisputeStarted();
				this.ToggleSlowMotion(true);
			}
			return true;
		}

		[RemoteMethod]
		public void DisputeStarted()
		{
			this.IsDisputeStarted = true;
			this._timeDisputeStarted = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (this.OnDisputeStarted != null)
			{
				this.OnDisputeStarted();
			}
		}

		[RemoteMethod]
		public void DisputeFinished(int teamKind)
		{
			this.IsDisputeStarted = false;
			this._timeDisputeStopped = GameHubBehaviour.Hub.GameTime.GetSynchTime();
			TeamKind obj = (TeamKind)teamKind;
			if (this.OnDisputeFinished != null)
			{
				this.OnDisputeFinished(obj);
			}
		}

		public void StopCompetingForBomb(CombatObject obj)
		{
			if (obj.Team == TeamKind.Blue)
			{
				this._competingBlueObjects.Remove(obj);
			}
			else if (obj.Team == TeamKind.Red)
			{
				this._competingRedObjects.Remove(obj);
			}
			if (this.IsDisputeStarted && this._competingRedObjects.Count + this._competingBlueObjects.Count == 0)
			{
				int teamKind = 0;
				this.DispatchReliable(GameHubBehaviour.Hub.SendAll).DisputeFinished(teamKind);
				this.DisputeFinished(teamKind);
			}
		}

		public void CancelDispute()
		{
			this._competingBlueObjects.Clear();
			this._competingRedObjects.Clear();
			if (this.IsDisputeStarted && this._competingRedObjects.Count + this._competingBlueObjects.Count == 0)
			{
				int teamKind = 0;
				this.DispatchReliable(GameHubBehaviour.Hub.SendAll).DisputeFinished(teamKind);
				this.DisputeFinished(teamKind);
			}
		}

		private void CheckIfBombDisputeFinishedWithAWinner(CombatObject combat)
		{
			if (combat == null || (combat.Team != TeamKind.Red && combat.Team != TeamKind.Blue) || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (GameHubBehaviour.Hub.GameTime.GetSynchTime() > this._timeDisputeStopped + Mathf.RoundToInt(1000f * this.Rules.TimeToCheckDisputeFinished))
			{
				return;
			}
			int teamOwner = (int)this.ActiveBomb.TeamOwner;
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).DisputeFinished(teamOwner);
			this.DisputeFinished(teamOwner);
		}

		public bool IsOtherTeamCompetingForBomb(CombatObject obj)
		{
			if (obj.Team == TeamKind.Blue)
			{
				return this._competingRedObjects.Count > 0;
			}
			return obj.Team == TeamKind.Red && this._competingBlueObjects.Count > 0;
		}

		public bool IsSomeoneCarryingBomb()
		{
			return this.ActiveBomb.IsSpawned && this.ActiveBomb.BombCarriersIds.Count > 0;
		}

		public TeamKind TeamCarryingBomb()
		{
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < this.ActiveBomb.BombCarriersIds.Count; i++)
			{
				CombatObject combat = CombatRef.GetCombat(this.ActiveBomb.BombCarriersIds[i]);
				if (combat && combat.Team == TeamKind.Red)
				{
					num2 += combat.BombGadget.BombGadgetInfo.OwnerMass / combat.BombGadget.BombGadgetInfo.TargetMass;
				}
				else if (combat)
				{
					num += combat.BombGadget.BombGadgetInfo.OwnerMass / combat.BombGadget.BombGadgetInfo.TargetMass;
				}
			}
			if (num == num2)
			{
				return TeamKind.Zero;
			}
			return (num <= num2) ? TeamKind.Red : TeamKind.Blue;
		}

		public bool IsCarryingBomb(Transform objTransform)
		{
			return this.IsCarryingBomb(objTransform.GetComponent<Collider>());
		}

		public bool IsCarryingBomb(Collider other)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			return this.IsCarryingBomb(combat);
		}

		public bool IsCarryingBomb(CombatObject combatObj)
		{
			return combatObj != null && combatObj.IsPlayer && this.IsCarryingBomb(combatObj.Id.ObjId);
		}

		public bool IsCarryingBomb(int id)
		{
			return this.ActiveBomb.IsSpawned && this.ActiveBomb.BombCarriersIds.Contains(id);
		}

		public void OnDetonatorCreated(PickupDropEvent data, BombDetonator bombDetonator)
		{
			BombManager.Log.InfoFormat("OnDetonatorCreated. Id: {0} pos: {1} Reason: {2}", new object[]
			{
				bombDetonator.Id.ObjId,
				bombDetonator.transform.position,
				data.Reason
			});
			this._disabledObjects.Clear();
			Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(data.Causer);
			if (@object != null)
			{
				this.OnBombDropped(@object.ObjId, data.Reason);
			}
			this.ActiveBomb.State = BombInstance.BombState.Idle;
			if (this.ListenToBombDelivery != null)
			{
				this.ListenToBombDelivery(data.Causer, (data.PickerTeam != TeamKind.Red) ? TeamKind.Red : TeamKind.Blue, bombDetonator.transform.position);
			}
			if (this.ListenToBombCarrierChanged != null)
			{
				this.ListenToBombCarrierChanged(null);
			}
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				base.StartCoroutine(this.StartDetonatorTimer(data, bombDetonator));
				return;
			}
			this.MatchUpdated();
		}

		private IEnumerator StartDetonatorTimer(PickupDropEvent data, BombDetonator bombDetonator)
		{
			this.ActiveBomb.DetonatorCauserId = bombDetonator.Id.ObjId;
			TeamKind team = data.PickerTeam;
			yield return this.waitForBombExplodeInSeconds;
			PlaybackManager.BombDetonation.Send(team, bombDetonator.Id.ObjId);
			this.ExplodeBomb(team, bombDetonator.transform.position);
			this.TimeToAnnouncer = 0L;
			yield return this.waitDotThreeSeconds;
			this.DoDamage(this.ActiveBomb);
			this.ActiveBomb.IsSpawned = false;
			this.ScoreController.OnBombDetonated(team, data.Causer);
			yield return UnityUtils.WaitForTwoSeconds;
			bombDetonator.RemoveBombAsset();
			yield break;
		}

		private void ExplodeBomb(TeamKind damagedTeam, Vector3 detonatorPosition)
		{
			float num = (float)(this.Rules.BombInfo.ExplosionRadius * this.Rules.BombInfo.ExplosionRadius);
			for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[i];
				if (playerData.Team == damagedTeam)
				{
					float sqrMagnitude = (playerData.CharacterInstance.transform.position - detonatorPosition).sqrMagnitude;
					if (sqrMagnitude <= num)
					{
						playerData.CharacterInstance.GetBitComponent<CombatController>().ForceDeath();
					}
				}
			}
		}

		public void DetonateBomb(TeamKind damagedTeam, int pickupInstanceId, int deliveryScore)
		{
			base.StartCoroutine(this.InnerDetonateClient(damagedTeam, pickupInstanceId, deliveryScore));
		}

		private IEnumerator InnerDetonateClient(TeamKind damagedTeam, int pickupInstanceId, int deliveryScore)
		{
			PickupManager.PickupInstance pickupInstance = GameHubBehaviour.Hub.Events.Pickups.GetPickUpByID(pickupInstanceId);
			if (pickupInstance == null)
			{
				for (int i = 0; i < 5; i++)
				{
					yield return this.waitDotThreeSeconds;
					pickupInstance = GameHubBehaviour.Hub.Events.Pickups.GetPickUpByID(pickupInstanceId);
					if (pickupInstance != null)
					{
						break;
					}
				}
			}
			if (pickupInstance == null)
			{
				BombManager.Log.WarnFormat("Could not find PickupInstance for id: {0}", new object[]
				{
					pickupInstanceId
				});
				yield break;
			}
			BombDetonator bombDetonator = pickupInstance.Pickup.GetComponent<BombDetonator>();
			if (bombDetonator == null)
			{
				BombManager.Log.WarnFormat("Could not find BombDetonator on PickupInstance to show bomb explosion. PickupInstanceId: {0} - Identifiable: {1}", new object[]
				{
					pickupInstanceId,
					pickupInstance.Pickup.ObjId
				});
				yield break;
			}
			bombDetonator.Detonate(deliveryScore);
			yield break;
		}

		public void OnPickupCreated(PickupDropEvent data, BombPickup bombPickup, int eventId)
		{
			this._disabledTeams.Clear();
			this._disabledObjects.Clear();
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.CancelDispute();
				this.ActiveBomb.eventId = eventId;
				PlaybackManager.BombInstance.Update(bombPickup.Id.ObjId, SpawnReason.Pickup);
				this.OnBombCarrierChanged(bombPickup.Id.ObjId);
			}
			this.ActiveBomb.IsSpawned = true;
			bombPickup.GetComponent<CombatObject>().CreepTeam = this.Rules.BombInfo.Team;
			BombVisualController instance = BombVisualController.GetInstance(true);
			instance.transform.parent = bombPickup.transform;
			instance.transform.localPosition = Vector3.zero;
			instance.SetCombatObject(bombPickup.transform);
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				instance.enabled = false;
			}
			if (this.ListenToClientBombCreation != null)
			{
				this.ListenToClientBombCreation(eventId);
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> OnSlowMotionToggled;

		private void ToggleSlowMotion(bool enable)
		{
			if (!this.Rules.SlowMotionEnabled)
			{
				return;
			}
			if (!this.SlowMotionEnabled && this.ScoreBoard.CurrentState != BombScoreBoard.State.BombDelivery)
			{
				BombManager.Log.Warn("Trying to activate slow motion outside of BombDelivery");
				return;
			}
			float timeScale = (!enable) ? this._timeScaleInitial : this.Rules.SlowMotionTimeScaleRate;
			GameHubBehaviour.Hub.GameTime.SetTimeScale(timeScale);
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).SlowMotionCallback(enable);
			this.SlowMotionCallback(enable);
		}

		[RemoteMethod]
		private void SlowMotionCallback(bool enable)
		{
			this.SlowMotionEnabled = enable;
			if (this.OnSlowMotionToggled != null)
			{
				this.OnSlowMotionToggled(enable);
			}
		}

		public void PhaseChanged()
		{
			if (this.ListenToPhaseChange != null)
			{
				this.ListenToPhaseChange(this.ScoreBoard.CurrentState);
			}
		}

		public void MatchUpdated()
		{
			if (this.ListenToMatchUpdate != null)
			{
				this.ListenToMatchUpdate();
			}
		}

		public BombInstance CreateBombInstance(BombInfo bombInfo)
		{
			this.ActiveBomb.DetonatorCauserId = 0;
			this.ActiveBomb.BombCarriersIds.Clear();
			this.ActiveBomb.TeamOwner = TeamKind.Zero;
			this.ActiveBomb.TeamTerrainOwner = TeamKind.Zero;
			return this.ActiveBomb;
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.MatchMan == null)
			{
				return;
			}
			if (this.SlowMotionEnabled && GameHubBehaviour.Hub.GameTime.GetPlaybackTime() > this._timeDisputeStarted + Mathf.RoundToInt(1000f * this.Rules.SlowMotionSeconds * this.Rules.SlowMotionTimeScaleRate))
			{
				this.ToggleSlowMotion(false);
			}
			this._disabledTeams.Clear();
			this._checkedForDropper = false;
			if (this._timeoutUpdater.ShouldHalt())
			{
				return;
			}
			BombScoreBoard.State currentState = this.ScoreBoard.CurrentState;
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.ScoreController.RunUpdate(playbackTime);
			if (currentState != this.ScoreBoard.CurrentState && this.ListenToPhaseChange != null)
			{
				this.ListenToPhaseChange(this.ScoreBoard.CurrentState);
			}
			this.GridController.RunUpdate(playbackTime);
		}

		public Identifiable GetObjectForId(int id)
		{
			return GameHubBehaviour.Hub.ObjectCollection.GetObject(id);
		}

		public CombatObject GetCombatObjectForId(int id)
		{
			Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(id);
			return (!(@object != null)) ? null : @object.GetBitComponent<CombatObject>();
		}

		public void DropDetonator(CombatObject bombPickup, TeamKind teamOwner)
		{
			this.ToggleSlowMotion(false);
			BombInstance activeBomb = this.ActiveBomb;
			if (activeBomb == null || activeBomb.DetonatorCauserId != 0)
			{
				return;
			}
			if (activeBomb.State == BombInstance.BombState.Meteor)
			{
				int lastCarrier = activeBomb.LastCarrier;
				CombatObject combatObject = CombatObject.GetCombatObject(lastCarrier);
				if (combatObject != null && combatObject.Stats != null)
				{
					combatObject.Stats.BombGadgetPowerShotScoreCount++;
				}
			}
			Vector3 position = bombPickup.transform.position;
			TeamKind teamKind = (teamOwner != TeamKind.Blue) ? TeamKind.Blue : TeamKind.Red;
			int num = activeBomb.LastCarriersByTeam[teamKind];
			if (num == -1)
			{
				List<PlayerData> list = (teamKind != TeamKind.Blue) ? GameHubBehaviour.Hub.Players.RedTeamPlayersAndBots : GameHubBehaviour.Hub.Players.BlueTeamPlayersAndBots;
				num = list[UnityEngine.Random.Range(0, list.Count)].CharacterInstance.ObjId;
			}
			activeBomb.DetonatorCauserId = num;
			SpawnReason reason = SpawnReason.TriggerDrop;
			if (teamOwner != TeamKind.Red)
			{
				if (teamOwner == TeamKind.Blue)
				{
					reason = SpawnReason.ScoreRed;
				}
			}
			else
			{
				reason = SpawnReason.ScoreBlu;
			}
			GameHubBehaviour.Hub.Events.TriggerEvent(new PickupDropEvent
			{
				Causer = num,
				Position = position,
				PickupAsset = activeBomb.GetBombInfo().AssetDetonator,
				UnspawnOnLifeTimeEnd = false,
				PickerTeam = teamOwner,
				Reason = reason
			});
			GameHubBehaviour.Hub.Events.TriggerEvent(new PickupRemoveEvent
			{
				Causer = num,
				PickupId = bombPickup.Id.ObjId,
				Position = position,
				Reason = reason,
				TargetEventId = activeBomb.eventId
			});
			BombManager.Log.InfoFormat("BombDetonated Player={0} TargetTeam={2} MatchTime={1}", new object[]
			{
				num,
				(float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() / 1000f,
				teamOwner
			});
		}

		private void DoDamage(BombInstance bombInstance)
		{
			CombatObject combatObjectForId = this.GetCombatObjectForId(bombInstance.DetonatorCauserId);
			ModifierData[] datas = ModifierData.CreateData(bombInstance.GetBombInfo().Modifiers);
			Collider[] array = Physics.OverlapSphere(this.BombMovement.transform.position, (float)bombInstance.GetBombInfo().ExplosionRadius, 1077058560);
			if (array == null || array.Length == 0)
			{
				return;
			}
			foreach (Collider comp in array)
			{
				CombatObject combat = CombatRef.GetCombat(comp);
				if (!(combat == null) && combat.IsAlive())
				{
					combat.Controller.AddModifiers(datas, combatObjectForId, -1, false);
				}
			}
		}

		public void ChangeTeamTrigger(TeamKind teamKind)
		{
			if (this.ActiveBomb.TeamTerrainOwner == teamKind)
			{
				return;
			}
			this.ActiveBomb.TeamTerrainOwner = teamKind;
		}

		[RemoteMethod]
		public void ClientEnableOvertimeEffects(int id)
		{
			Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(id);
			if (@object == null)
			{
				return;
			}
			BombTargetTrigger component = @object.GetComponent<BombTargetTrigger>();
			if (component == null)
			{
				return;
			}
			component.EnableOvertimeEffects();
		}

		public void OnBombDropped(int causerId, SpawnReason reason)
		{
			if (this.ListenToBombDrop != null)
			{
				this.ListenToBombDrop(this.ActiveBomb, reason, causerId);
			}
			if (GameHubBehaviour.Hub.Net.IsClient() && this.ClientListenToBombDrop != null)
			{
				this.ClientListenToBombDrop(this.ActiveBomb, reason, causerId);
			}
			this.ActiveBomb.BombCarriersIds.Remove(causerId);
			this.ActiveBomb.TeamOwner = this.TeamCarryingBomb();
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				PlaybackManager.BombInstance.Update(causerId, reason);
				MatchLogWriter.BombDropped(causerId, reason, this.BombMovement.Combat.Transform.position);
				float num = this.BombMovement.GetSpeedAngleToX();
				if (this.LastDropData.Populated)
				{
					if (num < 0f)
					{
						num = this.LastDropData.DropperForwardAngle;
					}
					MatchLogWriter.BombEvent(causerId, this.ConvertKind(reason), this.LastDropData.HoldTime, this.LastDropData.DropperPosition, num, this.ActiveBomb.State == BombInstance.BombState.Meteor);
					this.LastDropData.Populated = false;
				}
				else
				{
					Vector3 position = this.BombMovement.Combat.Transform.position;
					Identifiable objectForId = this.GetObjectForId(causerId);
					if (objectForId != null)
					{
						position = objectForId.transform.position;
					}
					MatchLogWriter.BombEvent(causerId, this.ConvertKind(reason), 0f, position, num, this.ActiveBomb.State == BombInstance.BombState.Meteor);
				}
				this.OnBombCarrierChanged(causerId);
			}
		}

		private GameServerBombEvent.EventKind ConvertKind(SpawnReason reason)
		{
			switch (reason)
			{
			case SpawnReason.TriggerDrop:
				return GameServerBombEvent.EventKind.TriggerDrop;
			case SpawnReason.InputDrop:
				if (this.LastDropData.PowerShot)
				{
					return GameServerBombEvent.EventKind.PowerShot;
				}
				return GameServerBombEvent.EventKind.InputDrop;
			default:
				if (reason == SpawnReason.Death)
				{
					return GameServerBombEvent.EventKind.Death;
				}
				if (reason != SpawnReason.BrokenLink)
				{
					BombManager.Log.ErrorFormat("Bomb dropped with invalid reason={0}", new object[]
					{
						reason
					});
					return GameServerBombEvent.EventKind.None;
				}
				return GameServerBombEvent.EventKind.BrokenLink;
			case SpawnReason.ScoreRed:
				return GameServerBombEvent.EventKind.ScoreRed;
			case SpawnReason.ScoreBlu:
				return GameServerBombEvent.EventKind.ScoreBlue;
			}
		}

		public void OnSpinningBegins(int causer)
		{
			this.ActiveBomb.State = BombInstance.BombState.Spinning;
			PlaybackManager.BombInstance.Update(causer, SpawnReason.StateChange);
		}

		public void OnSpinningEnd(int causer)
		{
			this.ActiveBomb.State = ((!this.IsSomeoneCarryingBomb()) ? BombInstance.BombState.Idle : BombInstance.BombState.Carried);
			PlaybackManager.BombInstance.Update(causer, SpawnReason.StateChange);
		}

		public void OnMeteorEnded(EffectRemoveEvent evt)
		{
			if (this.IsSomeoneCarryingBomb() || evt.TargetEventId != this.ActiveBomb.lastMeteorEventId)
			{
				return;
			}
			this.ActiveBomb.State = BombInstance.BombState.Idle;
			PlaybackManager.BombInstance.Update(evt.TargetEventId, SpawnReason.StateChange);
		}

		public void CallListenToBombAlmostDeliveredTriggerEnter(TeamKind trackTeamKind)
		{
			if (this.ListenToBombAlmostDeliveredTriggerEnter != null)
			{
				this.ListenToBombAlmostDeliveredTriggerEnter(trackTeamKind);
			}
			this.Dispatch(GameHubBehaviour.Hub.SendAll).ClientListenToBombAlmostDeliveredTriggerEnter((int)trackTeamKind);
		}

		public void CallListenToBombLastCurveTriggerEnter(TeamKind trackTeamKind)
		{
			this.Dispatch(GameHubBehaviour.Hub.SendAll).ClientListenToBombLastCurveTriggerEnter((int)trackTeamKind);
		}

		public void CallListenToBombFirstCurveTriggerEnter(TeamKind trackTeamKind)
		{
			this.Dispatch(GameHubBehaviour.Hub.SendAll).ClientListenToBombFirstCurveTriggerEnter((int)trackTeamKind);
		}

		public void CallListenToBombTrackEntryTriggerEnter(TeamKind trackTeamKind)
		{
			this.Dispatch(GameHubBehaviour.Hub.SendAll).ClientListenToBombTrackEntryTriggerEnter((int)trackTeamKind);
		}

		[RemoteMethod]
		public void ClientListenToBombAlmostDeliveredTriggerEnter(int trackTeamKind)
		{
			if (this.ListenToBombAlmostDeliveredTriggerEnter != null)
			{
				this.ListenToBombAlmostDeliveredTriggerEnter((TeamKind)trackTeamKind);
			}
		}

		[RemoteMethod]
		public void ClientListenToBombLastCurveTriggerEnter(int trackTeamKind)
		{
			if (this.ListenToBombLastCurveTriggerEnter != null)
			{
				this.ListenToBombLastCurveTriggerEnter((TeamKind)trackTeamKind);
			}
		}

		[RemoteMethod]
		public void ClientListenToBombFirstCurveTriggerEnter(int trackTeamKind)
		{
			if (this.ListenToBombFirstCurveTriggerEnter != null)
			{
				this.ListenToBombFirstCurveTriggerEnter((TeamKind)trackTeamKind);
			}
		}

		[RemoteMethod]
		public void ClientListenToBombTrackEntryTriggerEnter(int trackTeamKind)
		{
			if (this.ListenToBombTrackEntryTriggerEnter != null)
			{
				this.ListenToBombTrackEntryTriggerEnter((TeamKind)trackTeamKind);
			}
		}

		public void CallListenToBombAlmostDeliveredTriggerExit()
		{
			if (this.ListenToBombAlmostDeliveredTriggerExit != null)
			{
				this.ListenToBombAlmostDeliveredTriggerExit();
			}
			this.Dispatch(GameHubBehaviour.Hub.SendAll).ClientListenToBombAlmostDeliveredTriggerExit();
		}

		[RemoteMethod]
		public void ClientListenToBombAlmostDeliveredTriggerExit()
		{
			if (this.ListenToBombAlmostDeliveredTriggerExit != null)
			{
				this.ListenToBombAlmostDeliveredTriggerExit();
			}
		}

		public void GrabBomb(CombatObject combat)
		{
			bool meteor = this.ActiveBomb.State == BombInstance.BombState.Meteor;
			this.ActiveBomb.BombCarriersIds.Add(combat.Id.ObjId);
			this.ActiveBomb.LastCarriersByTeam[combat.Team] = (this.ActiveBomb.LastCarrier = combat.Id.ObjId);
			this.ActiveBomb.TeamOwner = this.TeamCarryingBomb();
			this.ActiveBomb.State = BombInstance.BombState.Carried;
			PlaybackManager.BombInstance.Update(combat.Id.ObjId, SpawnReason.Grabbed);
			this.OnBombCarrierChanged(combat.Id.ObjId);
			BombManager.Log.InfoFormat("BombPicked Player={0} MatchTime={1}", new object[]
			{
				combat.Id.ObjId,
				(float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() / 1000f
			});
			BombMatchBI.BombTaken(combat.Id.ObjId);
			MatchLogWriter.BombCollected(combat.Id.ObjId);
			MatchLogWriter.BombEvent(combat.Id.ObjId, GameServerBombEvent.EventKind.Grabbed, 0f, combat.Transform.position, this.BombMovement.GetSpeedAngleToX(), meteor);
		}

		public void OnBombUnspawned(PickupRemoveEvent data, BombPickup bombPickup)
		{
			this.ActiveBomb.IsSpawned = false;
			if (this.ListenToBombUnspawn != null)
			{
				this.ListenToBombUnspawn(data);
			}
			BombVisualController instance = BombVisualController.GetInstance(false);
			if (instance)
			{
				instance.transform.parent = null;
				instance.SetCombatObject(null);
			}
		}

		public void OvertimeStarted()
		{
			if (this.ListenToOvertimeStarted != null)
			{
				this.ListenToOvertimeStarted();
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this.ActiveBomb.IsSpawned = false;
		}

		private int OID
		{
			get
			{
				if (!this._identifiable)
				{
					this._identifiable = base.GetComponent<Identifiable>();
				}
				return this._identifiable.ObjId;
			}
		}

		public byte Sender { get; set; }

		public IBombManagerAsync Async()
		{
			return this.Async(0);
		}

		public IBombManagerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new BombManagerAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IBombManagerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new BombManagerDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IBombManagerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new BombManagerDispatch(this.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		protected IFuture Delayed
		{
			get
			{
				return this._delayed;
			}
		}

		protected void Delay(IFuture future)
		{
			this._delayed = future;
		}

		public object Invoke(int classId, short methodId, object[] args)
		{
			if (classId != 1029)
			{
				throw new Exception("Hierarchy in RemoteClass is not allowed!!! " + classId);
			}
			this._delayed = null;
			switch (methodId)
			{
			case 59:
				this.ClientListenToBombAlmostDeliveredTriggerEnter((int)args[0]);
				return null;
			case 60:
				this.ClientListenToBombLastCurveTriggerEnter((int)args[0]);
				return null;
			case 61:
				this.ClientListenToBombFirstCurveTriggerEnter((int)args[0]);
				return null;
			case 62:
				this.ClientListenToBombTrackEntryTriggerEnter((int)args[0]);
				return null;
			default:
				switch (methodId)
				{
				case 7:
					this.OnGridGameStarted();
					return null;
				case 8:
					this.OnGridGameFinished((byte)args[0], (float)args[1]);
					return null;
				case 9:
					this.OnPlayerUpdatedGridProgress((byte)args[0], (int)args[1]);
					return null;
				default:
					if (methodId == 20)
					{
						this.DisputeStarted();
						return null;
					}
					if (methodId == 21)
					{
						this.DisputeFinished((int)args[0]);
						return null;
					}
					if (methodId == 39)
					{
						this.SlowMotionCallback((bool)args[0]);
						return null;
					}
					if (methodId != 49)
					{
						throw new ScriptMethodNotFoundException(classId, (int)methodId);
					}
					this.ClientEnableOvertimeEffects((int)args[0]);
					return null;
				}
				break;
			case 64:
				this.ClientListenToBombAlmostDeliveredTriggerExit();
				return null;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BombManager));

		public BombRulesInfo Rules;

		private BombScoreController _scoreController;

		private BombGridController _gridController;

		public BombScoreBoard ScoreBoard = new BombScoreBoard();

		public BombMovement BombMovement;

		[NonSerialized]
		private readonly BombInstance _bombInstance = new BombInstance();

		private WaitForSeconds waitDotThreeSeconds = new WaitForSeconds(0.3f);

		private WaitForSeconds waitForBombExplodeInSeconds;

		private TimedUpdater _timeoutUpdater = new TimedUpdater
		{
			PeriodMillis = 50
		};

		public long TimeToAnnouncer;

		private HashSet<TeamKind> _disabledTeams = new HashSet<TeamKind>();

		private HashSet<CombatObject> _disabledObjects = new HashSet<CombatObject>();

		private HashSet<CombatObject> _competingBlueObjects = new HashSet<CombatObject>();

		private HashSet<CombatObject> _competingRedObjects = new HashSet<CombatObject>();

		private bool _checkedForDropper;

		public bool IsDisputeStarted;

		private int _timeDisputeStopped;

		private int _timeDisputeStarted;

		private float _timeScaleInitial;

		public BombManager.DropData LastDropData;

		public const int StaticClassId = 1029;

		private Identifiable _identifiable;

		[ThreadStatic]
		private BombManagerAsync _async;

		[ThreadStatic]
		private BombManagerDispatch _dispatch;

		private IFuture _delayed;

		public delegate void BombSpawnTrigger(int bombEventId);

		public delegate void BombDroppedListener(BombInstance bombInstance, SpawnReason reason, int causer);

		public delegate void BombCarrierChanged(CombatObject carrier);

		public delegate void BombDelivery(int causerId, TeamKind scoredTeam, Vector3 deliveryPosition);

		public struct DropData
		{
			public bool Populated;

			public Vector3 DropperPosition;

			public float DropperForwardAngle;

			public float HoldTime;

			public bool PowerShot;
		}
	}
}
