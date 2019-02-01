using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombScoreController : GameHubObject
	{
		public BombScoreController(BombManager bombManager)
		{
			this._bombManager = bombManager;
		}

		private BombScoreBoard ScoreBoard
		{
			get
			{
				return this._bombManager.ScoreBoard;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BombScoreController.OnBombTriggersReadyDelegate OnBombTriggersReady;

		private BombRulesInfo Rules
		{
			get
			{
				return this._bombManager.Rules;
			}
		}

		public void RunUpdate(int matchTime)
		{
			if (GameHubObject.Hub.MatchMan.MatchOver)
			{
				return;
			}
			this.CheckScoreBoard(matchTime);
		}

		private void CheckScoreBoard(int matchTime)
		{
			if (this.ScoreBoard.Timeout <= 0L)
			{
				return;
			}
			switch (this.ScoreBoard.CurrentState)
			{
			case BombScoreBoard.State.Warmup:
				if (this.ScoreBoard.Timeout <= (long)matchTime)
				{
					if (GameHubObject.Hub.Match.LevelIsTutorial())
					{
						this.ChangeStateToPreBomb(matchTime);
					}
					else
					{
						this.ScoreBoard.CurrentState = BombScoreBoard.State.Shop;
						int arenaIndex = GameHubObject.Hub.Match.ArenaIndex;
						GameArenaInfo gameArenaInfo = GameHubObject.Hub.ArenaConfig.Arenas[arenaIndex];
						this.ScoreBoard.Timeout = (long)((int)(((float)gameArenaInfo.ShopPhaseSeconds + gameArenaInfo.FirstShopExtraTimeSeconds) * 1000f) + matchTime);
						this.ScoreBoard.Dirty = true;
					}
				}
				break;
			case BombScoreBoard.State.PreBomb:
				if (this.ScoreBoard.Timeout <= (long)matchTime)
				{
					this.UnlockPlayers();
					BombInstance bombInstance = this._bombManager.CreateBombInstance(this.Rules.BombInfo);
					this.ScoreBoard.CurrentState = BombScoreBoard.State.BombDelivery;
					GameArenaInfo currentArena = GameHubObject.Hub.ArenaConfig.GetCurrentArena();
					this.ScoreBoard.Timeout = (long)((int)(currentArena.RoundTimeSeconds * 1000f) + matchTime);
					this.ScoreBoard.RoundStartTimeMillis = GameHubObject.Hub.GameTime.MatchTimer.GetTime();
					this.ScoreBoard.OvertimeStartTimeMillis = 0;
					GameHubObject.Hub.GameTime.MatchTimer.Start();
					this.ScoreBoard.Dirty = true;
					PickupDropEvent content = new PickupDropEvent
					{
						Causer = -1,
						PickupAsset = this.Rules.BombInfo.AssetPickup,
						UnspawnOnLifeTimeEnd = false,
						Reason = SpawnReason.ScoreBoard,
						Position = currentArena.BombSpawnPoint
					};
					int eventId = GameHubObject.Hub.Events.TriggerEvent(content);
					bombInstance.eventId = eventId;
					PlaybackManager.BombInstance.Update(-1, SpawnReason.ScoreBoard);
					BombScoreController.Log.InfoFormat("Spawning bomb Round={0} MatchTime={1}", new object[]
					{
						this.ScoreBoard.Round,
						(float)matchTime / 1000f
					});
				}
				break;
			case BombScoreBoard.State.BombDelivery:
			{
				bool flag = GameHubObject.Hub.Match.LevelIsTutorial();
				if (this._bombManager.ActiveBomb.IsSpawned)
				{
					if (!flag && this.ScoreBoard.Timeout <= (long)matchTime && !this.ScoreBoard.IsInOvertime)
					{
						this.ScoreBoard.OvertimeStartTimeMillis = GameHubObject.Hub.GameTime.MatchTimer.GetTime();
						this.ScoreBoard.IsInOvertime = true;
						this.ScoreBoard.Dirty = true;
						this._bombManager.OvertimeStarted();
					}
				}
				else if (!flag)
				{
					GameHubObject.Hub.GameTime.MatchTimer.Stop();
					this.ScoreBoard.Timeout = (long)((int)(this.Rules.ReplayDelaySeconds * 1000f) + matchTime);
					this.ScoreBoard.CurrentState = BombScoreBoard.State.PreReplay;
				}
				break;
			}
			case BombScoreBoard.State.PreReplay:
				if (this.ScoreBoard.Timeout <= (long)matchTime)
				{
					this.LockPlayers();
					this.ScoreBoard.CurrentState = BombScoreBoard.State.Replay;
					this.ScoreBoard.Timeout = (long)((int)(this.Rules.ReplayTimeSeconds * 1000f) + matchTime);
					this.ScoreBoard.Dirty = true;
				}
				break;
			case BombScoreBoard.State.Replay:
				if (this.ScoreBoard.Timeout <= (long)matchTime)
				{
					this.RepositionPlayers();
					if (this.ScoreBoard.BombScoreRed == this.Rules.BombScoreTarget)
					{
						this.ScoreBoard.CurrentState = BombScoreBoard.State.EndGame;
						this.ScoreBoard.Timeout = long.MaxValue;
						this.ScoreBoard.Dirty = true;
						GameHubObject.Hub.MatchMan.EndMatch(TeamKind.Red);
					}
					else if (this.ScoreBoard.BombScoreBlue == this.Rules.BombScoreTarget)
					{
						this.ScoreBoard.CurrentState = BombScoreBoard.State.EndGame;
						this.ScoreBoard.Timeout = long.MaxValue;
						this.ScoreBoard.Dirty = true;
						GameHubObject.Hub.MatchMan.EndMatch(TeamKind.Blue);
					}
					else
					{
						this.ScoreBoard.CurrentState = BombScoreBoard.State.Shop;
						this.ScoreBoard.Timeout = (long)((int)(this.Rules.ShopPhaseSeconds * 1000f) + matchTime);
						this.ScoreBoard.Dirty = true;
					}
					this._bombTriggersBlue.Reset();
					this._bombTriggersRed.Reset();
				}
				break;
			case BombScoreBoard.State.Shop:
				if (this.ScoreBoard.Timeout <= (long)matchTime)
				{
					this.ChangeStateToPreBomb(matchTime);
				}
				break;
			}
			if (this.ScoreBoard.Dirty)
			{
				PlaybackManager.Scoreboard.Send();
				this.ScoreBoard.Dirty = false;
			}
		}

		private void ChangeStateToPreBomb(int matchTime)
		{
			this.ScoreBoard.CurrentState = BombScoreBoard.State.PreBomb;
			this.ScoreBoard.Timeout = (long)(this.Rules.PreBombDurationSeconds * 1000 + matchTime);
			this.ScoreBoard.Dirty = true;
			this.RestoreInvincibles();
		}

		private void LockPlayers()
		{
			GameHubObject.Hub.Global.LockAllPlayers = true;
		}

		private void UnlockPlayers()
		{
			GameHubObject.Hub.Global.LockAllPlayers = false;
			List<PlayerData> playersAndBots = GameHubObject.Hub.Players.PlayersAndBots;
			for (int i = 0; i < playersAndBots.Count; i++)
			{
				PlayerData playerData = playersAndBots[i];
				Transform transform = playerData.CharacterInstance.transform;
			}
		}

		private void RepositionPlayers()
		{
			List<PlayerData> playersAndBots = GameHubObject.Hub.Players.PlayersAndBots;
			for (int i = 0; i < playersAndBots.Count; i++)
			{
				PlayerData playerData = playersAndBots[i];
				CombatObject bitComponent = playerData.CharacterInstance.GetBitComponent<CombatObject>();
				bitComponent.Clear();
				Transform spawn = bitComponent.SpawnController.GetSpawn();
				bitComponent.Movement.ForcePositionAndRotation(spawn.position, spawn.rotation);
				bitComponent.Attributes.ForceInvincible = true;
			}
		}

		private void RestoreInvincibles()
		{
			List<PlayerData> playersAndBots = GameHubObject.Hub.Players.PlayersAndBots;
			for (int i = 0; i < playersAndBots.Count; i++)
			{
				PlayerData playerData = playersAndBots[i];
				CombatObject bitComponent = playerData.CharacterInstance.GetBitComponent<CombatObject>();
				bitComponent.Attributes.ForceInvincible = false;
			}
		}

		public void OnLevelLoaded()
		{
			foreach (BombTargetTrigger bombTargetTrigger in UnityEngine.Object.FindObjectsOfType<BombTargetTrigger>())
			{
				TeamKind teamOwner = bombTargetTrigger.TeamOwner;
				if (teamOwner != TeamKind.Red)
				{
					if (teamOwner == TeamKind.Blue)
					{
						this._bombTriggersBlue = bombTargetTrigger;
					}
				}
				else
				{
					this._bombTriggersRed = bombTargetTrigger;
				}
			}
			if (this._bombTriggersRed && this._bombTriggersBlue && this.OnBombTriggersReady != null)
			{
				this.OnBombTriggersReady();
			}
		}

		public void OnBombDetonated(TeamKind terrainTeam, int causer)
		{
			RoundStats currentRound = this.ScoreBoard.CurrentRound;
			currentRound.DeliveryTime = GameHubObject.Hub.GameTime.MatchTimer.GetTime() / 1000;
			currentRound.Deliverer = causer;
			if (terrainTeam != TeamKind.Blue)
			{
				if (terrainTeam == TeamKind.Red)
				{
					this.ScoreBoard.BombScoreBlue++;
					GameHubObject.Hub.ScrapBank.AddBombReward(TeamKind.Blue, causer, this.ScoreBoard.Round);
					currentRound.DeliverTeam = TeamKind.Blue;
					if (this.ScoreBoard.BombScoreBlue == this.Rules.BombScoreTarget)
					{
						this.ScoreBoard.MatchOver = true;
					}
					BombMatchBI.RoundOver((float)GameHubObject.Hub.GameTime.GetPlaybackTime() / 1000f, GameHubObject.Hub.ScrapBank.RedScrap, GameHubObject.Hub.ScrapBank.BluScrap);
				}
			}
			else
			{
				this.ScoreBoard.BombScoreRed++;
				GameHubObject.Hub.ScrapBank.AddBombReward(TeamKind.Red, causer, this.ScoreBoard.Round);
				currentRound.DeliverTeam = TeamKind.Red;
				if (this.ScoreBoard.BombScoreRed == this.Rules.BombScoreTarget)
				{
					this.ScoreBoard.MatchOver = true;
				}
				BombMatchBI.RoundOver((float)GameHubObject.Hub.GameTime.GetPlaybackTime() / 1000f, GameHubObject.Hub.ScrapBank.RedScrap, GameHubObject.Hub.ScrapBank.BluScrap);
			}
			this.ScoreBoard.IsInOvertime = false;
			this.ScoreBoard.Dirty = true;
			this.ScoreBoard.NextRound();
		}

		public BombTargetTrigger GetBombTargetTrigger(TeamKind teamKind)
		{
			return (teamKind != TeamKind.Blue) ? this._bombTriggersRed : this._bombTriggersBlue;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BombScoreController));

		private BombManager _bombManager;

		private BombTargetTrigger _bombTriggersRed;

		private BombTargetTrigger _bombTriggersBlue;

		public delegate void OnBombTriggersReadyDelegate();
	}
}
