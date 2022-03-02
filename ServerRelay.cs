using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Battlepass;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.CompetitiveMode.DataTransferObjects.Players;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Match.Infra;
using HeavyMetalMachines.Matches.DataTransferObjects;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.Tournaments.DataTransferObjects;
using Hoplon.Serialization;
using Hoplon.Time;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines
{
	public class ServerRelay : GameHubBehaviour, ClientReconnectMessage.IClientReconnectListener, MatchController.GameOverMessage.IGameOverListener, ClientDisconnectMessage.IClientDisconnectListener, TerminateServerMessage.ITerminateServerListener
	{
		public void OnGameOver(MatchController.GameOverMessage msg)
		{
			if (!this._coroutineRunning)
			{
				this._coroutineRunning = true;
				base.StartCoroutine(this.GameOverActions(msg));
				return;
			}
		}

		private IEnumerator GameOverActions(MatchController.GameOverMessage msg)
		{
			ServerRelay.Log.Debug("[GameOverActions] StartCoroutine");
			yield return 0;
			Future quitServer = new Future
			{
				Name = "GameOverActions_Quit"
			};
			quitServer.WhenDone(delegate(IFuture future)
			{
				this.QuitServerWhenDoneFuture(msg, future.Name);
			});
			Future final = new Future
			{
				Name = "GameOverActions_Final"
			};
			final.WhenDone(new FutureCallback(this.finalWhenDoneFuture));
			quitServer.DependsOn(final);
			this._matchNeverStarted = ServerRelay.GetMatchNeverStarted(msg);
			TeamKind winner = this.GetWinnerTeam(msg.State);
			this.TryAddTournamentAndRankingUpdateFuture(final, winner);
			this.AddRewardsForPlayers(msg, final, winner);
			final.Result = 0;
			ServerRelay.Log.DebugFormat("[GameOverActions] Timeout started. {0} seconds to close server", new object[]
			{
				GameHubBehaviour.Hub.SharedConfigs.TimeOutSecondsToCloseServer
			});
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime((float)GameHubBehaviour.Hub.SharedConfigs.TimeOutSecondsToCloseServer));
			quitServer.Result = 0;
			yield break;
		}

		private void AddRewardsForPlayers(MatchController.GameOverMessage msg, Future finalFuture, TeamKind winner)
		{
			float timeSeconds = GameHubBehaviour.Hub.GameTime.MatchTimer.GetTimeSeconds();
			int rewardsByMatchTime = GameHubBehaviour.Hub.Server.Rewards.GetRewardsByMatchTime(timeSeconds);
			int totalFounderBonus = ServerRelay.GetTotalFounderBonus();
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Clients.Count; i++)
			{
				this.AddRewardsFutureToPlayer(msg, i, finalFuture, rewardsByMatchTime, winner, totalFounderBonus);
			}
		}

		private void TryAddTournamentAndRankingUpdateFuture(Future final, TeamKind winner)
		{
			if (this._matchNeverStarted)
			{
				return;
			}
			switch (GameHubBehaviour.Hub.Match.Kind)
			{
			case 0:
			case 3:
			case 7:
				this.AddRankedFutureUpdate(final, winner);
				break;
			case 5:
				this.AddTournamentFuture(final, winner);
				this.AddRankedFutureUpdate(final, winner);
				break;
			}
		}

		private static bool GetMatchNeverStarted(MatchController.GameOverMessage msg)
		{
			bool flag = msg.State == MatchData.MatchState.Nothing || msg.State == MatchData.MatchState.AwaitingConnections || msg.State == MatchData.MatchState.CharacterPick;
			if (flag)
			{
				ServerRelay.Log.WarnFormat("[GameOverActions] Match never started. MatchState={0}", new object[]
				{
					msg.State
				});
			}
			return flag;
		}

		private void AddRewardsFutureToPlayer(MatchController.GameOverMessage msg, int i, Future final, int xpByTime, TeamKind winner, int founderBonus)
		{
			PlayerData playerData = GameHubBehaviour.Hub.Players.Clients[i];
			if (this._matchNeverStarted || playerData.IsNarrator)
			{
				this.GiveEmptyRewards(playerData, final);
				return;
			}
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this.GiveFakeRewards(msg, playerData);
				return;
			}
			this.AddRewardsFuture(playerData, xpByTime, winner, founderBonus, final);
		}

		private void AddRewardsFuture(PlayerData player, int xpByTime, TeamKind winner, int founderBonus, Future final)
		{
			RewardsBag rewardsFor = this.GetRewardsFor(player, xpByTime, winner, founderBonus);
			Future future = new Future
			{
				Name = "GameOverActions_Reward"
			};
			final.DependsOn(future);
			this.UpdatePlayersMatchHistory(player, rewardsFor, winner);
			this.UpdateRemainingNoviceTrials(player, GameHubBehaviour.Hub.Match.Kind, rewardsFor);
			switch (GameHubBehaviour.Hub.Match.Kind)
			{
			case 0:
			case 3:
			case 5:
			case 7:
				this.UpdateMMR(player, future, final);
				this.UpdateRewards(player, future, rewardsFor, false);
				break;
			case 1:
			case 4:
			case 6:
				this.UpdateRewards(player, future, rewardsFor, true);
				break;
			case 2:
				future.Result = rewardsFor;
				break;
			}
		}

		private void GiveFakeRewards(MatchController.GameOverMessage msg, PlayerData player)
		{
			RewardsBag rewardsBag = new RewardsBag();
			string value = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SkipSFMissionConfigHack);
			if (string.IsNullOrEmpty(value))
			{
				rewardsBag.OldMissionProgresses = new MissionProgress[0];
				rewardsBag.MissionProgresses = new MissionProgress[0];
				rewardsBag.MissionsCompleted = new MissionCompleted[0];
			}
			else
			{
				string[] split = value.Split(new char[]
				{
					','
				});
				this.FakeValidateMissionByConfigAccessParams(player, rewardsBag, split, this.GetWinnerTeam(msg.State));
			}
			ServerRelay.Log.DebugFormat("Sending SetPlayerRewards by SkipSwordfish. player={0} s={1}", new object[]
			{
				player.PlayerAddress,
				value
			});
			GameHubBehaviour.Hub.Server.Dispatch(new byte[]
			{
				player.PlayerAddress
			}).SetPlayerRewards(rewardsBag.ToString());
		}

		private void GiveEmptyRewards(PlayerData player, Future final)
		{
			this.ClearMatchAndGiveRewards(player, new RewardsBag
			{
				OldMissionProgresses = new MissionProgress[0],
				MissionProgresses = new MissionProgress[0],
				MissionsCompleted = new MissionCompleted[0]
			}, final);
		}

		private static int GetTotalFounderBonus()
		{
			int num = 0;
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.Players[i];
				PlayerBag bag = playerData.Bag;
				byte founderPackLevel = bag.FounderPackLevel;
				FounderLevel founderLevel = founderPackLevel;
				if (FounderLevelEx.CheckHasFlag(founderLevel, 4))
				{
					num += GameHubBehaviour.Hub.SharedConfigs.FounderPackConfig.FounderPackGoldXpBonus;
				}
				else if (FounderLevelEx.CheckHasFlag(founderLevel, 2))
				{
					num += GameHubBehaviour.Hub.SharedConfigs.FounderPackConfig.FounderPackSilverXpBonus;
				}
				else if (FounderLevelEx.CheckHasFlag(founderLevel, 1))
				{
					num += GameHubBehaviour.Hub.SharedConfigs.FounderPackConfig.FounderPackBronzeXpBonus;
				}
			}
			return num;
		}

		private void AddRankedFutureUpdate(Future finalFuture, TeamKind winnerTeam)
		{
			Future future = new Future
			{
				Name = "GameOverActions_rankedUpdate"
			};
			finalFuture.DependsOn(future);
			this.UpdatePlayerCompetitiveStates(future, winnerTeam);
		}

		private void AddTournamentFuture(Future finalFuture, TeamKind winner)
		{
			if (winner == TeamKind.Red || winner == TeamKind.Blue)
			{
				Future future = new Future
				{
					Name = "GameOverActions_tournamentUpdate"
				};
				finalFuture.DependsOn(future);
				this.UpdateTeamOnTournament(future, winner);
			}
			else
			{
				ServerRelay.Log.WarnFormat("[GameOverActions] Tournament match ended without a clear winner", new object[0]);
			}
		}

		private void finalWhenDoneFuture(IFuture future)
		{
			ServerRelay.Log.DebugFormat("[GameOverActions][FutureWhenDone={0}] All bags cleared, lowering server priority", new object[]
			{
				future.Name
			});
			MatchLogWriter.WriteBotsDifficulty(this._getBotDifficulty);
			MatchLogWriter.WritePlayerExperience();
			ServerRelay.Log.InfoFormat("[GameOverActions] All game over actions done.", new object[0]);
			GameHubBehaviour.Hub.Swordfish.Connection.LowerPriority();
		}

		private void QuitServerWhenDoneFuture(MatchController.GameOverMessage msg, string futureName)
		{
			ServerRelay.Log.DebugFormat("[GameOverActions][FutureWhenDone={0}] All done, terminating server MatchNeverStarted={1}", new object[]
			{
				futureName,
				this._matchNeverStarted
			});
			MatchLogWriter.MatchEnd(msg.State);
			GameHubBehaviour.Hub.Quit(12);
		}

		private void UpdatePlayerCompetitiveStates(IFuture rankedUpdateFuture, TeamKind winner)
		{
			CompetitiveMatchResult competitiveMatchResult = this.GetCompetitiveMatchResult(winner);
			ServerRelay.Log.DebugFormat("[GameOverActions] Updating competitive states={0}", new object[]
			{
				competitiveMatchResult
			});
			ObservableExtensions.Subscribe<PlayerCompetitiveState[]>(Observable.DoOnError<PlayerCompetitiveState[]>(Observable.Do<PlayerCompetitiveState[]>(this._diContainer.Resolve<IProcessAndGetUpdatedCompetitiveStates>().Process(competitiveMatchResult), delegate(PlayerCompetitiveState[] states)
			{
				ServerRelay.Log.DebugFormat("[GameOverActions] Competitive states Process Do", new object[0]);
				for (int i = 0; i < states.Length; i++)
				{
					PlayerData playerData = this.GetPlayerData(states[i].PlayerId);
					if (playerData == null || !playerData.Connected)
					{
						ServerRelay.Log.DebugFormat("[GameOverActions] Skipping SetPlayerCompetitiveState for {0}", new object[]
						{
							states[i].PlayerId
						});
					}
					else
					{
						SerializablePlayerCompetitiveState serializablePlayerCompetitiveState = ServerRelay.ConvertPlayerCompetitiveStateToSerializable(states[i]);
						ServerRelay.Log.DebugFormat("[GameOverActions] SetPlayerCompetitiveState for playerid={0} PlayerAddress={1}", new object[]
						{
							states[i].PlayerId,
							playerData.PlayerAddress
						});
						GameHubBehaviour.Hub.Server.DispatchReliable(new byte[]
						{
							playerData.PlayerAddress
						}).SetPlayerCompetitiveState((string)serializablePlayerCompetitiveState);
					}
				}
				ServerRelay.Log.DebugFormat("[GameOverActions] Competitive states updated", new object[0]);
				rankedUpdateFuture.Result = 0;
			}), delegate(Exception ex)
			{
				ServerRelay.Log.ErrorFormat("[GameOverActions] UpdatePlayerCompetitiveStates failed! Ex={0}", new object[]
				{
					ex
				});
				rankedUpdateFuture.Result = -1;
			}));
		}

		private CompetitiveMatchResult GetCompetitiveMatchResult(TeamKind winner)
		{
			List<PlayerData> playersDatas;
			List<PlayerData> playersDatas2;
			if (winner == TeamKind.Blue)
			{
				playersDatas = GameHubBehaviour.Hub.Players.BlueTeamPlayersAndBots;
				playersDatas2 = GameHubBehaviour.Hub.Players.RedTeamPlayersAndBots;
			}
			else
			{
				playersDatas = GameHubBehaviour.Hub.Players.RedTeamPlayersAndBots;
				playersDatas2 = GameHubBehaviour.Hub.Players.BlueTeamPlayersAndBots;
			}
			bool isRankedMatch = GameHubBehaviour.Hub.Match.Kind == 3;
			return new CompetitiveMatchResult
			{
				MatchId = GameHubBehaviour.Hub.Swordfish.Connection.ServerMatchId.ToString(),
				WinnerTeam = this.GetTeamMatchState(playersDatas),
				LoserTeam = this.GetTeamMatchState(playersDatas2),
				IsRankedMatch = isRankedMatch
			};
		}

		private CompetitiveMatchPlayerState[] GetTeamMatchState(List<PlayerData> playersDatas)
		{
			CompetitiveMatchPlayerState[] array = new CompetitiveMatchPlayerState[playersDatas.Count];
			for (int i = 0; i < playersDatas.Count; i++)
			{
				PlayerData playerData = playersDatas[i];
				if (playerData.IsBot)
				{
					array[i] = new CompetitiveMatchPlayerState
					{
						PlayerId = -1L,
						Afk = false
					};
				}
				else
				{
					array[i] = new CompetitiveMatchPlayerState
					{
						PlayerId = playerData.PlayerId,
						Afk = this._afkManager.CheckLeaver(playerData)
					};
				}
			}
			return array;
		}

		private static SerializablePlayerCompetitiveState ConvertPlayerCompetitiveStateToSerializable(PlayerCompetitiveState state)
		{
			return new SerializablePlayerCompetitiveState
			{
				PlayerId = state.PlayerId,
				CalibrationTotalMatchesPlayed = state.CalibrationState.TotalMatchesPlayed,
				CalibrationTotalRequiredMatches = state.CalibrationState.TotalRequiredMatches,
				CalibrationMatchesResults = state.CalibrationState.MatchesResults,
				CurrentRank = ServerRelay.ConvertRankToSerializable(state.Rank.CurrentRank),
				HighestRank = ServerRelay.ConvertRankToSerializable(state.Rank.HighestRank),
				LockedTotalMatchesPlayed = state.Requirements.TotalMatchesPlayed,
				LockedTotalRequiredMatches = state.Requirements.TotalRequiredMatches,
				Status = state.Status
			};
		}

		private static SerializableCompetitiveRank ConvertRankToSerializable(CompetitiveRank rank)
		{
			return new SerializableCompetitiveRank
			{
				Division = rank.Division,
				Score = rank.Score,
				Subdivision = rank.Subdivision,
				TopPlacementPosition = rank.TopPlacementPosition
			};
		}

		private PlayerData GetPlayerData(long playerId)
		{
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.Players[i];
				if (playerData.PlayerId == playerId)
				{
					return playerData;
				}
			}
			return null;
		}

		private void UpdateRewards(PlayerData player, Future rewardFuture, RewardsBag rewards, bool isCustomOrPve)
		{
			if (isCustomOrPve)
			{
				this.ClearMatchAndGiveRewards(player, rewards, rewardFuture);
				rewardFuture.Result = 0;
			}
			else
			{
				ServerRelay.PlayerStateObject playerStateObject = new ServerRelay.PlayerStateObject
				{
					Data = player,
					PendingFuture = rewardFuture,
					GivenRewards = rewards
				};
				BattlepassCustomWS.UpdateRewards(rewards, playerStateObject, new SwordfishClientApi.ParameterizedCallback<string>(this.OnUpdateRewardsSuccess), new SwordfishClientApi.ErrorCallback(this.OnUpdateRewardsError));
			}
		}

		private void FakeValidateMissionByConfigAccessParams(PlayerData player, RewardsBag reward, string[] split, TeamKind winner)
		{
			player.BattlepassProgress.MissionProgresses = new List<MissionProgress>();
			reward.OldMissionProgresses = new MissionProgress[split.Length];
			for (int i = 0; i < split.Length; i++)
			{
				MissionProgress missionProgress = new MissionProgress
				{
					MissionIndex = int.Parse(split[i])
				};
				player.BattlepassProgress.MissionProgresses.Add(missionProgress);
				reward.OldMissionProgresses[i] = missionProgress.Clone();
			}
			PlayerStats bitComponent = player.CharacterInstance.GetBitComponent<PlayerStats>();
			bitComponent.MatchWon = (player.Team == winner);
			GameHubBehaviour.Hub.Server.Rewards.SetPerformance(reward, bitComponent, GameHubBehaviour.Hub.MatchHistory);
			this._battlepassMissionValidator.ValidateMissionCompletion(GameHubBehaviour.Hub.SharedConfigs.Battlepass.Mission.Missions, player.BattlepassProgress, bitComponent, GameHubBehaviour.Hub.MatchHistory, GameHubBehaviour.Hub.Players, GameHubBehaviour.Hub.InventoryColletion);
			reward.MissionProgresses = new MissionProgress[player.BattlepassProgress.MissionProgresses.Count];
			for (int j = 0; j < player.BattlepassProgress.MissionProgresses.Count; j++)
			{
				reward.MissionProgresses[j] = player.BattlepassProgress.MissionProgresses[j].Clone();
			}
			reward.MissionsCompleted = new MissionCompleted[bitComponent.MissionsCompletedIndex.Count];
			for (int k = 0; k < bitComponent.MissionsCompletedIndex.Count; k++)
			{
				reward.MissionsCompleted[k] = bitComponent.MissionsCompletedIndex[k];
			}
		}

		private void OnUpdateRewardsError(object state, Exception exception)
		{
			ServerRelay.PlayerStateObject playerStateObject = (ServerRelay.PlayerStateObject)state;
			ServerRelay.Log.ErrorFormat("OnUpdateRewardsError. player={0} exception={1}", new object[]
			{
				playerStateObject.Data.PlayerId,
				exception.ToString()
			});
			this.ClearMatchAndGiveRewards(playerStateObject.Data, playerStateObject.GivenRewards, playerStateObject.PendingFuture);
			if (playerStateObject.PendingFuture != null)
			{
				playerStateObject.PendingFuture.Result = -1;
			}
		}

		private void OnUpdateRewardsSuccess(object state, string s)
		{
			ServerRelay.PlayerStateObject playerStateObject = (ServerRelay.PlayerStateObject)state;
			ServerRelay.Log.DebugFormat("OnUpdateRewardsSuccess.player={0} bag={1}", new object[]
			{
				playerStateObject.Data.PlayerId,
				s
			});
			NetResult netResult = (NetResult)((JsonSerializeable<!0>)s);
			if (netResult.Success)
			{
				playerStateObject.GivenRewards = (RewardsBag)((JsonSerializeable<!0>)netResult.Msg);
				int levelForXp = GameHubBehaviour.Hub.SharedConfigs.Battlepass.GetLevelForXp(playerStateObject.GivenRewards.OldBattlepassXP);
				int levelForXp2 = GameHubBehaviour.Hub.SharedConfigs.Battlepass.GetLevelForXp(playerStateObject.GivenRewards.CurrentBattlepassXP);
				if (levelForXp != levelForXp2)
				{
					this.WriteLevelUpBI(levelForXp2, playerStateObject.Data);
				}
				int levelForXP = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetLevelForXP(playerStateObject.GivenRewards.OldCharacterXP);
				int levelForXP2 = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetLevelForXP(playerStateObject.GivenRewards.CurrentCharacterXP);
				ServerRelay.Log.DebugFormat("oldCharacterLevel={0} currentCharacterLevel={1} OldCharacterXP={2} CurrentCharacterXP={3}", new object[]
				{
					levelForXP,
					levelForXP2,
					playerStateObject.GivenRewards.OldCharacterXP,
					playerStateObject.GivenRewards.CurrentCharacterXP
				});
				if (levelForXP != levelForXP2)
				{
					this.WriteCharacterLevelUpBI(playerStateObject.GivenRewards, levelForXP2, playerStateObject.Data);
				}
				Mission[] missions = GameHubBehaviour.Hub.SharedConfigs.Battlepass.Mission.Missions;
				for (int i = 0; i < playerStateObject.GivenRewards.MissionsCompleted.Length; i++)
				{
					Mission missionProgress = missions[playerStateObject.GivenRewards.MissionsCompleted[i].MissionIndex];
					int objectiveCompletedIndex = playerStateObject.GivenRewards.MissionsCompleted[i].ObjectiveCompletedIndex;
					MatchLogWriter.WriteBattlepassMissionComplete(playerStateObject.Data, levelForXp, levelForXp2, missionProgress, objectiveCompletedIndex);
				}
				MatchLogWriter.WriteBattlePassMatchExperience(playerStateObject.Data, playerStateObject.GivenRewards);
				this.ClearMatchAndGiveRewards(playerStateObject.Data, playerStateObject.GivenRewards, playerStateObject.PendingFuture);
				if (playerStateObject.PendingFuture != null)
				{
					playerStateObject.PendingFuture.Result = 0;
				}
				return;
			}
			ServerRelay.Log.ErrorFormat("OnUpdateRewardsSuccess NetResultFailed. Error={0} Msg={1}", new object[]
			{
				netResult.Error,
				netResult.Msg
			});
			this.ClearMatchAndGiveRewards(playerStateObject.Data, playerStateObject.GivenRewards, playerStateObject.PendingFuture);
			if (playerStateObject.PendingFuture != null)
			{
				playerStateObject.PendingFuture.Result = 0;
			}
		}

		private void WriteCharacterLevelUpBI(RewardsBag givenRewards, int currentCharactersLevel, PlayerData player)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("MatchID={0}", GameHubBehaviour.Hub.Swordfish.Connection.ServerMatchId);
			stringBuilder.AppendFormat(" SteamID={0}", player.UserSF.UniversalID);
			stringBuilder.AppendFormat(" EventAt={0}", DateTime.UtcNow);
			stringBuilder.AppendFormat(" CharacterID={0}", givenRewards.CharacterId);
			stringBuilder.AppendFormat(" NewLevel={0}", currentCharactersLevel);
			GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(11, stringBuilder.ToString(), false);
		}

		private void WriteLevelUpBI(int currentBattlepassLevel, PlayerData player)
		{
			MatchLogWriter.WriteBattlepassLevelUp(player, currentBattlepassLevel);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("MatchID={0}", GameHubBehaviour.Hub.Swordfish.Connection.ServerMatchId);
			stringBuilder.AppendFormat(" SteamID={0}", player.UserSF.UniversalID);
			stringBuilder.AppendFormat(" EventAt={0}", DateTime.UtcNow);
			stringBuilder.AppendFormat(" NewLevel={0}", player.Bag.Level + currentBattlepassLevel);
			GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(10, stringBuilder.ToString(), false);
		}

		private RewardsBag GetRewardsFor(PlayerData player, int xpByTime, TeamKind winner, int founderBonus)
		{
			ServerRelay.Log.DebugFormat("Getting rewards for player={0} id={1}", new object[]
			{
				player.Name,
				player.PlayerId
			});
			RewardsInfo rewards = GameHubBehaviour.Hub.Server.Rewards;
			RewardsBag rewardsBag = new RewardsBag();
			rewardsBag.PlayerId = player.PlayerId;
			rewardsBag.UserID = player.UserSF.Id;
			rewardsBag.CharacterId = player.Character.CharacterItemTypeGuid;
			rewardsBag.SynchTime = GameHubBehaviour.Hub.GameTime.GetSynchTime();
			rewardsBag.RegionId = GameHubBehaviour.Hub.Swordfish.Connection.GetRegionId();
			int count = player.BattlepassProgress.MissionProgresses.Count;
			rewardsBag.OldMissionProgresses = new MissionProgress[count];
			for (int i = 0; i < count; i++)
			{
				rewardsBag.OldMissionProgresses[i] = player.BattlepassProgress.MissionProgresses[i].Clone();
			}
			for (int j = 0; j < rewardsBag.OldMissionProgresses.Length; j++)
			{
				rewardsBag.OldMissionProgresses[j].CurrentProgressValue = new MissionProgressValue[player.BattlepassProgress.MissionProgresses[j].CurrentProgressValue.Length];
				for (int k = 0; k < player.BattlepassProgress.MissionProgresses[j].CurrentProgressValue.Length; k++)
				{
					rewardsBag.OldMissionProgresses[j].CurrentProgressValue[k] = new MissionProgressValue();
					rewardsBag.OldMissionProgresses[j].CurrentProgressValue[k].CurrentValue = player.BattlepassProgress.MissionProgresses[j].CurrentProgressValue[k].CurrentValue;
				}
			}
			rewardsBag.MissionProgresses = new MissionProgress[count];
			for (int l = 0; l < count; l++)
			{
				rewardsBag.MissionProgresses[l] = player.BattlepassProgress.MissionProgresses[l].Clone();
			}
			rewardsBag.MissionsCompleted = new MissionCompleted[0];
			rewardsBag.OldBattlepassXP = player.BattlepassProgress.CurrentXp;
			rewardsBag.CurrentBattlepassXP = player.BattlepassProgress.CurrentXp;
			rewardsBag.SoftByMatchResult = 0;
			if (GameHubBehaviour.Hub.Match.Kind == 2)
			{
				ServerRelay.Log.InfoFormat("[Progression] [PlayerId={0}] MatchKind is {1}. Player will not receive any rewards.", new object[]
				{
					player.PlayerId,
					GameHubBehaviour.Hub.Match.Kind
				});
				return rewardsBag;
			}
			bool flag = GameHubBehaviour.Hub.Match.State == MatchData.MatchState.MatchOverTie || this._afkManager.CheckLeaver(player.PlayerAddress, player.PlayerId, player.UserSF.PublisherUserId);
			if (flag)
			{
				ServerRelay.Log.InfoFormat("[Progression] [PlayerId={0}] Player was AFK. He will not receive any rewards.", new object[]
				{
					player.PlayerId
				});
				rewardsBag.Abandoned = true;
				return rewardsBag;
			}
			PlayerStats bitComponent = player.CharacterInstance.GetBitComponent<PlayerStats>();
			bitComponent.MatchWon = (player.Team == winner);
			rewards.SetPerformance(rewardsBag, bitComponent, GameHubBehaviour.Hub.MatchHistory);
			if (GameHubBehaviour.Hub.Match.Kind == 4 || GameHubBehaviour.Hub.Match.Kind == 6)
			{
				ServerRelay.Log.InfoFormat("[Progression] [PlayerId={0}] MatchKind is {1}. Player will not receive any rewards.", new object[]
				{
					player.PlayerId,
					GameHubBehaviour.Hub.Match.Kind
				});
				return rewardsBag;
			}
			rewardsBag.XpByMatchtime = xpByTime;
			if (GameHubBehaviour.Hub.Match.Kind == 1)
			{
				float num = 1f;
				for (int m = 0; m < rewards.VsBotMultipliers.Length; m++)
				{
					if (rewards.VsBotMultipliers[m].MaxLevel >= player.Level)
					{
						num = rewards.VsBotMultipliers[m].Multiplier;
						break;
					}
				}
				rewardsBag.XpByMatchtime = Mathf.RoundToInt(num * (float)xpByTime);
			}
			rewardsBag.XpByWin = ((!bitComponent.MatchWon) ? 0 : Mathf.RoundToInt(rewards.WinXpBonus * (float)xpByTime));
			float num2 = Mathf.Max(Mathf.Max(rewardsBag.DamageDone, rewardsBag.RepairDone), Mathf.Max(rewardsBag.BombTime, rewardsBag.DebuffTime));
			int num3 = 0;
			for (int n = 0; n < rewards.PerformanceRewards.Length; n++)
			{
				RewardsInfo.PerformanceXpReward performanceXpReward = rewards.PerformanceRewards[n];
				if (num2 >= performanceXpReward.MinPerformance && performanceXpReward.XpBonusAmount > num3)
				{
					num3 = performanceXpReward.XpBonusAmount;
				}
			}
			rewardsBag.XpByPerformance = num3;
			rewardsBag.XpByFounderBonus = Mathf.RoundToInt((float)(rewardsBag.GetBaseXp() * founderBonus) / 100f);
			if (GameHubBehaviour.Hub.Match.Kind == null || GameHubBehaviour.Hub.Match.Kind == 7 || GameHubBehaviour.Hub.Match.Kind == 5 || GameHubBehaviour.Hub.Match.Kind == 3 || GameHubBehaviour.Hub.Match.Kind == 1)
			{
				this._battlepassMissionValidator.ValidateMissionCompletion(GameHubBehaviour.Hub.SharedConfigs.Battlepass.Mission.Missions, player.BattlepassProgress, bitComponent, GameHubBehaviour.Hub.MatchHistory, GameHubBehaviour.Hub.Players, GameHubBehaviour.Hub.InventoryColletion);
				rewardsBag.MissionProgresses = new MissionProgress[player.BattlepassProgress.MissionProgresses.Count];
				for (int num4 = 0; num4 < player.BattlepassProgress.MissionProgresses.Count; num4++)
				{
					MissionProgress missionProgress = new MissionProgress();
					missionProgress.MissionIndex = player.BattlepassProgress.MissionProgresses[num4].MissionIndex;
					missionProgress.Seen = player.BattlepassProgress.MissionProgresses[num4].Seen;
					rewardsBag.MissionProgresses[num4] = missionProgress;
				}
				for (int num5 = 0; num5 < rewardsBag.MissionProgresses.Length; num5++)
				{
					rewardsBag.MissionProgresses[num5].CurrentProgressValue = new MissionProgressValue[player.BattlepassProgress.MissionProgresses[num5].CurrentProgressValue.Length];
					for (int num6 = 0; num6 < player.BattlepassProgress.MissionProgresses[num5].CurrentProgressValue.Length; num6++)
					{
						rewardsBag.MissionProgresses[num5].CurrentProgressValue[num6] = new MissionProgressValue();
						rewardsBag.MissionProgresses[num5].CurrentProgressValue[num6].CurrentValue = player.BattlepassProgress.MissionProgresses[num5].CurrentProgressValue[num6].CurrentValue;
					}
				}
				rewardsBag.MissionsCompleted = new MissionCompleted[bitComponent.MissionsCompletedIndex.Count];
				for (int num7 = 0; num7 < bitComponent.MissionsCompletedIndex.Count; num7++)
				{
					rewardsBag.MissionsCompleted[num7] = bitComponent.MissionsCompletedIndex[num7];
				}
			}
			rewardsBag.SoftByMatchResult = this.GetSoftCoinReward(bitComponent.MatchWon, GameHubBehaviour.Hub.Match.Kind);
			ServerRelay.Log.InfoFormat("Rewards for={0} set={1}", new object[]
			{
				player.Name,
				rewardsBag
			});
			return rewardsBag;
		}

		private int GetSoftCoinReward(bool matchWin, MatchKind matchKind)
		{
			switch (matchKind)
			{
			case 0:
			case 7:
				return (!matchWin) ? GameHubBehaviour.Hub.Server.Rewards.PVPLoseScrapAmount : GameHubBehaviour.Hub.Server.Rewards.PVPWinScrapAmount;
			case 1:
				return (!matchWin) ? GameHubBehaviour.Hub.Server.Rewards.PVELoseScrapAmount : GameHubBehaviour.Hub.Server.Rewards.PVEWinScrapAmount;
			case 2:
				return 0;
			case 3:
				return (!matchWin) ? GameHubBehaviour.Hub.Server.Rewards.RankedLoseScrapAmount : GameHubBehaviour.Hub.Server.Rewards.RankedWinScrapAmount;
			case 4:
				return 0;
			case 5:
				return (!matchWin) ? GameHubBehaviour.Hub.Server.Rewards.TournamentLoseScrapAmount : GameHubBehaviour.Hub.Server.Rewards.TournamentWinScrapAmount;
			}
			ServerRelay.Log.WarnFormat("Match kind {0} not found, return 0 to soft coin earned", new object[]
			{
				matchKind
			});
			return 0;
		}

		private void ClearMatchAndGiveRewards(PlayerData player, RewardsBag rewards, Future final)
		{
			if (!GameHubBehaviour.Hub.Swordfish.Connection.Connected)
			{
				return;
			}
			Future rewardsGiven = new Future
			{
				Name = "GameOverActions_RewardsGiven"
			};
			if (final != null)
			{
				final.DependsOn(rewardsGiven);
			}
			ServerPlayer.ServerClearBagAndGiveRewards(player, delegate(NetResult result)
			{
				if (player.IsNarrator)
				{
					ServerRelay.Log.DebugFormat("[PlayerId={0}] Bag cleared={1} for narrator", new object[]
					{
						player.PlayerId,
						result
					});
					rewardsGiven.Result = 0;
				}
				else
				{
					ServerRelay.Log.DebugFormat("[Progression] [PlayerId={0}] Bag cleared={1} rewards given={2}", new object[]
					{
						player.PlayerId,
						result,
						rewards.ToString()
					});
				}
				if (this._matchNeverStarted)
				{
					ServerRelay.Log.WarnFormat("Match never started, skipping BI and level up routines for player={0}", new object[]
					{
						player.PlayerId
					});
					rewardsGiven.Result = 0;
					return;
				}
				if (player.Connected)
				{
					GameHubBehaviour.Hub.Server.DispatchReliable(new byte[]
					{
						player.PlayerAddress
					}).SetPlayerRewards(rewards.ToString());
				}
				if (player.IsNarrator)
				{
					return;
				}
				PlayerStats stats = GameHubBehaviour.Hub.ScrapBank.PlayerAccounts[player.PlayerCarId];
				RewardsInfo.Medals performanceMedal = (RewardsInfo.Medals)RewardsInfo.GetPerformanceMedal(rewards.DamageDone);
				RewardsInfo.Medals performanceMedal2 = (RewardsInfo.Medals)RewardsInfo.GetPerformanceMedal(rewards.RepairDone);
				RewardsInfo.Medals performanceMedal3 = (RewardsInfo.Medals)RewardsInfo.GetPerformanceMedal(rewards.BombTime);
				RewardsInfo.Medals performanceMedal4 = (RewardsInfo.Medals)RewardsInfo.GetPerformanceMedal(rewards.DebuffTime);
				MatchLogWriter.PlayerPerformanceMatchLog(player, stats, rewards, performanceMedal, performanceMedal2, performanceMedal3, performanceMedal4);
				MatchLogWriter.WritePlayerEndGameRewards(player, GameHubBehaviour.Hub.Match.Kind.ToString(), rewards.GetTotalMatchSoft(), rewards.GetMatchWon(), rewards.Abandoned);
				rewardsGiven.Result = 0;
			});
		}

		private void UpdateMMR(PlayerData player, IFuture characterUpdate, IFuture final)
		{
			Future future = new Future
			{
				Name = "GameOverActions_MMR"
			};
			final.DependsOn(future);
			ServerRelay.MMRUpdateStateObject state = new ServerRelay.MMRUpdateStateObject
			{
				PendingFuture = future,
				Player = player
			};
			characterUpdate.WhenDone(delegate(IFuture x)
			{
				this.UpdateUserMMR(state);
			});
		}

		private void UpdateTeamOnTournament(IFuture tournamentUpdate, TeamKind winner)
		{
			string universalID = GameHubBehaviour.Hub.Players.RedTeamPlayersAndBots[0].UserSF.UniversalID;
			string universalID2 = GameHubBehaviour.Hub.Players.BlueTeamPlayersAndBots[0].UserSF.UniversalID;
			int num = Mathf.CeilToInt(GameHubBehaviour.Hub.MatchHistory.GetMatchTimeSeconds() * 1000f);
			IFuture item = this.tournamentTeamSkillsUpdater.UpdateTeamSkills(universalID, universalID2, winner, GameHubBehaviour.Hub.Swordfish.Connection.TournamentStepId, this.GetTeamDTO((long)num, TeamKind.Red), this.GetTeamDTO((long)num, TeamKind.Blue));
			tournamentUpdate.DependsOn(item);
			tournamentUpdate.Result = 0;
		}

		private void GetTeamFail(object state, Exception exception)
		{
			ServerRelay.TeamUpdateStateObject teamUpdateStateObject = (ServerRelay.TeamUpdateStateObject)state;
			ServerRelay.Log.ErrorFormat("GetTeamFail. Team={0} Exception={1}", new object[]
			{
				teamUpdateStateObject.kind,
				exception
			});
			teamUpdateStateObject.PendingFuture.Result = 0;
		}

		private TeamStatsDTO GetTeamDTO(long matchTime, TeamKind kind)
		{
			TeamStatsDTO teamStatsDTO = new TeamStatsDTO();
			teamStatsDTO.MatchTime = matchTime;
			teamStatsDTO.MatchEndTime = DateTime.UtcNow.Ticks;
			if (kind != TeamKind.Red)
			{
				if (kind == TeamKind.Blue)
				{
					teamStatsDTO.goalsMade = GameHubBehaviour.Hub.MatchHistory.GetDeliveriesBlue();
					teamStatsDTO.goalsTaken = GameHubBehaviour.Hub.MatchHistory.GetDeliveriesRed();
				}
			}
			else
			{
				teamStatsDTO.goalsMade = GameHubBehaviour.Hub.MatchHistory.GetDeliveriesRed();
				teamStatsDTO.goalsTaken = GameHubBehaviour.Hub.MatchHistory.GetDeliveriesBlue();
			}
			return teamStatsDTO;
		}

		private void UpdateUserMMR(ServerRelay.MMRUpdateStateObject state)
		{
			GameHubBehaviour.Hub.ClientApi.character.GetAllCharacters(state, state.Player.PlayerId, new SwordfishClientApi.ParameterizedCallback<Character[]>(this.OnCharactersTakenForMMR), new SwordfishClientApi.ErrorCallback(this.OnCharactersTakenForMMRError));
		}

		private void OnCharactersTakenForMMRError(object objstate, Exception exception)
		{
			ServerRelay.MMRUpdateStateObject mmrupdateStateObject = (ServerRelay.MMRUpdateStateObject)objstate;
			ServerRelay.Log.Fatal(string.Format("[GameOverActions] Update User MMR failed for user={0} characters not loaded", mmrupdateStateObject.Player.UserSF.UniversalID), exception);
			mmrupdateStateObject.PendingFuture.Result = -1;
		}

		private void OnCharactersTakenForMMR(object objstate, Character[] chars)
		{
			ServerRelay.MMRUpdateStateObject mmrupdateStateObject = (ServerRelay.MMRUpdateStateObject)objstate;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < chars.Length; i++)
			{
				CharacterBag characterBag = (CharacterBag)((JsonSerializeable<!0>)chars[i].Bag);
				num += characterBag.WinsCount;
				num2 += characterBag.MatchesCount;
			}
			mmrupdateStateObject.Wins = num;
			mmrupdateStateObject.Games = num2;
			mmrupdateStateObject.MMR = (int)(1000f * (float)num / (float)(GameHubBehaviour.Hub.SharedConfigs.MMRFactor + num2));
			string universalID = mmrupdateStateObject.Player.UserSF.UniversalID;
			GameHubBehaviour.Hub.ClientApi.matchmaking.SetMMR(mmrupdateStateObject, universalID, (double)mmrupdateStateObject.MMR, new SwordfishClientApi.Callback(this.OnMMRUpdated), new SwordfishClientApi.ErrorCallback(this.OnMMRUpdateFailed));
		}

		private void OnMMRUpdateFailed(object objstate, Exception exception)
		{
			ServerRelay.MMRUpdateStateObject mmrupdateStateObject = (ServerRelay.MMRUpdateStateObject)objstate;
			ServerRelay.Log.Fatal(string.Format("[GameOverActions] Update MMR failed for User={0} mmr should be={1} wins={2} games={3}", new object[]
			{
				mmrupdateStateObject.Player.UserSF.UniversalID,
				mmrupdateStateObject.MMR,
				mmrupdateStateObject.Wins,
				mmrupdateStateObject.Games
			}), exception);
			mmrupdateStateObject.PendingFuture.Result = -2;
		}

		private void OnMMRUpdated(object objstate)
		{
			ServerRelay.MMRUpdateStateObject mmrupdateStateObject = (ServerRelay.MMRUpdateStateObject)objstate;
			ServerRelay.Log.DebugFormat("[GameOverActions] Update MMR for User={0} value={1} wins={2} games={3} factor={4}", new object[]
			{
				mmrupdateStateObject.Player.UserSF.UniversalID,
				mmrupdateStateObject.MMR,
				mmrupdateStateObject.Wins,
				mmrupdateStateObject.Games,
				GameHubBehaviour.Hub.SharedConfigs.MMRFactor
			});
			mmrupdateStateObject.PendingFuture.Result = 0;
		}

		private void OnStatsUpdatedSuccess(object state, string result)
		{
			ServerRelay.PlayerStateObject playerStateObject = (ServerRelay.PlayerStateObject)state;
			NetResult netResult = (NetResult)((JsonSerializeable<!0>)result);
			RewardsBag givenRewards = (RewardsBag)((JsonSerializeable<!0>)netResult.Msg);
			ServerRelay.Log.DebugFormat("Update successfull for={0}", new object[]
			{
				playerStateObject.Data.PlayerId
			});
			playerStateObject.GivenRewards = givenRewards;
			if (playerStateObject.PendingFuture != null)
			{
				playerStateObject.PendingFuture.Result = 0;
			}
		}

		private void OnStatsUpdatedFail(object state, Exception e)
		{
			ServerRelay.PlayerStateObject playerStateObject = (ServerRelay.PlayerStateObject)state;
			ServerRelay.Log.ErrorFormat("Update failed Exception={1}", new object[]
			{
				playerStateObject.Data.PlayerId,
				e
			});
			if (playerStateObject.PendingFuture != null)
			{
				playerStateObject.PendingFuture.Result = -1;
			}
		}

		private void UpdatePlayersMatchHistory(PlayerData playerData, RewardsBag rewards, TeamKind winnerTeam)
		{
			float performance = Mathf.Max(Mathf.Max(rewards.DamageDone, rewards.RepairDone), Mathf.Max(rewards.BombTime, rewards.DebuffTime));
			MatchHistoryItemBag matchHistoryItemBag = new MatchHistoryItemBag();
			matchHistoryItemBag.PlayerId = playerData.PlayerId;
			matchHistoryItemBag.CharacterId = playerData.CharacterId;
			matchHistoryItemBag.SkinId = playerData.Customizations.GetGuidBySlot(59).ToString();
			matchHistoryItemBag.SetDate(this._currentTime.NowServerUtc());
			matchHistoryItemBag.GameMode = GameHubBehaviour.Hub.Match.Kind;
			matchHistoryItemBag.BestPerformanceMedal = RewardsInfo.GetPerformanceMedal(performance);
			matchHistoryItemBag.MatchResult = ((playerData.Team != winnerTeam) ? 0 : 1);
			matchHistoryItemBag.Abandoned = rewards.Abandoned;
			if (rewards.Abandoned)
			{
				matchHistoryItemBag.MatchResult = 0;
			}
			matchHistoryItemBag.ArenaIndex = GameHubBehaviour.Hub.Match.ArenaIndex;
			matchHistoryItemBag.MatchId = GameHubBehaviour.Hub.Swordfish.Connection.ServerMatchId.ToString();
			long[] alliesPlayerIds = this.GetAlliesPlayerIds(playerData).ToArray();
			matchHistoryItemBag.AlliesPlayerIds = alliesPlayerIds;
			long[] enemiesPlayerIds = this.GetEnemiesPlayerIds(playerData).ToArray();
			matchHistoryItemBag.EnemiesPlayerIds = enemiesPlayerIds;
			ServerRelay.Log.DebugFormat("[GameOverActions] Creating match record: {0}", new object[]
			{
				matchHistoryItemBag
			});
			MatchHistoryCustomWS.CreateANewMatch(matchHistoryItemBag, delegate(object s, string o)
			{
				this.OnMatchCreated(playerData, o);
			}, delegate(object s, Exception e)
			{
				this.OnOnMatchCreationFailed(playerData, e);
			});
		}

		private void UpdateRemainingNoviceTrials(PlayerData playerData, MatchKind matchKind, RewardsBag reward)
		{
			if (reward.Abandoned || matchKind == 2)
			{
				return;
			}
			ObservableExtensions.Subscribe<Unit>(this._updateNoviceTrialsRemaining.Update(playerData.PlayerId));
		}

		private List<long> GetAlliesPlayerIds(PlayerData playerData)
		{
			List<long> list = new List<long>();
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData2 = GameHubBehaviour.Hub.Players.Players[i];
				if (playerData.PlayerId != playerData2.PlayerId && playerData.Team == playerData2.Team)
				{
					list.Add(playerData2.PlayerId);
				}
			}
			return list;
		}

		private List<long> GetEnemiesPlayerIds(PlayerData playerData)
		{
			List<long> list = new List<long>();
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData2 = GameHubBehaviour.Hub.Players.Players[i];
				if (playerData.PlayerId != playerData2.PlayerId && playerData.Team != playerData2.Team)
				{
					list.Add(playerData2.PlayerId);
				}
			}
			return list;
		}

		private void OnMatchCreated(PlayerData playerData, string result)
		{
			ServerRelay.Log.DebugFormat("[GameOverActions] Match Created for={0} Result={1}", new object[]
			{
				playerData.PlayerId,
				result
			});
		}

		private void OnOnMatchCreationFailed(PlayerData playerData, Exception exception)
		{
			ServerRelay.Log.ErrorFormat("[GameOverActions] Match Creation failed for={0} Exception={1}", new object[]
			{
				playerData.PlayerId,
				exception
			});
		}

		private TeamKind GetWinnerTeam(MatchData.MatchState matchwinner)
		{
			if (matchwinner == MatchData.MatchState.MatchOverBluWins)
			{
				return TeamKind.Blue;
			}
			if (matchwinner != MatchData.MatchState.MatchOverRedWins)
			{
				return TeamKind.Zero;
			}
			return TeamKind.Red;
		}

		public void OnClientReconnect(ClientReconnectMessage msg)
		{
			byte connectionId = msg.Session.ConnectionId;
			GameHubBehaviour.Hub.Server.DispatchReliable(new byte[]
			{
				connectionId
			}).SetInfo(GameHubBehaviour.Hub.Match);
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(connectionId);
			if (playerByAddress == null)
			{
				ServerRelay.Log.ErrorFormat("A player with address {0} could not be found. Ignoring reconnect.", new object[]
				{
					connectionId
				});
				return;
			}
			playerByAddress.Connected = true;
			ServerRelay.Log.InfoFormat("OnClientReconnect player={0}, connectionId={1}", new object[]
			{
				playerByAddress,
				connectionId
			});
			this._teamsDispatcher.SendTeams(connectionId);
			this._playersDispatcher.UpdatePlayers();
			if (playerByAddress.IsNarrator)
			{
				AnnouncerEvent content = new AnnouncerEvent
				{
					AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.SpectatorConnected,
					Killer = (int)playerByAddress.PlayerAddress
				};
				GameHubBehaviour.Hub.Events.TriggerEvent(content);
				return;
			}
			Identifiable characterInstance = playerByAddress.CharacterInstance;
			if (characterInstance == null)
			{
				return;
			}
			PlayerStats component = characterInstance.GetComponent<PlayerStats>();
			if (component != null)
			{
				component.OnPlayerReconnected();
			}
			AnnouncerEvent content2 = new AnnouncerEvent
			{
				AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.PlayerReconnected,
				Killer = characterInstance.ObjId
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content2);
		}

		public void OnClientDisconnect(ClientDisconnectMessage msg)
		{
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(msg.Session.ConnectionId);
			if (playerByAddress == null)
			{
				return;
			}
			playerByAddress.Connected = false;
			ServerRelay.Log.InfoFormat("OnClientDisconnect player={0}, connectionId={1}", new object[]
			{
				playerByAddress,
				msg.Session.ConnectionId
			});
			Identifiable characterInstance = playerByAddress.CharacterInstance;
			playerByAddress.RemoveOnline();
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial() || GameHubBehaviour.Hub.Match.Kind == 6)
			{
				ServerPlayer.SetCurrentServer(playerByAddress.PlayerId, 0, null, null, null, false, new Action<NetResult>(this.ClearBagOnDisconnectCallback));
			}
			this._playersDispatcher.UpdatePlayers();
			if (characterInstance == null)
			{
				return;
			}
			PlayerStats component = characterInstance.GetComponent<PlayerStats>();
			if (component != null)
			{
				component.OnPlayerDisconnected();
			}
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.PlayerDisconnected,
				Killer = characterInstance.ObjId
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		private void ClearBagOnDisconnectCallback(NetResult obj)
		{
			ServerRelay.Log.DebugFormat("[GameOverActions] ClearBagOnDisconnectCallback result: {0}", new object[]
			{
				obj
			});
		}

		public void OnTerminateServer(TerminateServerMessage msg)
		{
			((NetworkServer)GameHubBehaviour.Hub.Net).StopServer();
			ServerRelay.Log.Debug("OnterminateServer");
			if (GameHubBehaviour.Hub.MatchMan == null)
			{
				Mural.PostAll(new MatchController.GameOverMessage
				{
					State = MatchData.MatchState.Nothing
				}, typeof(MatchController.GameOverMessage.IGameOverListener));
				return;
			}
			GameHubBehaviour.Hub.MatchMan.EndMatch(TeamKind.Zero);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ServerRelay));

		private BattlepassMissionValidator _battlepassMissionValidator = new BattlepassMissionValidator();

		private TournamentTeamSkillsUpdater tournamentTeamSkillsUpdater = new TournamentTeamSkillsUpdater();

		[InjectOnServer]
		private DiContainer _diContainer;

		[Inject]
		private IMatchPlayersDispatcher _playersDispatcher;

		[Inject]
		private IMatchTeamsDispatcher _teamsDispatcher;

		[Inject]
		private ICurrentTime _currentTime;

		[InjectOnServer]
		private IGetBotDifficulty _getBotDifficulty;

		[InjectOnServer]
		private IAFKManager _afkManager;

		[InjectOnServer]
		private IUpdateNoviceTrialsRemaining _updateNoviceTrialsRemaining;

		private bool _matchNeverStarted;

		private bool _coroutineRunning;

		public class PerformanceProjections
		{
			public float DamageDone;

			public float RepairDone;

			public float BombTime;

			public float DebuffTime;
		}

		private struct MMRUpdateStateObject
		{
			public PlayerData Player;

			public IFuture PendingFuture;

			public int MMR;

			public int Wins;

			public int Games;
		}

		private struct TeamUpdateStateObject
		{
			public IFuture PendingFuture;

			public TeamKind kind;

			public int MatchTime;
		}

		private struct PlayerStateObject
		{
			public PlayerData Data;

			public Future PendingFuture;

			public RewardsBag GivenRewards;
		}
	}
}
