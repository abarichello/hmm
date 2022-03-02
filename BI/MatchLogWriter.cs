using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ClientAPI.Utils;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.BI.Battlepass;
using HeavyMetalMachines.BI.CharacterSelection;
using HeavyMetalMachines.BI.GameServer;
using HeavyMetalMachines.BI.Matches;
using HeavyMetalMachines.BI.Players;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches.DataTransferObjects;
using HeavyMetalMachines.Swordfish;
using NativePlugins;
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

		public static T FillBasicInfo<T>(T log) where T : IBaseLog
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
			MatchLogWriter.Log.DebugFormat("Creating BI Log file={0}", new object[]
			{
				text
			});
			MatchLogWriter._fileHandle = LoggerPlugin.CreateLogger(text, CppFileMode.WriteBinary);
		}

		public static void Release()
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			MatchLogWriter.Log.Debug("Releasing BI log file");
			LoggerPlugin.ReleaseLogger(MatchLogWriter._fileHandle);
			MatchLogWriter._enabled = false;
		}

		public static void WriteLog(IBaseLog log)
		{
			MatchLogWriter.WriteLog(log, false);
		}

		private static void WriteLog(IBaseLog log, bool devLog)
		{
			if (devLog)
			{
				MatchLogWriter.Log.Info(Json.ToJSON(log));
			}
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			string szMessage = Json.ToJSON(log);
			LoggerPlugin.Log(MatchLogWriter._fileHandle, szMessage);
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
			if (MatchLogWriter.<>f__mg$cache2 == null)
			{
				MatchLogWriter.<>f__mg$cache2 = new PauseController.InGamePauseStateChangedEvent(MatchLogWriter.OnPauseStateChanged);
			}
			PauseController.OnInGamePauseStateChanged += MatchLogWriter.<>f__mg$cache2;
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
			gameServerMatchStart.GameVersion = "Release.15.00.250";
			gameServerMatchStart.Team1 = new List<string>();
			gameServerMatchStart.Team2 = new List<string>();
			gameServerMatchStart.ArenaIndex = MatchLogWriter.Hub.Match.ArenaIndex;
			gameServerMatchStart.MatchKind = MatchLogWriter.ConvertMatchKindToMatchKindBi(MatchLogWriter.Hub.Match.Kind).ToString();
			gameServerMatchStart.QueueName = MatchLogWriter.Hub.Swordfish.Connection.QueueName;
			gameServerMatchStart.RegionName = MatchLogWriter.Hub.Swordfish.Connection.RegionName;
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
			MatchLogWriter.WriteLog(gameServerMatchStart, true);
		}

		private static MatchLogWriter.BiMatchKind ConvertMatchKindToMatchKindBi(MatchKind kind)
		{
			switch (kind)
			{
			case 0:
				return MatchLogWriter.BiMatchKind.PvP;
			case 1:
				return MatchLogWriter.BiMatchKind.PvE;
			case 2:
				return MatchLogWriter.BiMatchKind.Tutorial;
			case 3:
				return MatchLogWriter.BiMatchKind.Ranked;
			case 4:
				return MatchLogWriter.BiMatchKind.Custom;
			case 5:
				return MatchLogWriter.BiMatchKind.Tournament;
			case 6:
				return MatchLogWriter.BiMatchKind.Training;
			case 7:
				return MatchLogWriter.BiMatchKind.Novice;
			default:
				throw new Exception(string.Format("Unknown match kind: {0}", kind));
			}
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
				matchStartCustomizations.SkinGUID = playerData.Customizations.GetGuidBySlot(59).ToString();
				matchStartCustomizations.KillVfxGUID = playerData.Customizations.GetGuidBySlot(4).ToString();
				matchStartCustomizations.PortraitGUID = playerData.Customizations.GetGuidBySlot(60).ToString();
				matchStartCustomizations.RespawnGUID = playerData.Customizations.GetGuidBySlot(5).ToString();
				matchStartCustomizations.ScoreVfxGUID = playerData.Customizations.GetGuidBySlot(3).ToString();
				matchStartCustomizations.SprayGUID = playerData.Customizations.GetGuidBySlot(1).ToString();
				matchStartCustomizations.TakeOffVfxGUID = playerData.Customizations.GetGuidBySlot(2).ToString();
				matchStartCustomizations.Emote0GUID = playerData.Customizations.GetGuidBySlot(40).ToString();
				matchStartCustomizations.Emote1GUID = playerData.Customizations.GetGuidBySlot(41).ToString();
				matchStartCustomizations.Emote2GUID = playerData.Customizations.GetGuidBySlot(42).ToString();
				matchStartCustomizations.Emote3GUID = playerData.Customizations.GetGuidBySlot(43).ToString();
				matchStartCustomizations.Emote4GUID = playerData.Customizations.GetGuidBySlot(44).ToString();
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
			MatchLogWriter.WriteLog(gameServerMatchEnd, true);
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
			playerPerformance.Kill = stats.Kills;
			playerPerformance.Assists = stats.Assists;
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
			playerPerformance.Gadget0Count = stats.GetGadgetUses(GadgetSlot.CustomGadget0);
			playerPerformance.Gadget1Count = stats.GetGadgetUses(GadgetSlot.CustomGadget1);
			playerPerformance.Gadget2Count = stats.GetGadgetUses(GadgetSlot.CustomGadget2);
			playerPerformance.BombGrabberGadgetCount = stats.GetGadgetUses(GadgetSlot.BombGadget);
			playerPerformance.BoostGadgetCount = stats.GetGadgetUses(GadgetSlot.BoostGadget);
			playerPerformance.Emote0Uses = stats.GetGadgetUses(GadgetSlot.EmoteGadget0);
			playerPerformance.Emote1Uses = stats.GetGadgetUses(GadgetSlot.EmoteGadget1);
			playerPerformance.Emote2Uses = stats.GetGadgetUses(GadgetSlot.EmoteGadget2);
			playerPerformance.Emote3Uses = stats.GetGadgetUses(GadgetSlot.EmoteGadget3);
			playerPerformance.Emote4Uses = stats.GetGadgetUses(GadgetSlot.EmoteGadget4);
			playerPerformance.ReverseCount = stats.ReverseCount;
			playerPerformance.BombLostDeathCount = stats.BombLostDeathCount;
			playerPerformance.BombLostBlockerCount = stats.BombLostBlockerCount;
			playerPerformance.BombLostDropperCount = stats.BombLostDropperCount;
			playerPerformance.BombLostGadgetCount = stats.BombLostGadgetCount;
			playerPerformance.BombTakenCount = stats.BombTakenCount;
			MatchLogWriter.WriteLog(playerPerformance);
		}

		public static void LogGadgetAction(IMatchLog log)
		{
			IMatchLog log2 = MatchLogWriter.FillMatchInfo<IMatchLog>(log);
			MatchLogWriter.WriteLog(log2);
		}

		public static void UserAction(LogAction action, string userId, string reason)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerPlayerAction gameServerPlayerAction = MatchLogWriter.FillBasicInfo<GameServerPlayerAction>(default(GameServerPlayerAction));
			gameServerPlayerAction.Action = action;
			gameServerPlayerAction.PlayerId = userId;
			gameServerPlayerAction.Reason = reason;
			MatchLogWriter.WriteLog(gameServerPlayerAction);
		}

		public static void PlayerDisconnect(LogAction action, string universalId, Heartbeat.DisconnectReason reason, double lastInputTimeSeconds)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerPlayerDisconnect gameServerPlayerDisconnect = MatchLogWriter.FillBasicInfo<GameServerPlayerDisconnect>(default(GameServerPlayerDisconnect));
			gameServerPlayerDisconnect.Action = action;
			gameServerPlayerDisconnect.PlayerId = universalId;
			gameServerPlayerDisconnect.Reason = MatchLogWriter.GetBiReasonName(reason);
			gameServerPlayerDisconnect.SecondsWithoutInput = lastInputTimeSeconds.ToString("e3");
			MatchLogWriter.WriteLog(gameServerPlayerDisconnect);
		}

		public static void PlayerInputFroze(long playerId, double lastInputTimeSeconds)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerPlayerInputFroze gameServerPlayerInputFroze = MatchLogWriter.FillBasicInfo<GameServerPlayerInputFroze>(default(GameServerPlayerInputFroze));
			gameServerPlayerInputFroze.PlayerId = playerId.ToString();
			gameServerPlayerInputFroze.SecondsWithoutInput = lastInputTimeSeconds.ToString("e3");
			MatchLogWriter.WriteLog(gameServerPlayerInputFroze);
		}

		private static string GetBiReasonName(Heartbeat.DisconnectReason reason)
		{
			switch (reason)
			{
			case Heartbeat.DisconnectReason.Unknown:
				return "Unknown";
			case Heartbeat.DisconnectReason.ConnectionClosed:
				return "ConnectionClosed";
			case Heartbeat.DisconnectReason.PlayerDisconnect:
				return "PlayerDisconnect";
			case Heartbeat.DisconnectReason.ConnectionLost:
				return "ConnectionLost";
			case Heartbeat.DisconnectReason.ServerAlone:
				return "ServerAlone";
			case Heartbeat.DisconnectReason.MatchCanceledByMissingPlayers:
				return "MatchCanceledByMissingPlayers";
			case Heartbeat.DisconnectReason.MatchCanceledByServerLoadingStuck:
				return "MatchCanceledByServerLoadingStuck";
			default:
				return "Undefined";
			}
		}

		public static void CharacterSelected(string playerUniversalId, string characterId)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerPlayerSelectCharacter gameServerPlayerSelectCharacter = MatchLogWriter.FillBasicInfo<GameServerPlayerSelectCharacter>(default(GameServerPlayerSelectCharacter));
			gameServerPlayerSelectCharacter.PlayerId = playerUniversalId;
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
					goto IL_81;
				}
				break;
			case SpawnReason.ScoreRed:
				kind = TeamKind.Red;
				goto IL_81;
			case SpawnReason.ScoreBlu:
				kind = TeamKind.Blue;
				goto IL_81;
			}
			kind = MatchLogWriter.GetTeamKind(objectId);
			IL_81:
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
			if (MatchLogWriter.<>f__mg$cache3 == null)
			{
				MatchLogWriter.<>f__mg$cache3 = new Converter<int, string>(MatchLogWriter.GetPlayerId);
			}
			gameServerPlayerKill.Assists = assists.ConvertAll<string>(MatchLogWriter.<>f__mg$cache3);
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
				List<MatchInfoPlayerDamage.PlayerEntry> damage = matchInfoPlayerDamage.Damage;
				MatchInfoPlayerDamage.PlayerEntry item = default(MatchInfoPlayerDamage.PlayerEntry);
				item.PlayerId = playerId;
				item.Damage = damageDealtToPlayers;
				damage.Add(item);
			}
			MatchLogWriter.WriteLog(matchInfoPlayerDamage);
		}

		public static void WriteAfk(IAFKManager afkManager)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			GameServerPlayerAfk gameServerPlayerAfk = MatchLogWriter.FillBasicInfo<GameServerPlayerAfk>(default(GameServerPlayerAfk));
			gameServerPlayerAfk.Afk = new List<GameServerPlayerAfk.PlayerAfk>();
			for (int i = 0; i < afkManager.Entries.Count; i++)
			{
				AFKController.AFKEntry afkentry = afkManager.Entries[i];
				List<GameServerPlayerAfk.PlayerAfk> afk = gameServerPlayerAfk.Afk;
				GameServerPlayerAfk.PlayerAfk item = default(GameServerPlayerAfk.PlayerAfk);
				item.AfkTime = afkentry.AFKTime;
				item.DisconnectionTime = afkentry.DisconnectionTime;
				item.IsLeaver = afkentry.IsLeaver();
				item.PlayerId = afkentry.Player.UserId;
				afk.Add(item);
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
				List<MatchCounselorInfo.CounselorInfo> counselorInfos = matchCounselorInfo.CounselorInfos;
				MatchCounselorInfo.CounselorInfo item = default(MatchCounselorInfo.CounselorInfo);
				item.PlayerId = playerData.UserId;
				item.IsActive = ((!playerData.HasCounselor) ? 0 : 1);
				counselorInfos.Add(item);
			}
			MatchLogWriter.WriteLog(matchCounselorInfo);
		}

		public static void WriteBotsDifficulty(IGetBotDifficulty getBotDifficulty)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			MatchInfoBotDifficulty matchInfoBotDifficulty = MatchLogWriter.FillBasicInfo<MatchInfoBotDifficulty>(default(MatchInfoBotDifficulty));
			matchInfoBotDifficulty.teamsBotDifficulty = new List<MatchInfoBotDifficulty.TeamBotDifficulty>();
			matchInfoBotDifficulty.teamsBotDifficulty.Add(MatchLogWriter.GetTeamBotDifficulty(TeamKind.Blue, getBotDifficulty));
			matchInfoBotDifficulty.teamsBotDifficulty.Add(MatchLogWriter.GetTeamBotDifficulty(TeamKind.Red, getBotDifficulty));
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
				PlayerEndGamePresence.PresenceData presenceData = default(PlayerEndGamePresence.PresenceData);
				presenceData.PlayerId = playerData.UserId;
				presenceData.Present = playerData.Connected;
				PlayerEndGamePresence.PresenceData item = presenceData;
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
					PlayerTechnicalExperience.Experience experience = default(PlayerTechnicalExperience.Experience);
					experience.PlayerId = playerExperienceData.PlayerId;
					experience.FreezeCount = total.FreezeCount;
					experience.FreezeTotalTimeMillis = total.FreezeAcc;
					PlayerTechnicalExperience.Experience item = experience;
					playerTechnicalExperience.Experiences.Add(item);
				}
			}
			MatchLogWriter.WriteLog(playerTechnicalExperience);
		}

		private static MatchInfoBotDifficulty.TeamBotDifficulty GetTeamBotDifficulty(TeamKind team, IGetBotDifficulty getBotDifficulty)
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
			MatchInfoBotDifficulty.TeamBotDifficulty result = default(MatchInfoBotDifficulty.TeamBotDifficulty);
			result.Team = MatchLogWriter.GetTeam(team);
			result.BotWasActivated = flag;
			result.Difficulty = (int)getBotDifficulty.Get(team);
			return result;
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

		public static void WriteBattlepassMissionComplete(PlayerData player, int oldLevel, int currentLevel, Mission missionProgress, int objectiveIndex)
		{
			if (!MatchLogWriter._enabled)
			{
				return;
			}
			BattlepassMissionCompleteBI battlepassMissionCompleteBI = MatchLogWriter.FillBattlepassInfo<BattlepassMissionCompleteBI>(default(BattlepassMissionCompleteBI), player);
			battlepassMissionCompleteBI.MissionObjective = missionProgress.Objectives[objectiveIndex].Objective.ToString();
			battlepassMissionCompleteBI.MissionTarget = missionProgress.Objectives[objectiveIndex].Target;
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

		private static volatile bool _enabled;

		private static int _lastPauseStateChangeSynchTime = 0;

		private static int _bombEventId;

		[CompilerGenerated]
		private static BaseSpawnManager.PlayerUnspawnListener <>f__mg$cache0;

		[CompilerGenerated]
		private static BaseSpawnManager.PlayerUnspawnListener <>f__mg$cache1;

		[CompilerGenerated]
		private static PauseController.InGamePauseStateChangedEvent <>f__mg$cache2;

		[CompilerGenerated]
		private static Converter<int, string> <>f__mg$cache3;

		private enum BiMatchKind
		{
			PvP,
			PvE,
			Tutorial,
			Ranked,
			Custom,
			Tournament,
			Training,
			Novice
		}
	}
}
