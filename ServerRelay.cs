using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ClientAPI;
using ClientAPI.Objects;
using Commons.Swordfish.Battlepass;
using Commons.Swordfish.Progression;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Battlepass;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using UnityEngine;

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
			yield return 0;
			MatchData.MatchState state = msg.State;
			if (state == MatchData.MatchState.Nothing || state == MatchData.MatchState.AwaitingConnections || state == MatchData.MatchState.CharacterPick)
			{
				this._matchNeverStarted = true;
			}
			Future final = new Future();
			final.Name = "Final";
			final.WhenDone(delegate(IFuture x)
			{
				MatchLogWriter.WriteBotsDifficulty();
				MatchLogWriter.WritePlayerExperience();
				MatchLogWriter.MatchEnd(msg.State);
				GameHubBehaviour.Hub.Quit();
			});
			TeamKind winner = this.GetWinnerTeam(msg.State);
			RewardsInfo info = GameHubBehaviour.Hub.Server.Rewards;
			float matchTime = GameHubBehaviour.Hub.GameTime.MatchTimer.GetTimeSeconds();
			int xpByTime = info.GetRewardsByMatchTime(matchTime);
			int founderBonus = 0;
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.Players[i];
				PlayerBag bag = playerData.Bag;
				byte founderPackLevel = bag.FounderPackLevel;
				FounderLevel level = (FounderLevel)founderPackLevel;
				if (level.CheckHasFlag(FounderLevel.Gold))
				{
					founderBonus += GameHubBehaviour.Hub.SharedConfigs.FounderPackConfig.FounderPackGoldXpBonus;
				}
				else if (level.CheckHasFlag(FounderLevel.Silver))
				{
					founderBonus += GameHubBehaviour.Hub.SharedConfigs.FounderPackConfig.FounderPackSilverXpBonus;
				}
				else if (level.CheckHasFlag(FounderLevel.Bronze))
				{
					founderBonus += GameHubBehaviour.Hub.SharedConfigs.FounderPackConfig.FounderPackBronzeXpBonus;
				}
			}
			for (int j = 0; j < GameHubBehaviour.Hub.Players.Clients.Count; j++)
			{
				PlayerData playerData2 = GameHubBehaviour.Hub.Players.Clients[j];
				if (this._matchNeverStarted || playerData2.IsNarrator)
				{
					this.ClearMatchAndGiveRewards(playerData2, new RewardsBag
					{
						OldMissionProgresses = new MissionProgress[0],
						MissionProgresses = new MissionProgress[0],
						MissionsCompleted = new int[0]
					}, final);
				}
				else if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
				{
					RewardsBag rewardsBag = new RewardsBag();
					string value = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SkipSFMissionConfigHack);
					if (string.IsNullOrEmpty(value))
					{
						rewardsBag.OldMissionProgresses = new MissionProgress[0];
						rewardsBag.MissionProgresses = new MissionProgress[0];
						rewardsBag.MissionsCompleted = new int[0];
					}
					else
					{
						string[] split = value.Split(new char[]
						{
							','
						});
						this.FakeValidateMissionByConfigAccessParams(playerData2, rewardsBag, split, this.GetWinnerTeam(msg.State));
					}
					GameHubBehaviour.Hub.Server.Dispatch(new byte[]
					{
						playerData2.PlayerAddress
					}).SetPlayerRewards(rewardsBag.ToString());
				}
				else
				{
					RewardsBag rewardsFor = this.GetRewardsFor(playerData2, xpByTime, winner, founderBonus);
					Future future = new Future();
					future.Name = "Reward";
					final.DependsOn(future);
					switch (GameHubBehaviour.Hub.Match.Kind)
					{
					case MatchData.MatchKind.PvP:
					case MatchData.MatchKind.Ranked:
						this.UpdatePlayersMatchHistory(playerData2, rewardsFor, winner);
						this.UpdateMMR(playerData2, future, final);
						this.UpdateRewards(playerData2, future, rewardsFor, false);
						break;
					case MatchData.MatchKind.PvE:
					case MatchData.MatchKind.Custom:
						this.UpdatePlayersMatchHistory(playerData2, rewardsFor, winner);
						this.UpdateRewards(playerData2, future, rewardsFor, true);
						break;
					case MatchData.MatchKind.Tutorial:
						future.Result = rewardsFor;
						break;
					}
				}
			}
			for (int k = 0; k < GameHubBehaviour.Hub.Players.Narrators.Count; k++)
			{
				PlayerData player = GameHubBehaviour.Hub.Players.Narrators[k];
				this.ClearMatchAndGiveRewards(player, new RewardsBag(), final);
			}
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime((float)GameHubBehaviour.Hub.SharedConfigs.TimeOutSecondsToCloseServer));
			final.Result = 0;
			yield break;
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
					MissionIndex = int.Parse(split[i]),
					CurrentValue = 0f
				};
				player.BattlepassProgress.MissionProgresses.Add(missionProgress);
				reward.OldMissionProgresses[i] = missionProgress.Clone();
			}
			PlayerStats bitComponent = player.CharacterInstance.GetBitComponent<PlayerStats>();
			bitComponent.MatchWon = (player.Team == winner);
			GameHubBehaviour.Hub.Server.Rewards.SetPerformance(reward, bitComponent, GameHubBehaviour.Hub.MatchHistory);
			this._battlepassMissionValidator.ValidateMissionCompletion(player.BattlepassProgress, bitComponent, GameHubBehaviour.Hub.MatchHistory);
			reward.MissionProgresses = new MissionProgress[player.BattlepassProgress.MissionProgresses.Count];
			for (int j = 0; j < player.BattlepassProgress.MissionProgresses.Count; j++)
			{
				reward.MissionProgresses[j] = player.BattlepassProgress.MissionProgresses[j].Clone();
			}
			reward.MissionsCompleted = new int[bitComponent.MissionsCompletedIndex.Count];
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
			NetResult netResult = (NetResult)((JsonSerializeable<T>)s);
			if (netResult.Success)
			{
				playerStateObject.GivenRewards = (RewardsBag)((JsonSerializeable<T>)netResult.Msg);
				int levelForXp = GameHubBehaviour.Hub.SharedConfigs.Battlepass.GetLevelForXp(playerStateObject.GivenRewards.OldBattlepassXP);
				int levelForXp2 = GameHubBehaviour.Hub.SharedConfigs.Battlepass.GetLevelForXp(playerStateObject.GivenRewards.CurrentBattlepassXP);
				if (levelForXp != levelForXp2)
				{
					this.WriteLevelUpBI(levelForXp2, playerStateObject.Data);
				}
				int xpforLevel = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetXPForLevel(playerStateObject.GivenRewards.OldCharacterXP);
				int xpforLevel2 = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetXPForLevel(playerStateObject.GivenRewards.CurrentCharacterXP);
				if (xpforLevel != xpforLevel2)
				{
					this.WriteCharacterLevelUpBI(playerStateObject.GivenRewards, xpforLevel2, playerStateObject.Data);
				}
				Commons.Swordfish.Battlepass.Mission[] missions = GameHubBehaviour.Hub.SharedConfigs.Battlepass.Mission.Missions;
				for (int i = 0; i < playerStateObject.GivenRewards.MissionsCompleted.Length; i++)
				{
					Commons.Swordfish.Battlepass.Mission missionProgress = missions[playerStateObject.GivenRewards.MissionsCompleted[i]];
					MatchLogWriter.WriteBattlepassMissionComplete(playerStateObject.Data, levelForXp, levelForXp2, missionProgress);
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
			GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(ServerBITags.GameServerCharacterLevelUp, stringBuilder.ToString(), false);
		}

		private void WriteLevelUpBI(int currentBattlepassLevel, PlayerData player)
		{
			MatchLogWriter.WriteBattlepassLevelUp(player, currentBattlepassLevel);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("MatchID={0}", GameHubBehaviour.Hub.Swordfish.Connection.ServerMatchId);
			stringBuilder.AppendFormat(" SteamID={0}", player.UserSF.UniversalID);
			stringBuilder.AppendFormat(" EventAt={0}", DateTime.UtcNow);
			stringBuilder.AppendFormat(" NewLevel={0}", player.Bag.Level + currentBattlepassLevel);
			GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(ServerBITags.GameServerPlayerLevelUp, stringBuilder.ToString(), false);
		}

		private RewardsBag GetRewardsFor(PlayerData player, int xpByTime, TeamKind winner, int founderBonus)
		{
			RewardsInfo rewards = GameHubBehaviour.Hub.Server.Rewards;
			RewardsBag rewardsBag = new RewardsBag();
			rewardsBag.PlayerId = player.PlayerId;
			rewardsBag.UserID = player.UserSF.Id;
			rewardsBag.CharacterId = player.Character.CharacterItemTypeGuid;
			rewardsBag.SynchTime = GameHubBehaviour.Hub.GameTime.GetSynchTime();
			int count = player.BattlepassProgress.MissionProgresses.Count;
			rewardsBag.OldMissionProgresses = new MissionProgress[count];
			for (int i = 0; i < count; i++)
			{
				rewardsBag.OldMissionProgresses[i] = player.BattlepassProgress.MissionProgresses[i].Clone();
			}
			rewardsBag.MissionProgresses = new MissionProgress[count];
			for (int j = 0; j < count; j++)
			{
				rewardsBag.MissionProgresses[j] = player.BattlepassProgress.MissionProgresses[j].Clone();
			}
			rewardsBag.MissionsCompleted = new int[0];
			rewardsBag.OldBattlepassXP = player.BattlepassProgress.CurrentXp;
			rewardsBag.CurrentBattlepassXP = player.BattlepassProgress.CurrentXp;
			rewardsBag.SoftCoinEarned = 0;
			if (GameHubBehaviour.Hub.Match.Kind == MatchData.MatchKind.Tutorial)
			{
				ServerRelay.Log.InfoFormat("[Progression] [PlayerId={0}] MatchKind is Tutorial. Player will not receive any rewards.", new object[]
				{
					player.PlayerId
				});
				return rewardsBag;
			}
			bool flag = GameHubBehaviour.Hub.afkController.CheckLeaver(player.PlayerAddress, player.PlayerId, player.UserSF.PublisherUserId);
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
			if (GameHubBehaviour.Hub.Match.Kind == MatchData.MatchKind.Custom)
			{
				ServerRelay.Log.InfoFormat("[Progression] [PlayerId={0}] MatchKind is CustomMatch. Player will not receive any rewards.", new object[]
				{
					player.PlayerId
				});
				return rewardsBag;
			}
			rewardsBag.XpByMatchtime = xpByTime;
			if (GameHubBehaviour.Hub.Match.Kind == MatchData.MatchKind.PvE)
			{
				float num = 1f;
				for (int k = 0; k < rewards.VsBotMultipliers.Length; k++)
				{
					if (rewards.VsBotMultipliers[k].MaxLevel >= player.Level)
					{
						num = rewards.VsBotMultipliers[k].Multiplier;
						break;
					}
				}
				rewardsBag.XpByMatchtime = Mathf.RoundToInt(num * (float)xpByTime);
			}
			rewardsBag.XpByWin = ((!bitComponent.MatchWon) ? 0 : Mathf.RoundToInt(rewards.WinXpBonus * (float)xpByTime));
			float num2 = Mathf.Max(Mathf.Max(rewardsBag.DamageDone, rewardsBag.RepairDone), Mathf.Max(rewardsBag.BombTime, rewardsBag.DebuffTime));
			int num3 = 0;
			for (int l = 0; l < rewards.PerformanceRewards.Length; l++)
			{
				RewardsInfo.PerformanceXpReward performanceXpReward = rewards.PerformanceRewards[l];
				if (num2 >= performanceXpReward.MinPerformance && performanceXpReward.XpBonusAmount > num3)
				{
					num3 = performanceXpReward.XpBonusAmount;
				}
			}
			rewardsBag.XpByPerformance = num3;
			rewardsBag.XpByFounderBonus = Mathf.RoundToInt((float)(rewardsBag.GetBaseXp() * founderBonus) / 100f);
			if (GameHubBehaviour.Hub.Match.Kind == MatchData.MatchKind.PvP || GameHubBehaviour.Hub.Match.Kind == MatchData.MatchKind.Ranked || GameHubBehaviour.Hub.Match.Kind == MatchData.MatchKind.PvE)
			{
				this._battlepassMissionValidator.ValidateMissionCompletion(player.BattlepassProgress, bitComponent, GameHubBehaviour.Hub.MatchHistory);
				rewardsBag.BombDeliveredCount = bitComponent.BombsDelivered;
				rewardsBag.KillsCount = bitComponent.KillsAndAssists;
				rewardsBag.DeathsCount = bitComponent.Deaths;
				rewardsBag.TotalDamage = (int)bitComponent.DamageDealtToPlayers;
				rewardsBag.TotalRepair = (int)bitComponent.HealingProvided;
				rewardsBag.TravelledDistance = (int)bitComponent.TravelledDistance / 1000;
				rewardsBag.SpeedBoostCount = bitComponent.BoostGadgetCount;
				rewardsBag.BombLostCount = bitComponent.BombLostDeathCount;
				rewardsBag.BombStolenCount = bitComponent.BombTakenCount;
				rewardsBag.TotalBombPossession = (int)bitComponent.BombPossessionTime;
				rewardsBag.TotalDebuffTime = (int)bitComponent.DebuffTime;
				rewardsBag.TotalTimePlayed = GameHubBehaviour.Hub.GameTime.MatchTimer.GetTime() / 1000;
				rewardsBag.MissionProgresses = new MissionProgress[player.BattlepassProgress.MissionProgresses.Count];
				for (int m = 0; m < player.BattlepassProgress.MissionProgresses.Count; m++)
				{
					rewardsBag.MissionProgresses[m] = player.BattlepassProgress.MissionProgresses[m].Clone();
				}
				rewardsBag.MissionsCompleted = new int[bitComponent.MissionsCompletedIndex.Count];
				for (int n = 0; n < bitComponent.MissionsCompletedIndex.Count; n++)
				{
					rewardsBag.MissionsCompleted[n] = bitComponent.MissionsCompletedIndex[n];
				}
			}
			rewardsBag.SoftCoinEarned = this.GetSoftCoinReward(bitComponent.MatchWon, GameHubBehaviour.Hub.Match.Kind);
			ServerRelay.Log.InfoFormat("Rewards for={0} set={1}", new object[]
			{
				player.Name,
				rewardsBag
			});
			return rewardsBag;
		}

		private int GetSoftCoinReward(bool matchWin, MatchData.MatchKind matchKind)
		{
			switch (matchKind)
			{
			case MatchData.MatchKind.PvP:
				return (!matchWin) ? GameHubBehaviour.Hub.Server.Rewards.PVPLoseScrapAmount : GameHubBehaviour.Hub.Server.Rewards.PVPWinScrapAmount;
			case MatchData.MatchKind.PvE:
				return (!matchWin) ? GameHubBehaviour.Hub.Server.Rewards.PVELoseScrapAmount : GameHubBehaviour.Hub.Server.Rewards.PVEWinScrapAmount;
			case MatchData.MatchKind.Tutorial:
				return 0;
			case MatchData.MatchKind.Ranked:
				return (!matchWin) ? GameHubBehaviour.Hub.Server.Rewards.RankedLoseScrapAmount : GameHubBehaviour.Hub.Server.Rewards.RankedWinScrapAmount;
			case MatchData.MatchKind.Custom:
				return 0;
			default:
				ServerRelay.Log.WarnFormat("Match kind {0} not found, return 0 to soft coin earned", new object[]
				{
					matchKind
				});
				return 0;
			}
		}

		private void ClearMatchAndGiveRewards(PlayerData player, RewardsBag rewards, Future final)
		{
			if (!GameHubBehaviour.Hub.Swordfish.Connection.Connected)
			{
				return;
			}
			Future rewardsGiven = new Future();
			rewardsGiven.Name = "RewardsGiven";
			if (final != null)
			{
				final.DependsOn(rewardsGiven);
			}
			ServerPlayer.ServerClearBagAndGiveRewards(player, delegate(NetResult result)
			{
				if (player.IsNarrator)
				{
					rewardsGiven.Result = 0;
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
				PlayerStats playerStats = GameHubBehaviour.Hub.ScrapBank.PlayerAccounts[player.PlayerCarId];
				RewardsInfo.Medals performanceMedal = (RewardsInfo.Medals)RewardsInfo.GetPerformanceMedal(rewards.DamageDone);
				RewardsInfo.Medals performanceMedal2 = (RewardsInfo.Medals)RewardsInfo.GetPerformanceMedal(rewards.RepairDone);
				RewardsInfo.Medals performanceMedal3 = (RewardsInfo.Medals)RewardsInfo.GetPerformanceMedal(rewards.BombTime);
				RewardsInfo.Medals performanceMedal4 = (RewardsInfo.Medals)RewardsInfo.GetPerformanceMedal(rewards.DebuffTime);
				string text = string.Empty;
				text += string.Format("SwordfishSessionID={0}", Guid.Empty);
				text += string.Format(" MatchID={0}", GameHubBehaviour.Hub.Swordfish.Connection.ServerMatchId);
				text += string.Format(" SteamID={0}", player.UserSF.UniversalID);
				text += string.Format(" KillAndAssists={0}", playerStats.KillsAndAssists);
				text += string.Format(" Deaths={0}", playerStats.Deaths);
				text += string.Format(" TotalDamageDone={0}", playerStats.DamageDealtToPlayers);
				text += string.Format(" TotalDamageReceived={0}", playerStats.DamageReceived);
				text += string.Format(" TotalHeal={0}", playerStats.HealingProvided);
				text += string.Format(" ScrapsEarned={0}", playerStats.TotalScrapCollected);
				text += string.Format(" TimeCarryingBomb={0}", playerStats.BombPossessionTime);
				text += string.Format(" DebuffTimeApplied={0}", playerStats.DebuffTime);
				text += string.Format(" RoleCarrierKills={0}", playerStats.RoleCarrierKills);
				text += string.Format(" RoleTacklerKills={0}", playerStats.RoleTacklerKills);
				text += string.Format(" RoleSupportKills={0}", playerStats.RoleSupportKills);
				text += string.Format(" BombCarrierKills={0}", playerStats.BombCarrierKills);
				text += string.Format(" BombsLost={0}", playerStats.BombLostDeathCount);
				text += string.Format(" BombsDelivered={0}", playerStats.BombsDelivered);
				text += string.Format(" BombsStolen={0}", playerStats.BombTakenCount);
				text += string.Format(" TravelledDistance={0}", playerStats.TravelledDistance);
				text += string.Format(" NumberOfDisconnects={0}", playerStats.NumberOfDisconnects);
				text += string.Format(" NumberOfReconnects={0}", playerStats.NumberOfReconnects);
				text += string.Format(" HighestKillingStreak={0}", playerStats.HighestKillingStreak);
				text += string.Format(" HighestDeathStreak={0}", playerStats.HighestDeathStreak);
				text += string.Format(" DamageMedal={0}", performanceMedal);
				text += string.Format(" DamageDonePerformance={0}", rewards.DamageDone);
				text += string.Format(" HealMedal={0}", performanceMedal2);
				text += string.Format(" HealDonePerformance={0}", rewards.RepairDone);
				text += string.Format(" BombMedal={0}", performanceMedal3);
				text += string.Format(" BombTimePerformance={0}", rewards.BombTime);
				text += string.Format(" Debuffmetal={0}", performanceMedal4);
				text += string.Format(" DebuffTimePerformance={0}", rewards.DebuffTime);
				GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(ServerBITags.GameServerPlayerPerformance, text, false);
				MatchLogWriter.PlayerPerformanceMatchLog(player, playerStats, rewards, performanceMedal, performanceMedal2, performanceMedal3, performanceMedal4);
				MatchLogWriter.WritePlayerEndGameRewards(player, GameHubBehaviour.Hub.Match.Kind.ToString(), rewards.SoftCoinEarned, rewards.GetMatchWon(), rewards.Abandoned);
				rewardsGiven.Result = 0;
			});
		}

		private void UpdateMMR(PlayerData player, IFuture charcaterUpdate, IFuture final)
		{
			Future future = new Future();
			future.Name = "MMR";
			final.DependsOn(future);
			ServerRelay.MMRUpdateStateObject state = new ServerRelay.MMRUpdateStateObject
			{
				PendingFuture = future,
				Player = player
			};
			charcaterUpdate.WhenDone(delegate(IFuture x)
			{
				this.UpdateUserMMR(state);
			});
		}

		private void UpdateUserMMR(ServerRelay.MMRUpdateStateObject state)
		{
			GameHubBehaviour.Hub.ClientApi.character.GetAllCharacters(state, state.Player.PlayerId, new SwordfishClientApi.ParameterizedCallback<Character[]>(this.OnCharactersTakenForMMR), new SwordfishClientApi.ErrorCallback(this.OnCharactersTakenForMMRError));
		}

		private void OnCharactersTakenForMMRError(object objstate, Exception exception)
		{
			ServerRelay.MMRUpdateStateObject mmrupdateStateObject = (ServerRelay.MMRUpdateStateObject)objstate;
			ServerRelay.Log.Fatal(string.Format("Update User MMR failed for user={0} characters not loaded", mmrupdateStateObject.Player.UserSF.UniversalID), exception);
			mmrupdateStateObject.PendingFuture.Result = -1;
		}

		private void OnCharactersTakenForMMR(object objstate, Character[] chars)
		{
			ServerRelay.MMRUpdateStateObject mmrupdateStateObject = (ServerRelay.MMRUpdateStateObject)objstate;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < chars.Length; i++)
			{
				CharacterBag characterBag = (CharacterBag)((JsonSerializeable<T>)chars[i].Bag);
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
			ServerRelay.Log.Fatal(string.Format("Update MMR failed for User={0} mmr should be={1} wins={2} games={3}", new object[]
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
			((ServerRelay.MMRUpdateStateObject)objstate).PendingFuture.Result = 0;
		}

		private void OnStatsUpdatedSuccess(object state, string result)
		{
			ServerRelay.PlayerStateObject playerStateObject = (ServerRelay.PlayerStateObject)state;
			NetResult netResult = (NetResult)((JsonSerializeable<T>)result);
			RewardsBag givenRewards = (RewardsBag)((JsonSerializeable<T>)netResult.Msg);
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
			int kind = (int)GameHubBehaviour.Hub.Match.Kind;
			float performance = Mathf.Max(Mathf.Max(rewards.DamageDone, rewards.RepairDone), Mathf.Max(rewards.BombTime, rewards.DebuffTime));
			MatchHistoryItemBag matchHistoryItemBag = new MatchHistoryItemBag();
			matchHistoryItemBag.PlayerId = playerData.PlayerId;
			matchHistoryItemBag.CharacterId = playerData.CharacterId;
			matchHistoryItemBag.SkinId = playerData.Customizations.SelectedSkin.ToString();
			matchHistoryItemBag.SetDate(DateTime.UtcNow);
			matchHistoryItemBag.GameMode = kind;
			matchHistoryItemBag.BestPerformanceMedal = RewardsInfo.GetPerformanceMedal(performance);
			matchHistoryItemBag.MatchResult = ((playerData.Team != winnerTeam) ? 0 : 1);
			matchHistoryItemBag.Abandoned = rewards.Abandoned;
			if (rewards.Abandoned)
			{
				matchHistoryItemBag.MatchResult = 0;
			}
			matchHistoryItemBag.ArenaIndex = GameHubBehaviour.Hub.Match.ArenaIndex;
			matchHistoryItemBag.MatchId = GameHubBehaviour.Hub.Swordfish.Connection.ServerMatchId.ToString();
			MatchHistoryCustomWS.CreateANewMatch(matchHistoryItemBag, delegate(object s, string o)
			{
				this.OnMatchCreated(playerData, o);
			}, delegate(object s, Exception e)
			{
				this.OnOnMatchCreationFailed(playerData, e);
			});
		}

		private void OnMatchCreated(PlayerData playerData, string result)
		{
		}

		private void OnOnMatchCreationFailed(PlayerData playerData, Exception exception)
		{
			ServerRelay.Log.ErrorFormat("Match Creation failed for={0} Exception={1}", new object[]
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
				return;
			}
			playerByAddress.Connected = true;
			ServerRelay.Log.InfoFormat("OnClientReconnect player={0}, connectionId={1}", new object[]
			{
				playerByAddress,
				connectionId
			});
			GameHubBehaviour.Hub.Players.UpdatePlayers();
			if (playerByAddress.IsNarrator)
			{
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
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = AnnouncerLog.AnnouncerEventKinds.PlayerReconnected,
				Killer = characterInstance.ObjId
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
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
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				ServerPlayer.SetCurrentServer(playerByAddress.PlayerId, 0, null, null, null, false, new Action<NetResult>(this.TutorialClearBagOnDisconnectCallback));
			}
			GameHubBehaviour.Hub.Players.UpdatePlayers();
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

		private void TutorialClearBagOnDisconnectCallback(NetResult obj)
		{
		}

		public void OnTerminateServer(TerminateServerMessage msg)
		{
			((NetworkServer)GameHubBehaviour.Hub.Net).StopServer();
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

		private ProgressionController _progression = new ProgressionController();

		private BattlepassMissionValidator _battlepassMissionValidator = new BattlepassMissionValidator();

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

		private struct PlayerStateObject
		{
			public PlayerData Data;

			public Future PendingFuture;

			public RewardsBag GivenRewards;
		}
	}
}
