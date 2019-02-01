using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ClientAPI.Utils;
using Commons.Swordfish.Battlepass;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Logs;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.BI
{
	public static class MatchLogWriter
	{
		private static HMMHub Hub
		{
			get
			{
				return GameHubBehaviour.Hub;
			}
		}

		private static T FillBasicInfo<T>(T log) where T : IBaseLog
		{
			log.EventAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
			log.MatchId = MatchLogWriter.Hub.Swordfish.Connection.ServerMatchId.ToString();
			return log;
		}

		private static T FillMatchInfo<T>(T log) where T : IMatchLog
		{
			int time = MatchLogWriter.Hub.GameTime.MatchTimer.GetTime();
			int overtimeStartTimeMillis = MatchLogWriter.Hub.BombManager.ScoreBoard.OvertimeStartTimeMillis;
			log = MatchLogWriter.FillBasicInfo<T>(log);
			log.Round = MatchLogWriter.Hub.BombManager.ScoreBoard.Round;
			log.MatchTime = time;
			log.OvertimeTime = ((overtimeStartTimeMillis > 0) ? (time - overtimeStartTimeMillis) : 0);
			return log;
		}

		private static T FillBattlepassInfo<T>(T log, PlayerData playerdata) where T : IBattlepassLog
		{
			log = MatchLogWriter.FillBasicInfo<T>(log);
			log.Season = MatchLogWriter.Hub.SharedConfigs.Battlepass.Season;
			log.SteamID = playerdata.UserSF.UniversalID;
			return log;
		}

		private static string GetPlayerId(int objectId)
		{
			PlayerData playerByObjectId = MatchLogWriter.Hub.Players.GetPlayerByObjectId(objectId);
			if (playerByObjectId == null)
			{
				return "-1";
			}
			return playerByObjectId.UserId;
		}

		private static int GetTeam(TeamKind kind)
		{
			if (kind == TeamKind.Red)
			{
				return 2;
			}
			if (kind != TeamKind.Blue)
			{
				return 0;
			}
			return 1;
		}

		private static TeamKind GetTeamKind(int causerId)
		{
			PlayerData playerOrBotsByObjectId = MatchLogWriter.Hub.Players.GetPlayerOrBotsByObjectId(causerId);
			return (!(playerOrBotsByObjectId == null)) ? playerOrBotsByObjectId.Team : TeamKind.Zero;
		}

		private static void CreateLogger()
		{
			MatchLogWriter._enabled = MatchLogWriter.Hub.Config.GetBoolValue(ConfigAccess.MatchLogEnabled);
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			string text = MatchLogWriter.Hub.Config.GetValue(ConfigAccess.MatchLogFile);
			if (!Directory.GetParent(text).Exists)
			{
				text = "match.log";
			}
			text = text.Replace(".", DateTime.Now.ToString("-yyyyMMdd-HHmmss-") + MatchLogWriter.Hub.Swordfish.Connection.ServerMatchId.ToString() + ".");
			MatchLogWriter._fileHandle = CppLogger.CreateLogger(text, CppFileMode.WriteBinary);
		}

		public static void Release()
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			CppLogger.ReleaseLogger(MatchLogWriter._fileHandle);
			MatchLogWriter._enabled = false;
		}

		private static void WriteLog(IBaseLog log)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			string szMessage = Json.ToJSON(log);
			CppLogger.Log(MatchLogWriter._fileHandle, szMessage);
		}

		public static void AddListeners()
		{
			BaseSpawnManager players = MatchLogWriter.Hub.Events.Players;
			if (MatchLogWriter.<>f__mg$cache0 == null)
			{
				MatchLogWriter.<>f__mg$cache0 = new BaseSpawnManager.PlayerUnspawnListener(MatchLogWriter.OnPlayerUnspawned);
			}
			players.ListenToObjectUnspawn += MatchLogWriter.<>f__mg$cache0;
			BaseSpawnManager bots = MatchLogWriter.Hub.Events.Bots;
			if (MatchLogWriter.<>f__mg$cache1 == null)
			{
				MatchLogWriter.<>f__mg$cache1 = new BaseSpawnManager.PlayerUnspawnListener(MatchLogWriter.OnPlayerUnspawned);
			}
			bots.ListenToObjectUnspawn += MatchLogWriter.<>f__mg$cache1;
			CreepSpawnManager creeps = MatchLogWriter.Hub.Events.Creeps;
			if (MatchLogWriter.<>f__mg$cache2 == null)
			{
				MatchLogWriter.<>f__mg$cache2 = new CreepSpawnManager.CreepUnspawnListener(MatchLogWriter.OnCreepUnspawned);
			}
			creeps.ListenToCreepUnspawn += MatchLogWriter.<>f__mg$cache2;
			if (MatchLogWriter.<>f__mg$cache3 == null)
			{
				MatchLogWriter.<>f__mg$cache3 = new PauseController.InGamePauseStateChangedEvent(MatchLogWriter.OnPauseStateChanged);
			}
			PauseController.OnInGamePauseStateChanged += MatchLogWriter.<>f__mg$cache3;
		}

		private static void OnPauseStateChanged(PauseController.PauseState oldstate, PauseController.PauseState newstate, PlayerData playerdata)
		{
			if (!MatchLogWriter._enabled || (newstate != PauseController.PauseState.Paused && newstate != PauseController.PauseState.Unpaused) || (newstate == PauseController.PauseState.Unpaused && oldstate != PauseController.PauseState.UnpauseCountDown))
			{
				return;
			}
			int synchTime = MatchLogWriter.Hub.GameTime.GetSynchTime();
			PauseStateChange pauseStateChange = MatchLogWriter.FillMatchInfo<PauseStateChange>(default(PauseStateChange));
			pauseStateChange.PlayerId = playerdata.UserId;
			pauseStateChange.PauseState = newstate.ToString();
			if (MatchLogWriter._lastPauseStateChangeSynchTime == 0)
			{
				MatchLogWriter._lastPauseStateChangeSynchTime = synchTime;
			}
			pauseStateChange.SynchTimeSinceLastChange = synchTime - MatchLogWriter._lastPauseStateChangeSynchTime;
			MatchLogWriter._lastPauseStateChangeSynchTime = synchTime;
			MatchLogWriter.WriteLog(pauseStateChange);
		}

		private static void OnCreepUnspawned(CreepRemoveEvent data)
		{
			if (data.Reason != SpawnReason.Death)
			{
				return;
			}
			MatchLogWriter.CreepKilled(data.CauserId);
		}

		private static void OnPlayerUnspawned(PlayerEvent data)
		{
			if (data.Reason != SpawnReason.Death)
			{
				return;
			}
			MatchLogWriter.PlayerKilled(data.CauserId, MatchLogWriter.GetTeamKind(data.CauserId), data.TargetId, MatchLogWriter.GetTeamKind(data.TargetId), data.Assists, data.Location);
		}

		public static void LogStart()
		{
			MatchLogWriter.CreateLogger();
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			MatchLogWriter.AddListeners();
			MatchLogWriter._bombEventId = 0;
			GameServerMatchStart gameServerMatchStart = MatchLogWriter.FillBasicInfo<GameServerMatchStart>(default(GameServerMatchStart));
			gameServerMatchStart.GameVersion = "2.07.972";
			gameServerMatchStart.Team1 = new List<string>();
			gameServerMatchStart.Team2 = new List<string>();
			gameServerMatchStart.ArenaIndex = MatchLogWriter.Hub.Match.ArenaIndex;
			foreach (SwordfishConnection.MatchUser matchUser in MatchLogWriter.Hub.Swordfish.Connection.Users)
			{
				TeamKind team = matchUser.Team;
				if (team != TeamKind.Blue)
				{
					if (team == TeamKind.Red)
					{
						gameServerMatchStart.Team2.Add(matchUser.Id);
					}
				}
				else
				{
					gameServerMatchStart.Team1.Add(matchUser.Id);
				}
			}
			MatchLogWriter.WriteLog(gameServerMatchStart);
		}

		public static void WriteMatchCustomizationsBI()
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			for (int i = 0; i < MatchLogWriter.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = MatchLogWriter.Hub.Players.Players[i];
				MatchStartCustomizations matchStartCustomizations = MatchLogWriter.FillBasicInfo<MatchStartCustomizations>(default(MatchStartCustomizations));
				matchStartCustomizations.Season = MatchLogWriter.Hub.SharedConfigs.Battlepass.Season;
				matchStartCustomizations.SteamID = playerData.UserSF.UniversalID;
				matchStartCustomizations.SkinGUID = playerData.Customizations.SelectedSkin.ToString();
				matchStartCustomizations.KillVfxGUID = playerData.Customizations.SelectedKillVfxItemTypeId.ToString();
				matchStartCustomizations.PortraitGUID = playerData.Customizations.SelectedPortraitItemTypeId.ToString();
				matchStartCustomizations.RespawnGUID = playerData.Customizations.SelectedRespawnVfxItemTypeId.ToString();
				matchStartCustomizations.ScoreVfxGUID = playerData.Customizations.SelectedScoreVfxItemTypeId.ToString();
				matchStartCustomizations.SprayGUID = playerData.Customizations.SelectedSprayItemTypeId.ToString();
				matchStartCustomizations.TakeOffVfxGUID = playerData.Customizations.SelectedTakeOffVfxItemTypeId.ToString();
				MatchLogWriter.WriteLog(matchStartCustomizations);
			}
		}

		public static void MatchEnd(MatchData.MatchState winner)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerMatchEnd gameServerMatchEnd = MatchLogWriter.FillMatchInfo<GameServerMatchEnd>(default(GameServerMatchEnd));
			if (winner != MatchData.MatchState.MatchOverBluWins)
			{
				if (winner == MatchData.MatchState.MatchOverRedWins)
				{
					gameServerMatchEnd.Winner = MatchLogWriter.GetTeam(TeamKind.Red);
				}
			}
			else
			{
				gameServerMatchEnd.Winner = MatchLogWriter.GetTeam(TeamKind.Blue);
			}
			MatchLogWriter.WriteLog(gameServerMatchEnd);
			MatchLogWriter.Release();
		}

		public static void PlayerPerformanceMatchLog(PlayerData player, PlayerStats stats, RewardsBag rewards, RewardsInfo.Medals damageMedal, RewardsInfo.Medals healMedal, RewardsInfo.Medals bombMedal, RewardsInfo.Medals debuffMedal)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			PlayerPerformance playerPerformance = MatchLogWriter.FillBasicInfo<PlayerPerformance>(default(PlayerPerformance));
			playerPerformance.SwordfishSessionID = Guid.Empty.ToString();
			playerPerformance.SteamID = player.UserSF.UniversalID;
			playerPerformance.KillAndAssists = stats.KillsAndAssists;
			playerPerformance.Deaths = stats.Deaths;
			playerPerformance.TotalDamageDone = stats.DamageDealtToPlayers;
			playerPerformance.TotalDamageReceived = stats.DamageReceived;
			playerPerformance.TotalHeal = stats.HealingProvided;
			playerPerformance.ScrapsEarned = stats.TotalScrapCollected;
			playerPerformance.TimeCarryingBomb = stats.BombPossessionTime;
			playerPerformance.DebuffTimeApplied = stats.DebuffTime;
			playerPerformance.RoleCarrierKills = stats.RoleCarrierKills;
			playerPerformance.RoleTacklerKills = stats.RoleTacklerKills;
			playerPerformance.RoleSupportKills = stats.RoleSupportKills;
			playerPerformance.BombCarrierKills = stats.BombCarrierKills;
			playerPerformance.BombsDelivered = stats.BombsDelivered;
			playerPerformance.TravelledDistance = stats.TravelledDistance;
			playerPerformance.NumberOfDisconnects = stats.NumberOfDisconnects;
			playerPerformance.NumberOfReconnects = stats.NumberOfReconnects;
			playerPerformance.HighestKillingStreak = stats.HighestKillingStreak;
			playerPerformance.HighestDeathStreak = stats.HighestDeathStreak;
			playerPerformance.DamageMedal = damageMedal.ToString();
			playerPerformance.DamageDonePerformance = rewards.DamageDone;
			playerPerformance.HealMedal = healMedal.ToString();
			playerPerformance.HealDonePerformance = rewards.RepairDone;
			playerPerformance.BombMedal = bombMedal.ToString();
			playerPerformance.BombTimePerformance = rewards.BombTime;
			playerPerformance.Debuffmetal = debuffMedal.ToString();
			playerPerformance.DebuffTimePerformance = rewards.DebuffTime;
			playerPerformance.TimeBotControlledMillis = stats.TimeBotControlled;
			playerPerformance.Gadget0Count = stats.Gadget0Count;
			playerPerformance.Gadget1Count = stats.Gadget1Count;
			playerPerformance.Gadget2Count = stats.Gadget2Count;
			playerPerformance.BombGrabberGadgetCount = stats.BombGadgetGrabberCount;
			playerPerformance.BoostGadgetCount = stats.BoostGadgetCount;
			playerPerformance.ReverseCount = stats.ReverseCount;
			playerPerformance.BombLostDeathCount = stats.BombLostDeathCount;
			playerPerformance.BombLostBlockerCount = stats.BombLostBlockerCount;
			playerPerformance.BombLostDropperCount = stats.BombLostDropperCount;
			playerPerformance.BombLostGadgetCount = stats.BombLostGadgetCount;
			playerPerformance.BombTakenCount = stats.BombTakenCount;
			MatchLogWriter.WriteLog(playerPerformance);
		}

		public static void PlayerAction(LogAction action, int objectId, string reason)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerPlayerAction gameServerPlayerAction = MatchLogWriter.FillBasicInfo<GameServerPlayerAction>(default(GameServerPlayerAction));
			gameServerPlayerAction.Action = action;
			gameServerPlayerAction.PlayerId = MatchLogWriter.GetPlayerId(objectId);
			gameServerPlayerAction.Reason = reason;
			MatchLogWriter.WriteLog(gameServerPlayerAction);
		}

		public static void CharacterSelected(int objectId, string characterId)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerPlayerSelectCharacter gameServerPlayerSelectCharacter = MatchLogWriter.FillBasicInfo<GameServerPlayerSelectCharacter>(default(GameServerPlayerSelectCharacter));
			gameServerPlayerSelectCharacter.PlayerId = MatchLogWriter.GetPlayerId(objectId);
			gameServerPlayerSelectCharacter.CharacterId = characterId;
			MatchLogWriter.WriteLog(gameServerPlayerSelectCharacter);
		}

		[Obsolete]
		public static void BombCollected(int objectId)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerBombCollect gameServerBombCollect = MatchLogWriter.FillMatchInfo<GameServerBombCollect>(default(GameServerBombCollect));
			gameServerBombCollect.PlayerId = MatchLogWriter.GetPlayerId(objectId);
			MatchLogWriter.WriteLog(gameServerBombCollect);
		}

		[Obsolete]
		public static void BombDropped(int objectId, SpawnReason reason, Vector3 position)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerBombDrop gameServerBombDrop = MatchLogWriter.FillMatchInfo<GameServerBombDrop>(default(GameServerBombDrop));
			gameServerBombDrop.Reason = reason.ToString();
			gameServerBombDrop.PlayerId = MatchLogWriter.GetPlayerId(objectId);
			TeamKind kind = TeamKind.Neutral;
			switch (reason)
			{
			case SpawnReason.TriggerDrop:
			case SpawnReason.InputDrop:
				break;
			default:
				if (reason != SpawnReason.Death)
				{
					goto IL_7F;
				}
				break;
			case SpawnReason.ScoreRed:
				kind = TeamKind.Red;
				goto IL_7F;
			case SpawnReason.ScoreBlu:
				kind = TeamKind.Blue;
				goto IL_7F;
			}
			kind = MatchLogWriter.GetTeamKind(objectId);
			IL_7F:
			gameServerBombDrop.Team = MatchLogWriter.GetTeam(kind);
			gameServerBombDrop.PositionX = position.x;
			gameServerBombDrop.PositionZ = position.z;
			MatchLogWriter.WriteLog(gameServerBombDrop);
		}

		public static void BombEvent(int player, GameServerBombEvent.EventKind kind, float holdTime, Vector3 position, float bombAngle, bool meteor)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerBombEvent gameServerBombEvent = MatchLogWriter.FillMatchInfo<GameServerBombEvent>(default(GameServerBombEvent));
			gameServerBombEvent.Kind = kind;
			gameServerBombEvent.HoldTimeSeconds = holdTime;
			gameServerBombEvent.PositionX = position.x;
			gameServerBombEvent.PositionZ = position.z;
			gameServerBombEvent.BombDirectionAngle = bombAngle;
			gameServerBombEvent.Meteor = meteor;
			gameServerBombEvent.PlayerId = MatchLogWriter.GetPlayerId(player);
			TeamKind teamKind = MatchLogWriter.GetTeamKind(player);
			gameServerBombEvent.Team = MatchLogWriter.GetTeam(teamKind);
			gameServerBombEvent.BombEventId = MatchLogWriter._bombEventId++;
			MatchLogWriter.WriteLog(gameServerBombEvent);
		}

		public static void CreepKilled(int objectId)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerCreepKill gameServerCreepKill = MatchLogWriter.FillMatchInfo<GameServerCreepKill>(default(GameServerCreepKill));
			gameServerCreepKill.PlayerId = MatchLogWriter.GetPlayerId(objectId);
			MatchLogWriter.WriteLog(gameServerCreepKill);
		}

		public static void PlayerKilled(int objectId, TeamKind objTeam, int victimId, TeamKind victimTeam, List<int> assists, Vector3 position)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerPlayerKill gameServerPlayerKill = default(GameServerPlayerKill);
			gameServerPlayerKill = MatchLogWriter.FillMatchInfo<GameServerPlayerKill>(gameServerPlayerKill);
			gameServerPlayerKill.PlayerId = MatchLogWriter.GetPlayerId(objectId);
			gameServerPlayerKill.PlayerTeam = MatchLogWriter.GetTeam(objTeam);
			gameServerPlayerKill.VictimId = MatchLogWriter.GetPlayerId(victimId);
			gameServerPlayerKill.VictimTeam = MatchLogWriter.GetTeam(victimTeam);
			if (MatchLogWriter.<>f__mg$cache4 == null)
			{
				MatchLogWriter.<>f__mg$cache4 = new Converter<int, string>(MatchLogWriter.GetPlayerId);
			}
			gameServerPlayerKill.Assists = assists.ConvertAll<string>(MatchLogWriter.<>f__mg$cache4);
			gameServerPlayerKill.PositionX = position.x;
			gameServerPlayerKill.PositionZ = position.z;
			MatchLogWriter.WriteLog(gameServerPlayerKill);
		}

		public static void Upgraded(int objectId, string skillId, string upgradeId, GarageController.UpgradeOperationKind operation)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerUpgradeTransaction gameServerUpgradeTransaction = MatchLogWriter.FillMatchInfo<GameServerUpgradeTransaction>(default(GameServerUpgradeTransaction));
			gameServerUpgradeTransaction.PlayerId = MatchLogWriter.GetPlayerId(objectId);
			gameServerUpgradeTransaction.SkillId = skillId;
			gameServerUpgradeTransaction.UpgradeId = upgradeId;
			gameServerUpgradeTransaction.TransactionType = operation.ToString();
			MatchLogWriter.WriteLog(gameServerUpgradeTransaction);
		}

		public static void WriteDamage()
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			MatchInfoPlayerDamage matchInfoPlayerDamage = default(MatchInfoPlayerDamage);
			matchInfoPlayerDamage = MatchLogWriter.FillMatchInfo<MatchInfoPlayerDamage>(matchInfoPlayerDamage);
			matchInfoPlayerDamage.Damage = new List<MatchInfoPlayerDamage.PlayerEntry>();
			foreach (KeyValuePair<int, PlayerStats> keyValuePair in MatchLogWriter.Hub.ScrapBank.PlayerAccounts)
			{
				string playerId = MatchLogWriter.GetPlayerId(keyValuePair.Key);
				float damageDealtToPlayers = keyValuePair.Value.DamageDealtToPlayers;
				matchInfoPlayerDamage.Damage.Add(new MatchInfoPlayerDamage.PlayerEntry
				{
					PlayerId = playerId,
					Damage = damageDealtToPlayers
				});
			}
			MatchLogWriter.WriteLog(matchInfoPlayerDamage);
		}

		public static void WriteAfk()
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerPlayerAfk gameServerPlayerAfk = MatchLogWriter.FillBasicInfo<GameServerPlayerAfk>(default(GameServerPlayerAfk));
			gameServerPlayerAfk.Afk = new List<GameServerPlayerAfk.PlayerAfk>();
			for (int i = 0; i < MatchLogWriter.Hub.afkController.Entries.Count; i++)
			{
				AFKController.AFKEntry afkentry = MatchLogWriter.Hub.afkController.Entries[i];
				gameServerPlayerAfk.Afk.Add(new GameServerPlayerAfk.PlayerAfk
				{
					AfkTime = afkentry.AFKTime,
					DisconnectionTime = afkentry.DisconnectionTime,
					IsLeaver = afkentry.IsLeaver(),
					PlayerId = afkentry.Player.UserId
				});
			}
			MatchLogWriter.WriteLog(gameServerPlayerAfk);
		}

		public static void WriteCounselorBI()
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			MatchCounselorInfo matchCounselorInfo = MatchLogWriter.FillBasicInfo<MatchCounselorInfo>(default(MatchCounselorInfo));
			matchCounselorInfo.CounselorInfos = new List<MatchCounselorInfo.CounselorInfo>();
			for (int i = 0; i < MatchLogWriter.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = MatchLogWriter.Hub.Players.Players[i];
				matchCounselorInfo.CounselorInfos.Add(new MatchCounselorInfo.CounselorInfo
				{
					PlayerId = playerData.UserId,
					IsActive = ((!playerData.HasCounselor) ? 0 : 1)
				});
			}
			MatchLogWriter.WriteLog(matchCounselorInfo);
		}

		public static void WriteBotsDifficulty()
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			MatchInfoBotDifficulty matchInfoBotDifficulty = MatchLogWriter.FillBasicInfo<MatchInfoBotDifficulty>(default(MatchInfoBotDifficulty));
			matchInfoBotDifficulty.teamsBotDifficulty = new List<MatchInfoBotDifficulty.TeamBotDifficulty>();
			matchInfoBotDifficulty.teamsBotDifficulty.Add(MatchLogWriter.GetTeamBotDifficulty(TeamKind.Blue));
			matchInfoBotDifficulty.teamsBotDifficulty.Add(MatchLogWriter.GetTeamBotDifficulty(TeamKind.Red));
			MatchLogWriter.WriteLog(matchInfoBotDifficulty);
		}

		public static List<PlayerEndGamePresence.PresenceData> WritePlayersPresence()
		{
			if (!MatchLogWriter._enabled)
			{
				return null;
			}
			PlayerEndGamePresence playerEndGamePresence = MatchLogWriter.FillBasicInfo<PlayerEndGamePresence>(default(PlayerEndGamePresence));
			playerEndGamePresence.Presences = new List<PlayerEndGamePresence.PresenceData>(MatchLogWriter.Hub.Players.Players.Count);
			for (int i = 0; i < MatchLogWriter.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = MatchLogWriter.Hub.Players.Players[i];
				PlayerEndGamePresence.PresenceData item = new PlayerEndGamePresence.PresenceData
				{
					PlayerId = playerData.UserId,
					Present = playerData.Connected
				};
				playerEndGamePresence.Presences.Add(item);
			}
			MatchLogWriter.WriteLog(playerEndGamePresence);
			return playerEndGamePresence.Presences;
		}

		public static void WritePlayerExperience()
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			PlayerTechnicalExperience playerTechnicalExperience = MatchLogWriter.FillBasicInfo<PlayerTechnicalExperience>(default(PlayerTechnicalExperience));
			playerTechnicalExperience.Experiences = new List<PlayerTechnicalExperience.Experience>(MatchLogWriter.Hub.Players.Players.Count);
			for (int i = 0; i < MatchLogWriter.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = MatchLogWriter.Hub.Players.Players[i];
				PlayerTechnicalExperienceManager.PlayerExperienceData playerExperienceData;
				if (!MatchLogWriter.Hub.PlayerExperienceBI.ExperienceDatas.TryGetValue(playerData.PlayerAddress, out playerExperienceData))
				{
					MatchLogWriter.Log.WarnFormat("Failed to find player experience data for player={0}", new object[]
					{
						playerData.PlayerAddress
					});
				}
				else
				{
					ExperienceDataSet total = playerExperienceData.Total;
					PlayerTechnicalExperience.Experience item = new PlayerTechnicalExperience.Experience
					{
						PlayerId = playerExperienceData.PlayerId,
						FreezeCount = total.FreezeCount,
						FreezeTotalTimeMillis = total.FreezeAcc
					};
					playerTechnicalExperience.Experiences.Add(item);
				}
			}
			MatchLogWriter.WriteLog(playerTechnicalExperience);
		}

		private static MatchInfoBotDifficulty.TeamBotDifficulty GetTeamBotDifficulty(TeamKind team)
		{
			bool flag = false;
			for (int i = 0; i < MatchLogWriter.Hub.Players.Bots.Count; i++)
			{
				PlayerData playerData = MatchLogWriter.Hub.Players.Bots[i];
				if (playerData.Team == team)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				foreach (PlayerStats playerStats in MatchLogWriter.Hub.ScrapBank.PlayerAccounts.Values)
				{
					if (playerStats != null && playerStats.Combat.Team == team && playerStats.TimeBotControlled > 0)
					{
						flag = true;
						break;
					}
				}
			}
			return new MatchInfoBotDifficulty.TeamBotDifficulty
			{
				Team = MatchLogWriter.GetTeam(team),
				BotWasActivated = flag,
				Difficulty = (int)MatchLogWriter.Hub.Players.GetBotDifficulty(team)
			};
		}

		public static void WriteBattlepassLevelUp(PlayerData player, int currentBattlepassLevel)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			BattlepassLevelUpBI battlepassLevelUpBI = MatchLogWriter.FillBattlepassInfo<BattlepassLevelUpBI>(default(BattlepassLevelUpBI), player);
			battlepassLevelUpBI.LevelReachedZeroBased = currentBattlepassLevel;
			battlepassLevelUpBI.IsPremium = player.BattlepassProgress.HasPremium();
			MatchLogWriter.WriteLog(battlepassLevelUpBI);
		}

		public static void WriteBattlepassMissionComplete(PlayerData player, int oldLevel, int currentLevel, Mission missionProgress)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			BattlepassMissionCompleteBI battlepassMissionCompleteBI = MatchLogWriter.FillBattlepassInfo<BattlepassMissionCompleteBI>(default(BattlepassMissionCompleteBI), player);
			battlepassMissionCompleteBI.MissionObjective = missionProgress.Objective.ToString();
			battlepassMissionCompleteBI.MissionTarget = missionProgress.Target;
			battlepassMissionCompleteBI.MissionXp = missionProgress.XpReward;
			battlepassMissionCompleteBI.LevelBeforeZeroBased = oldLevel;
			battlepassMissionCompleteBI.LevelAfterZeroBased = currentLevel;
			MatchLogWriter.WriteLog(battlepassMissionCompleteBI);
		}

		public static void WriteBattlePassMatchExperience(PlayerData player, RewardsBag givenRewards)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			BattlePassMatchExperienceBI battlePassMatchExperienceBI = MatchLogWriter.FillBattlepassInfo<BattlePassMatchExperienceBI>(default(BattlePassMatchExperienceBI), player);
			battlePassMatchExperienceBI.XpByMatchtime = givenRewards.XpByMatchtime;
			battlePassMatchExperienceBI.XpByWin = givenRewards.XpByWin;
			battlePassMatchExperienceBI.XpByPerformance = givenRewards.XpByPerformance;
			battlePassMatchExperienceBI.XPByMission = givenRewards.XPByMission;
			battlePassMatchExperienceBI.XpByFounderBonus = givenRewards.XpByFounderBonus;
			battlePassMatchExperienceBI.XpByBoost = givenRewards.XpByBoost;
			battlePassMatchExperienceBI.XpByEvent = givenRewards.XpByEvent;
			battlePassMatchExperienceBI.XpGivenTotal = givenRewards.GetTotalMatchXP();
			MatchLogWriter.WriteLog(battlePassMatchExperienceBI);
		}

		public static void WritePlayerEndGameRewards(PlayerData player, string matchKind, int fameAmount, bool playerWon, bool isPlayerLeaver)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			PlayerEndGameRewards playerEndGameRewards = MatchLogWriter.FillBasicInfo<PlayerEndGameRewards>(default(PlayerEndGameRewards));
			playerEndGameRewards.SteamID = player.UserSF.UniversalID;
			playerEndGameRewards.MatchKind = matchKind;
			playerEndGameRewards.MatchWon = playerWon;
			playerEndGameRewards.FameAmount = fameAmount;
			playerEndGameRewards.IsLeaver = isPlayerLeaver;
			MatchLogWriter.WriteLog(playerEndGameRewards);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MatchLogWriter));

		private static ulong _fileHandle;

		private static bool _enabled;

		private static int _lastPauseStateChangeSynchTime = 0;

		private static int _bombEventId;

		[CompilerGenerated]
		private static BaseSpawnManager.PlayerUnspawnListener <>f__mg$cache0;

		[CompilerGenerated]
		private static BaseSpawnManager.PlayerUnspawnListener <>f__mg$cache1;

		[CompilerGenerated]
		private static CreepSpawnManager.CreepUnspawnListener <>f__mg$cache2;

		[CompilerGenerated]
		private static PauseController.InGamePauseStateChangedEvent <>f__mg$cache3;

		[CompilerGenerated]
		private static Converter<int, string> <>f__mg$cache4;
	}
}
