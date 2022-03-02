using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombScoreController : GameHubObject
	{
		public BombScoreController(BombManager bombManager, IBombInstanceDispatcher bombInstanceDispatcher, IScoreboardDispatcher scoreboardDispatcher)
		{
			this._bombManager = bombManager;
			this._bombInstanceDispatcher = bombInstanceDispatcher;
			this._scoreboardDispatcher = scoreboardDispatcher;
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
			if (GameHubObject.Hub.Match.MatchOver)
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
			case BombScoreboardState.Warmup:
				if (this.ScoreBoard.Timeout <= (long)matchTime)
				{
					if (GameHubObject.Hub.Match.LevelIsTutorial())
					{
						this.ChangeStateToPreBomb(matchTime);
					}
					else
					{
						this.ScoreBoard.CurrentState = BombScoreboardState.Shop;
						IGameArenaInfo currentArena = GameHubObject.Hub.ArenaConfig.GetCurrentArena();
						this.ScoreBoard.Timeout = (long)((int)(((float)currentArena.ShopPhaseSeconds + currentArena.FirstShopExtraTimeSeconds) * 1000f) + matchTime);
						this.ScoreBoard.Dirty = true;
					}
				}
				break;
			case BombScoreboardState.PreBomb:
				if (this.ScoreBoard.Timeout <= (long)matchTime)
				{
					this.UnlockPlayers();
					BombInstance bombInstance = this._bombManager.CreateBombInstance(this.Rules.BombInfo);
					this.ScoreBoard.CurrentState = BombScoreboardState.BombDelivery;
					IGameArenaInfo currentArena2 = GameHubObject.Hub.ArenaConfig.GetCurrentArena();
					this.ScoreBoard.Timeout = (long)((int)(currentArena2.RoundTimeSeconds * 1000f) + matchTime);
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
						Position = currentArena2.BombSpawnPoint
					};
					int eventId = GameHubObject.Hub.Events.TriggerEvent(content);
					bombInstance.eventId = eventId;
					this._bombInstanceDispatcher.Update(-1, GameHubObject.Hub.BombManager.ActiveBomb, SpawnReason.ScoreBoard);
					BombScoreController.Log.InfoFormat("Spawning bomb Round={0} MatchTime={1}", new object[]
					{
						this.ScoreBoard.Round,
						(float)matchTime / 1000f
					});
				}
				break;
			case BombScoreboardState.BombDelivery:
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
					this.ScoreBoard.CurrentState = BombScoreboardState.PreReplay;
				}
				break;
			}
			case BombScoreboardState.PreReplay:
				if (this.ScoreBoard.Timeout <= (long)matchTime)
				{
					this.LockPlayers();
					this.ScoreBoard.CurrentState = BombScoreboardState.Replay;
					this.ScoreBoard.Timeout = (long)((int)(this.Rules.ReplayTimeSeconds * 1000f) + matchTime);
					this.ScoreBoard.Dirty = true;
				}
				break;
			case BombScoreboardState.Replay:
				if (this.ScoreBoard.Timeout <= (long)matchTime)
				{
					this.RepositionPlayers();
					if (this.ScoreBoard.BombScoreRed == this.Rules.BombScoreTarget)
					{
						this.ScoreBoard.CurrentState = BombScoreboardState.EndGame;
						this.ScoreBoard.Timeout = long.MaxValue;
						this.ScoreBoard.Dirty = true;
						GameHubObject.Hub.MatchMan.EndMatch(TeamKind.Red);
					}
					else if (this.ScoreBoard.BombScoreBlue == this.Rules.BombScoreTarget)
					{
						this.ScoreBoard.CurrentState = BombScoreboardState.EndGame;
						this.ScoreBoard.Timeout = long.MaxValue;
						this.ScoreBoard.Dirty = true;
						GameHubObject.Hub.MatchMan.EndMatch(TeamKind.Blue);
					}
					else
					{
						this.ScoreBoard.CurrentState = BombScoreboardState.Shop;
						this.ScoreBoard.Timeout = (long)((int)(this.Rules.ShopPhaseSeconds * 1000f) + matchTime);
						this.ScoreBoard.Dirty = true;
					}
					this._bombTriggersBlue.Reset();
					this._bombTriggersRed.Reset();
				}
				break;
			case BombScoreboardState.Shop:
				if (this.ScoreBoard.Timeout <= (long)matchTime)
				{
					this.ChangeStateToPreBomb(matchTime);
				}
				break;
			}
			if (this.ScoreBoard.Dirty)
			{
				this._scoreboardDispatcher.Send();
				this.ScoreBoard.Dirty = false;
			}
		}

		private void ChangeStateToPreBomb(int matchTime)
		{
			this.ScoreBoard.CurrentState = BombScoreboardState.PreBomb;
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
			BombScoreController.Log.DebugFormat("Unlocking All players", new object[0]);
			List<PlayerData> playersAndBots = GameHubObject.Hub.Players.PlayersAndBots;
			for (int i = 0; i < playersAndBots.Count; i++)
			{
				PlayerData playerData = playersAndBots[i];
				Transform transform = playerData.CharacterInstance.transform;
				BombScoreController.Log.DebugFormat("Player Name={0}, position={1}", new object[]
				{
					playerData.Name,
					transform.position
				});
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
				BombScoreController.Log.DebugFormat("Repositioning={0} isAlive={1} position={2}", new object[]
				{
					playerData.Name,
					bitComponent.IsAlive(),
					spawn.position
				});
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
			foreach (BombTargetTrigger bombTargetTrigger in Object.FindObjectsOfType<BombTargetTrigger>())
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

		private IScoreboardDispatcher _scoreboardDispatcher;

		private IBombInstanceDispatcher _bombInstanceDispatcher;

		private BombManager _bombManager;

		private BombTargetTrigger _bombTriggersRed;

		private BombTargetTrigger _bombTriggersBlue;

		public delegate void OnBombTriggersReadyDelegate();
	}
}
