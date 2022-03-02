using System;
using HeavyMetalMachines.Configuring.Instances;
using UnityEngine;

namespace HeavyMetalMachines
{
	public static class ConfigAccess
	{
		// Note: this type is marked as 'beforefieldinit'.
		static ConfigAccess()
		{
			ConfigInstance[] array = new ConfigInstance[4];
			int num = 0;
			ConfigInstance configInstance = default(ConfigInstance);
			configInstance.Context = "Debug";
			configInstance.Key = "Team1Character4";
			configInstance.DefaultInt = 1;
			array[num] = configInstance;
			int num2 = 1;
			ConfigInstance configInstance2 = default(ConfigInstance);
			configInstance2.Context = "Debug";
			configInstance2.Key = "Team1Character3";
			configInstance2.DefaultInt = 6;
			array[num2] = configInstance2;
			int num3 = 2;
			ConfigInstance configInstance3 = default(ConfigInstance);
			configInstance3.Context = "Debug";
			configInstance3.Key = "Team1Character2";
			configInstance3.DefaultInt = 8;
			array[num3] = configInstance3;
			int num4 = 3;
			ConfigInstance configInstance4 = default(ConfigInstance);
			configInstance4.Context = "Debug";
			configInstance4.Key = "Team1Character1";
			configInstance4.DefaultInt = 13;
			array[num4] = configInstance4;
			ConfigAccess.Team1Characters = array;
			ConfigInstance[] array2 = new ConfigInstance[4];
			int num5 = 0;
			ConfigInstance configInstance5 = default(ConfigInstance);
			configInstance5.Context = "Debug";
			configInstance5.Key = "Team2Character4";
			configInstance5.DefaultInt = 1;
			array2[num5] = configInstance5;
			int num6 = 1;
			ConfigInstance configInstance6 = default(ConfigInstance);
			configInstance6.Context = "Debug";
			configInstance6.Key = "Team2Character3";
			configInstance6.DefaultInt = 6;
			array2[num6] = configInstance6;
			int num7 = 2;
			ConfigInstance configInstance7 = default(ConfigInstance);
			configInstance7.Context = "Debug";
			configInstance7.Key = "Team2Character2";
			configInstance7.DefaultInt = 8;
			array2[num7] = configInstance7;
			int num8 = 3;
			ConfigInstance configInstance8 = default(ConfigInstance);
			configInstance8.Context = "Debug";
			configInstance8.Key = "Team2Character1";
			configInstance8.DefaultInt = 13;
			array2[num8] = configInstance8;
			ConfigAccess.Team2Characters = array2;
			ConfigInstance[] array3 = new ConfigInstance[4];
			int num9 = 0;
			ConfigInstance configInstance9 = default(ConfigInstance);
			configInstance9.Context = "Debug";
			configInstance9.Key = "Team1Skin4";
			configInstance9.DefaultString = string.Empty;
			array3[num9] = configInstance9;
			int num10 = 1;
			ConfigInstance configInstance10 = default(ConfigInstance);
			configInstance10.Context = "Debug";
			configInstance10.Key = "Team1Skin3";
			configInstance10.DefaultString = string.Empty;
			array3[num10] = configInstance10;
			int num11 = 2;
			ConfigInstance configInstance11 = default(ConfigInstance);
			configInstance11.Context = "Debug";
			configInstance11.Key = "Team1Skin2";
			configInstance11.DefaultString = string.Empty;
			array3[num11] = configInstance11;
			int num12 = 3;
			ConfigInstance configInstance12 = default(ConfigInstance);
			configInstance12.Context = "Debug";
			configInstance12.Key = "Team1Skin1";
			configInstance12.DefaultString = string.Empty;
			array3[num12] = configInstance12;
			ConfigAccess.Team1Skins = array3;
			ConfigInstance[] array4 = new ConfigInstance[4];
			int num13 = 0;
			ConfigInstance configInstance13 = default(ConfigInstance);
			configInstance13.Context = "Debug";
			configInstance13.Key = "Team2Skin4";
			configInstance13.DefaultString = string.Empty;
			array4[num13] = configInstance13;
			int num14 = 1;
			ConfigInstance configInstance14 = default(ConfigInstance);
			configInstance14.Context = "Debug";
			configInstance14.Key = "Team2Skin3";
			configInstance14.DefaultString = string.Empty;
			array4[num14] = configInstance14;
			int num15 = 2;
			ConfigInstance configInstance15 = default(ConfigInstance);
			configInstance15.Context = "Debug";
			configInstance15.Key = "Team2Skin2";
			configInstance15.DefaultString = string.Empty;
			array4[num15] = configInstance15;
			int num16 = 3;
			ConfigInstance configInstance16 = default(ConfigInstance);
			configInstance16.Context = "Debug";
			configInstance16.Key = "Team2Skin1";
			configInstance16.DefaultString = string.Empty;
			array4[num16] = configInstance16;
			ConfigAccess.Team2Skins = array4;
			ConfigInstance forcedFakePublisher = default(ConfigInstance);
			forcedFakePublisher.Context = "Game";
			forcedFakePublisher.Key = "ForcedFakePublisher";
			forcedFakePublisher.DefaultString = string.Empty;
			ConfigAccess.ForcedFakePublisher = forcedFakePublisher;
			ConfigInstance forcedFakePsnCrossplayEnabled = default(ConfigInstance);
			forcedFakePsnCrossplayEnabled.Context = "Game";
			forcedFakePsnCrossplayEnabled.Key = "ForcedFakePsnCrossplayEnabled";
			forcedFakePsnCrossplayEnabled.DefaultBool = false;
			ConfigAccess.ForcedFakePsnCrossplayEnabled = forcedFakePsnCrossplayEnabled;
			ConfigInstance forcedFakePsnCrossplayValue = default(ConfigInstance);
			forcedFakePsnCrossplayValue.Context = "Game";
			forcedFakePsnCrossplayValue.Key = "ForcedFakePsnCrossplayValue";
			forcedFakePsnCrossplayValue.DefaultBool = false;
			ConfigAccess.ForcedFakePsnCrossplayValue = forcedFakePsnCrossplayValue;
			ConfigInstance voiceVolumeAt = default(ConfigInstance);
			voiceVolumeAt.Context = "Game";
			voiceVolumeAt.Key = "VCVolumeAt100";
			voiceVolumeAt.DefaultInt = 50;
			ConfigAccess.VoiceVolumeAt100 = voiceVolumeAt;
			ConfigInstance voiceVolumeAt2 = default(ConfigInstance);
			voiceVolumeAt2.Context = "Game";
			voiceVolumeAt2.Key = "VCVolumeAt200";
			voiceVolumeAt2.DefaultInt = 80;
			ConfigAccess.VoiceVolumeAt200 = voiceVolumeAt2;
			ConfigInstance customGCControl = default(ConfigInstance);
			customGCControl.Context = "Game";
			customGCControl.Key = "CustomGCControl";
			customGCControl.DefaultBool = true;
			ConfigAccess.CustomGCControl = customGCControl;
			ConfigInstance maxGCAllocatedSize = default(ConfigInstance);
			maxGCAllocatedSize.Context = "Game";
			maxGCAllocatedSize.Key = "MaxGCAllocatedSize";
			maxGCAllocatedSize.DefaultLong = 1283457024L;
			ConfigAccess.MaxGCAllocatedSize = maxGCAllocatedSize;
			ConfigInstance matchLogFile = default(ConfigInstance);
			matchLogFile.Context = "Log";
			matchLogFile.Key = "MatchLogFile";
			matchLogFile.DefaultString = "match.log";
			ConfigAccess.MatchLogFile = matchLogFile;
			ConfigInstance matchLogEnabled = default(ConfigInstance);
			matchLogEnabled.Context = "Log";
			matchLogEnabled.Key = "MatchLogEnabled";
			matchLogEnabled.DefaultBool = true;
			ConfigAccess.MatchLogEnabled = matchLogEnabled;
			ConfigInstance logLevel = default(ConfigInstance);
			logLevel.Context = "Log";
			logLevel.Key = "level";
			logLevel.DefaultString = "error,warning,event,info,stats,debug";
			ConfigAccess.LogLevel = logLevel;
			ConfigInstance logFilter = default(ConfigInstance);
			logFilter.Context = "Log";
			logFilter.Key = "filter";
			logFilter.DefaultString = string.Empty;
			ConfigAccess.LogFilter = logFilter;
			ConfigInstance freezeLogCount = default(ConfigInstance);
			freezeLogCount.Context = "Log";
			freezeLogCount.Key = "FreezeLogCount";
			freezeLogCount.DefaultInt = 1;
			ConfigAccess.FreezeLogCount = freezeLogCount;
			ConfigInstance serverPort = default(ConfigInstance);
			serverPort.Context = "Server";
			serverPort.Key = "Port";
			serverPort.DefaultInt = 9696;
			ConfigAccess.ServerPort = serverPort;
			ConfigInstance serverIP = default(ConfigInstance);
			serverIP.Context = "Server";
			serverIP.Key = "IP";
			serverIP.DefaultString = "127.0.0.1";
			ConfigAccess.ServerIP = serverIP;
			ConfigInstance serverLoneTimeoutSeconds = default(ConfigInstance);
			serverLoneTimeoutSeconds.Context = "Server";
			serverLoneTimeoutSeconds.Key = "LoneTimeout";
			serverLoneTimeoutSeconds.DefaultInt = 300;
			ConfigAccess.ServerLoneTimeoutSeconds = serverLoneTimeoutSeconds;
			ConfigInstance serverTutorialLoneTimeoutSeconds = default(ConfigInstance);
			serverTutorialLoneTimeoutSeconds.Context = "Server";
			serverTutorialLoneTimeoutSeconds.Key = "TutorialLoneTimeout";
			serverTutorialLoneTimeoutSeconds.DefaultInt = 60;
			ConfigAccess.ServerTutorialLoneTimeoutSeconds = serverTutorialLoneTimeoutSeconds;
			ConfigInstance targetFixedStepServer = default(ConfigInstance);
			targetFixedStepServer.Context = "Server";
			targetFixedStepServer.Key = "TargetFixedStep";
			targetFixedStepServer.DefaultFloat = 0.016666668f;
			ConfigAccess.TargetFixedStepServer = targetFixedStepServer;
			ConfigInstance afkdisconnectionLimit = default(ConfigInstance);
			afkdisconnectionLimit.Context = "Server";
			afkdisconnectionLimit.Key = "AFKDisconnectionLimit";
			afkdisconnectionLimit.DefaultFloat = 300f;
			ConfigAccess.AFKDisconnectionLimit = afkdisconnectionLimit;
			ConfigInstance afkmodifierLimit = default(ConfigInstance);
			afkmodifierLimit.Context = "Server";
			afkmodifierLimit.Key = "AFKModifierLimit";
			afkmodifierLimit.DefaultFloat = 300f;
			ConfigAccess.AFKModifierLimit = afkmodifierLimit;
			ConfigInstance afkinputLimit = default(ConfigInstance);
			afkinputLimit.Context = "Server";
			afkinputLimit.Key = "AFKInputLimit";
			afkinputLimit.DefaultFloat = 15f;
			ConfigAccess.AFKInputLimit = afkinputLimit;
			ConfigInstance loadingTimeoutSeconds = default(ConfigInstance);
			loadingTimeoutSeconds.Context = "Server";
			loadingTimeoutSeconds.Key = "LoadingTimeoutSeconds";
			loadingTimeoutSeconds.DefaultInt = 60;
			ConfigAccess.LoadingTimeoutSeconds = loadingTimeoutSeconds;
			ConfigInstance forceRookie = default(ConfigInstance);
			forceRookie.Context = "Server";
			forceRookie.Key = "ForceRookie";
			forceRookie.DefaultBool = false;
			ConfigAccess.ForceRookie = forceRookie;
			ConfigInstance forceAlwaysRunPathFind = default(ConfigInstance);
			forceAlwaysRunPathFind.Context = "Server";
			forceAlwaysRunPathFind.Key = "ForcePathFind";
			forceAlwaysRunPathFind.DefaultBool = false;
			ConfigAccess.ForceAlwaysRunPathFind = forceAlwaysRunPathFind;
			ConfigInstance enableTrainingPopUpV = default(ConfigInstance);
			enableTrainingPopUpV.Context = "Game";
			enableTrainingPopUpV.Key = "TrainingGroup";
			enableTrainingPopUpV.DefaultInt = 0;
			ConfigAccess.EnableTrainingPopUpV3 = enableTrainingPopUpV;
			ConfigInstance disablePlayback = default(ConfigInstance);
			disablePlayback.Context = "Game";
			disablePlayback.Key = "DisablePlayback2";
			disablePlayback.DefaultBool = false;
			ConfigAccess.DisablePlayback2 = disablePlayback;
			ConfigInstance disableHORTACam = default(ConfigInstance);
			disableHORTACam.Context = "Game";
			disableHORTACam.Key = "DisableHORTACam";
			disableHORTACam.DefaultBool = false;
			ConfigAccess.DisableHORTACam = disableHORTACam;
			ConfigInstance disableBotsV = default(ConfigInstance);
			disableBotsV.Context = "Game";
			disableBotsV.Key = "DisableBotsV2";
			disableBotsV.DefaultBool = false;
			ConfigAccess.DisableBotsV2 = disableBotsV;
			ConfigInstance enableBotsV = default(ConfigInstance);
			enableBotsV.Context = "Game";
			enableBotsV.Key = "EnableBotsV3";
			enableBotsV.DefaultBool = false;
			ConfigAccess.EnableBotsV3 = enableBotsV;
			ConfigInstance disableCamV = default(ConfigInstance);
			disableCamV.Context = "Game";
			disableCamV.Key = "DisableCamV2";
			disableCamV.DefaultBool = false;
			ConfigAccess.DisableCamV2 = disableCamV;
			ConfigInstance enableVoice = default(ConfigInstance);
			enableVoice.Context = "Game";
			enableVoice.Key = "EnableVoice2";
			enableVoice.DefaultBool = true;
			ConfigAccess.EnableVoice2 = enableVoice;
			ConfigInstance welcomeState = default(ConfigInstance);
			welcomeState.Context = "Game";
			welcomeState.Key = "WelcomeState";
			welcomeState.DefaultBool = true;
			ConfigAccess.WelcomeState = welcomeState;
			ConfigInstance targetFramerateServer = default(ConfigInstance);
			targetFramerateServer.Context = "Debug";
			targetFramerateServer.Key = "TargetFramerate";
			targetFramerateServer.DefaultInt = 60;
			ConfigAccess.TargetFramerateServer = targetFramerateServer;
			ConfigInstance forceUnityLog = default(ConfigInstance);
			forceUnityLog.Context = "Debug";
			forceUnityLog.Key = "ForceUnityLog";
			forceUnityLog.DefaultBool = false;
			ConfigAccess.ForceUnityLog = forceUnityLog;
			ConfigInstance skipSwordfish = default(ConfigInstance);
			skipSwordfish.Context = "Debug";
			skipSwordfish.Key = "SkipSwordfish";
			skipSwordfish.DefaultBool = false;
			ConfigAccess.SkipSwordfish = skipSwordfish;
			ConfigInstance playerName = default(ConfigInstance);
			playerName.Context = "Debug";
			playerName.Key = "PlayerName";
			playerName.DefaultString = "DefaultPlayerName" + Random.Range(0, 1000);
			ConfigAccess.PlayerName = playerName;
			ConfigInstance isDebug = default(ConfigInstance);
			isDebug.Context = "Debug";
			isDebug.Key = "IsDebug";
			isDebug.DefaultBool = false;
			ConfigAccess.IsDebug = isDebug;
			ConfigInstance autoTest = default(ConfigInstance);
			autoTest.Context = "Debug";
			autoTest.Key = "AutoTest";
			autoTest.DefaultBool = false;
			ConfigAccess.AutoTest = autoTest;
			ConfigInstance autoTestTimeInSeconds = default(ConfigInstance);
			autoTestTimeInSeconds.Context = "Debug";
			autoTestTimeInSeconds.Key = "AutoTestTimeInSeconds";
			autoTestTimeInSeconds.DefaultInt = 600;
			ConfigAccess.AutoTestTimeInSeconds = autoTestTimeInSeconds;
			ConfigInstance fastTestChar = default(ConfigInstance);
			fastTestChar.Context = "Debug";
			fastTestChar.Key = "FastTestChar";
			fastTestChar.DefaultInt = -1;
			ConfigAccess.FastTestChar = fastTestChar;
			ConfigInstance delegateDebug = default(ConfigInstance);
			delegateDebug.Context = "Debug";
			delegateDebug.Key = "DelegateDebug";
			delegateDebug.DefaultBool = false;
			ConfigAccess.DelegateDebug = delegateDebug;
			ConfigInstance fmodliveUpdate = default(ConfigInstance);
			fmodliveUpdate.Context = "Debug";
			fmodliveUpdate.Key = "FMODLiveUpdate";
			fmodliveUpdate.DefaultBool = false;
			ConfigAccess.FMODLiveUpdate = fmodliveUpdate;
			ConfigInstance fmoddebug = default(ConfigInstance);
			fmoddebug.Context = "Debug";
			fmoddebug.Key = "FMODDebug";
			fmoddebug.DefaultBool = false;
			ConfigAccess.FMODDebug = fmoddebug;
			ConfigInstance ignoreTournamentClientRequirements = default(ConfigInstance);
			ignoreTournamentClientRequirements.Context = "Debug";
			ignoreTournamentClientRequirements.Key = "IgnoreTournamentClientRequirements";
			ignoreTournamentClientRequirements.DefaultBool = false;
			ConfigAccess.IgnoreTournamentClientRequirements = ignoreTournamentClientRequirements;
			ConfigInstance ignoreTeamInviteRequirements = default(ConfigInstance);
			ignoreTeamInviteRequirements.Context = "Debug";
			ignoreTeamInviteRequirements.Key = "IgnoreTeamInviteRequirements";
			ignoreTeamInviteRequirements.DefaultBool = false;
			ConfigAccess.IgnoreTeamInviteRequirements = ignoreTeamInviteRequirements;
			ConfigInstance enableAltF4Hack = default(ConfigInstance);
			enableAltF4Hack.Context = "Debug";
			enableAltF4Hack.Key = "EnableAltF4Hack";
			enableAltF4Hack.DefaultBool = true;
			ConfigAccess.EnableAltF4Hack = enableAltF4Hack;
			ConfigInstance prefabUsageReport = default(ConfigInstance);
			prefabUsageReport.Context = "Debug";
			prefabUsageReport.Key = "PrefabUsageReport";
			prefabUsageReport.DefaultBool = false;
			ConfigAccess.PrefabUsageReport = prefabUsageReport;
			ConfigInstance enableFakeRankingDataHack = default(ConfigInstance);
			enableFakeRankingDataHack.Context = "Debug";
			enableFakeRankingDataHack.Key = "EnableFakeRankingDataHack";
			enableFakeRankingDataHack.DefaultBool = false;
			ConfigAccess.EnableFakeRankingDataHack = enableFakeRankingDataHack;
			ConfigInstance directMatch = default(ConfigInstance);
			directMatch.Context = "Debug";
			directMatch.Key = "DirectMatch";
			directMatch.DefaultBool = false;
			ConfigAccess.DirectMatch = directMatch;
			ConfigInstance rotation = default(ConfigInstance);
			rotation.Context = "Debug";
			rotation.Key = "Rotation";
			rotation.DefaultString = string.Empty;
			ConfigAccess.Rotation = rotation;
			ConfigInstance allowSameCharacter = default(ConfigInstance);
			allowSameCharacter.Context = "Debug";
			allowSameCharacter.Key = "AllowSameCharacter";
			allowSameCharacter.DefaultBool = false;
			ConfigAccess.AllowSameCharacter = allowSameCharacter;
			ConfigInstance forceEmoteEquip = default(ConfigInstance);
			forceEmoteEquip.Context = "Debug";
			forceEmoteEquip.Key = "ForceEmoteEquip";
			forceEmoteEquip.DefaultString = "00000000-0000-0000-0000-000000000000";
			ConfigAccess.ForceEmoteEquip = forceEmoteEquip;
			ConfigInstance matchKind = default(ConfigInstance);
			matchKind.Context = "Debug";
			matchKind.Key = "MatchKind";
			matchKind.DefaultInt = 0;
			ConfigAccess.MatchKind = matchKind;
			ConfigInstance sfgameUser = default(ConfigInstance);
			sfgameUser.Context = "Game";
			sfgameUser.Key = "SFGameUser";
			sfgameUser.DefaultString = "FakeUser";
			ConfigAccess.SFGameUser = sfgameUser;
			ConfigInstance sfgamePass = default(ConfigInstance);
			sfgamePass.Context = "Game";
			sfgamePass.Key = "SFGamePass";
			sfgamePass.DefaultString = "FakeUserPass";
			ConfigAccess.SFGamePass = sfgamePass;
			ConfigInstance startingScrap = default(ConfigInstance);
			startingScrap.Context = "Game";
			startingScrap.Key = "StartingScrap";
			startingScrap.DefaultInt = 300;
			ConfigAccess.StartingScrap = startingScrap;
			ConfigInstance serverTimeZone = default(ConfigInstance);
			serverTimeZone.Context = "Game";
			serverTimeZone.Key = "ServerTimeZone";
			serverTimeZone.DefaultInt = 0;
			ConfigAccess.ServerTimeZone = serverTimeZone;
			ConfigInstance pickMode = default(ConfigInstance);
			pickMode.Context = "Game";
			pickMode.Key = "PickMode";
			pickMode.DefaultInt = 1;
			ConfigAccess.PickMode = pickMode;
			ConfigInstance arenaIndex = default(ConfigInstance);
			arenaIndex.Context = "Game";
			arenaIndex.Key = "ArenaIndex";
			arenaIndex.DefaultInt = 1;
			ConfigAccess.ArenaIndex = arenaIndex;
			ConfigInstance playerCount = default(ConfigInstance);
			playerCount.Context = "Game";
			playerCount.Key = "PlayerCount";
			playerCount.DefaultInt = 1;
			ConfigAccess.PlayerCount = playerCount;
			ConfigInstance allPlayersOnBluTeam = default(ConfigInstance);
			allPlayersOnBluTeam.Context = "Game";
			allPlayersOnBluTeam.Key = "AllPlayersOnBluTeam";
			allPlayersOnBluTeam.DefaultBool = false;
			ConfigAccess.AllPlayersOnBluTeam = allPlayersOnBluTeam;
			ConfigInstance firstPlayerOnRedTeam = default(ConfigInstance);
			firstPlayerOnRedTeam.Context = "Game";
			firstPlayerOnRedTeam.Key = "FirstPlayerOnRedTeam";
			firstPlayerOnRedTeam.DefaultBool = false;
			ConfigAccess.FirstPlayerOnRedTeam = firstPlayerOnRedTeam;
			ConfigInstance allowCameraMovement = default(ConfigInstance);
			allowCameraMovement.Context = "Game";
			allowCameraMovement.Key = "AllowCameraMovement";
			allowCameraMovement.DefaultBool = false;
			ConfigAccess.AllowCameraMovement = allowCameraMovement;
			ConfigInstance disableAudioKindParticle = default(ConfigInstance);
			disableAudioKindParticle.Context = "Game";
			disableAudioKindParticle.Key = "DisableAudioKindParticle";
			disableAudioKindParticle.DefaultBool = false;
			ConfigAccess.DisableAudioKindParticle = disableAudioKindParticle;
			ConfigInstance redTeamBotsCount = default(ConfigInstance);
			redTeamBotsCount.Context = "Game";
			redTeamBotsCount.Key = "RedTeamBotsCount";
			redTeamBotsCount.DefaultInt = 0;
			ConfigAccess.RedTeamBotsCount = redTeamBotsCount;
			ConfigInstance bluTeamBotsCount = default(ConfigInstance);
			bluTeamBotsCount.Context = "Game";
			bluTeamBotsCount.Key = "BluTeamBotsCount";
			bluTeamBotsCount.DefaultInt = 0;
			ConfigAccess.BluTeamBotsCount = bluTeamBotsCount;
			ConfigInstance skipTutorial = default(ConfigInstance);
			skipTutorial.Context = "Game";
			skipTutorial.Key = "SkipTutorial";
			skipTutorial.DefaultBool = false;
			ConfigAccess.SkipTutorial = skipTutorial;
			ConfigInstance skipSplashPlayer = default(ConfigInstance);
			skipSplashPlayer.Context = "Game";
			skipSplashPlayer.Key = "SkipSplashPlayer";
			skipSplashPlayer.DefaultBool = false;
			ConfigAccess.SkipSplashPlayer = skipSplashPlayer;
			ConfigInstance skipTutorialSplashes = default(ConfigInstance);
			skipTutorialSplashes.Context = "Game";
			skipTutorialSplashes.Key = "SkipTutorialSplashes";
			skipTutorialSplashes.DefaultBool = false;
			ConfigAccess.SkipTutorialSplashes = skipTutorialSplashes;
			ConfigInstance targetFixedStepClient = default(ConfigInstance);
			targetFixedStepClient.Context = "Game";
			targetFixedStepClient.Key = "TargetFixedStep";
			targetFixedStepClient.DefaultFloat = 0.25f;
			ConfigAccess.TargetFixedStepClient = targetFixedStepClient;
			ConfigInstance clipCursor = default(ConfigInstance);
			clipCursor.Context = "Game";
			clipCursor.Key = "ClipCursor";
			clipCursor.DefaultBool = true;
			ConfigAccess.ClipCursor = clipCursor;
			ConfigInstance allCharactersFree = default(ConfigInstance);
			allCharactersFree.Context = "Game";
			allCharactersFree.Key = "AllCharactersFree";
			allCharactersFree.DefaultBool = false;
			ConfigAccess.AllCharactersFree = allCharactersFree;
			ConfigInstance chatComponentEnable = default(ConfigInstance);
			chatComponentEnable.Context = "Game";
			chatComponentEnable.Key = "ChatComponentEnable";
			chatComponentEnable.DefaultBool = false;
			ConfigAccess.ChatComponentEnable = chatComponentEnable;
			ConfigInstance netClientAuthenticationMaxAttempts = default(ConfigInstance);
			netClientAuthenticationMaxAttempts.Context = "Game";
			netClientAuthenticationMaxAttempts.Key = "AuthenticationMaxAttempts";
			netClientAuthenticationMaxAttempts.DefaultInt = 7;
			ConfigAccess.NetClientAuthenticationMaxAttempts = netClientAuthenticationMaxAttempts;
			ConfigInstance netClientAuthenticationRetryWaitTimeInSeconds = default(ConfigInstance);
			netClientAuthenticationRetryWaitTimeInSeconds.Context = "Game";
			netClientAuthenticationRetryWaitTimeInSeconds.Key = "AuthenticationRetryWaitTimeInSeconds";
			netClientAuthenticationRetryWaitTimeInSeconds.DefaultInt = 3;
			ConfigAccess.NetClientAuthenticationRetryWaitTimeInSeconds = netClientAuthenticationRetryWaitTimeInSeconds;
			ConfigInstance raceStartCursorLockEnable = default(ConfigInstance);
			raceStartCursorLockEnable.Context = "Game";
			raceStartCursorLockEnable.Key = "RaceStartCursorLockEnable";
			raceStartCursorLockEnable.DefaultBool = true;
			ConfigAccess.RaceStartCursorLockEnable = raceStartCursorLockEnable;
			ConfigInstance disabledTestAB = default(ConfigInstance);
			disabledTestAB.Context = "Game";
			disabledTestAB.Key = "DisabledTestAB";
			disabledTestAB.DefaultBool = false;
			ConfigAccess.DisabledTestAB = disabledTestAB;
			ConfigInstance noviceTrials = default(ConfigInstance);
			noviceTrials.Context = "Game";
			noviceTrials.Key = "NoviceTrials";
			noviceTrials.DefaultInt = 0;
			ConfigAccess.NoviceTrials = noviceTrials;
			ConfigInstance noviceTrialsABTest = default(ConfigInstance);
			noviceTrialsABTest.Context = "Game";
			noviceTrialsABTest.Key = "NoviceTrialsABTest";
			noviceTrialsABTest.DefaultBool = false;
			ConfigAccess.NoviceTrialsABTest = noviceTrialsABTest;
			ConfigInstance sfgameName = default(ConfigInstance);
			sfgameName.Context = "Swordfish";
			sfgameName.Key = "GameName";
			sfgameName.DefaultString = "FakeGame";
			ConfigAccess.SFGameName = sfgameName;
			ConfigInstance sfmatchMakingAcceptTimeout = default(ConfigInstance);
			sfmatchMakingAcceptTimeout.Context = "Swordfish";
			sfmatchMakingAcceptTimeout.Key = "SFMatchMakingAcceptTimeout";
			sfmatchMakingAcceptTimeout.DefaultInt = 30000;
			ConfigAccess.SFMatchMakingAcceptTimeout = sfmatchMakingAcceptTimeout;
			ConfigInstance sfbaseUrl = default(ConfigInstance);
			sfbaseUrl.Context = "Swordfish";
			sfbaseUrl.Key = "BaseUrl";
			sfbaseUrl.DefaultString = "http://localhost/swordfish/";
			ConfigAccess.SFBaseUrl = sfbaseUrl;
			ConfigInstance sftimeout = default(ConfigInstance);
			sftimeout.Context = "Swordfish";
			sftimeout.Key = "Timeout";
			sftimeout.DefaultInt = 75000;
			ConfigAccess.SFTimeout = sftimeout;
			ConfigInstance sfmaxRetries = default(ConfigInstance);
			sfmaxRetries.Context = "Swordfish";
			sfmaxRetries.Key = "MaxRetries";
			sfmaxRetries.DefaultInt = 3;
			ConfigAccess.SFMaxRetries = sfmaxRetries;
			ConfigInstance sfmatchMakingTimeout = default(ConfigInstance);
			sfmatchMakingTimeout.Context = "Swordfish";
			sfmatchMakingTimeout.Key = "SFMatchMakingTimeout";
			sfmatchMakingTimeout.DefaultInt = 10000;
			ConfigAccess.SFMatchMakingTimeout = sfmatchMakingTimeout;
			ConfigInstance sfdisableBI = default(ConfigInstance);
			sfdisableBI.Context = "Swordfish";
			sfdisableBI.Key = "DisableBI";
			sfdisableBI.DefaultBool = false;
			ConfigAccess.SFDisableBI = sfdisableBI;
			ConfigInstance sfcycleIntervalToDetectOffline = default(ConfigInstance);
			sfcycleIntervalToDetectOffline.Context = "Swordfish";
			sfcycleIntervalToDetectOffline.Key = "SFCycleIntervalToDetectOffline";
			sfcycleIntervalToDetectOffline.DefaultInt = 10000;
			ConfigAccess.SFCycleIntervalToDetectOffline = sfcycleIntervalToDetectOffline;
			ConfigInstance sfnumberOfRetryToDetectOffline = default(ConfigInstance);
			sfnumberOfRetryToDetectOffline.Context = "Swordfish";
			sfnumberOfRetryToDetectOffline.Key = "SFNumberOfRetryToDetectOffline";
			sfnumberOfRetryToDetectOffline.DefaultInt = 5;
			ConfigAccess.SFNumberOfRetryToDetectOffline = sfnumberOfRetryToDetectOffline;
			ConfigInstance sfwebRequestTimeoutInMillis = default(ConfigInstance);
			sfwebRequestTimeoutInMillis.Context = "Swordfish";
			sfwebRequestTimeoutInMillis.Key = "SFWebRequestTimeoutInMillis";
			sfwebRequestTimeoutInMillis.DefaultInt = 5000;
			ConfigAccess.SFWebRequestTimeoutInMillis = sfwebRequestTimeoutInMillis;
			ConfigInstance sfnewsUrl = default(ConfigInstance);
			sfnewsUrl.Context = "Swordfish";
			sfnewsUrl.Key = "NewsUrl";
			sfnewsUrl.DefaultString = "http://heavymetalmachines.com/publisher/ingame/v2/";
			ConfigAccess.SFNewsUrl = sfnewsUrl;
			ConfigInstance sfhelpUrl = default(ConfigInstance);
			sfhelpUrl.Context = "Swordfish";
			sfhelpUrl.Key = "HelpUrl";
			sfhelpUrl.DefaultString = "http://www.heavymetalmachines.com/help/index.php";
			ConfigAccess.SFHelpUrl = sfhelpUrl;
			ConfigInstance metalSponsorsUrl = default(ConfigInstance);
			metalSponsorsUrl.Context = "Swordfish";
			metalSponsorsUrl.Key = "GetFreeItemsUrl";
			metalSponsorsUrl.DefaultString = "https://www.heavymetalmachines.com/metal-sponsors/";
			ConfigAccess.MetalSponsorsUrl = metalSponsorsUrl;
			ConfigInstance sfleaderboardUrl = default(ConfigInstance);
			sfleaderboardUrl.Context = "Swordfish";
			sfleaderboardUrl.Key = "LeaderboardUrl";
			sfleaderboardUrl.DefaultString = "http://www.heavymetalmachines.com/leaderboard/index.php";
			ConfigAccess.SFLeaderboardUrl = sfleaderboardUrl;
			ConfigInstance sfteamsUrl = default(ConfigInstance);
			sfteamsUrl.Context = "Swordfish";
			sfteamsUrl.Key = "TeamsUrl";
			sfteamsUrl.DefaultString = "https://www.heavymetalmachines.com/team/";
			ConfigAccess.SFTeamsUrl = sfteamsUrl;
			ConfigInstance sfteamsSearchUrl = default(ConfigInstance);
			sfteamsSearchUrl.Context = "Swordfish";
			sfteamsSearchUrl.Key = "SFTeamsSearchUrl";
			sfteamsSearchUrl.DefaultString = "https://www.heavymetalmachines.com/teammates/";
			ConfigAccess.SFTeamsSearchUrl = sfteamsSearchUrl;
			ConfigInstance sfNetMaximumConnections = default(ConfigInstance);
			sfNetMaximumConnections.Context = "SfNetwork";
			sfNetMaximumConnections.Key = "MaximumConnections";
			sfNetMaximumConnections.DefaultInt = 20;
			ConfigAccess.SfNetMaximumConnections = sfNetMaximumConnections;
			ConfigInstance sfNetClientRecycledMessagesPoolSize = default(ConfigInstance);
			sfNetClientRecycledMessagesPoolSize.Context = "SfNetwork";
			sfNetClientRecycledMessagesPoolSize.Key = "ClientRecycledMessagesPoolSize";
			sfNetClientRecycledMessagesPoolSize.DefaultInt = 500;
			ConfigAccess.SfNetClientRecycledMessagesPoolSize = sfNetClientRecycledMessagesPoolSize;
			ConfigInstance sfNetServerRecycledMessagesPoolSize = default(ConfigInstance);
			sfNetServerRecycledMessagesPoolSize.Context = "SfNetwork";
			sfNetServerRecycledMessagesPoolSize.Key = "ServerRecycledMessagesPoolSize";
			sfNetServerRecycledMessagesPoolSize.DefaultInt = 1000;
			ConfigAccess.SfNetServerRecycledMessagesPoolSize = sfNetServerRecycledMessagesPoolSize;
			ConfigInstance sfNetHeartbeatInterval = default(ConfigInstance);
			sfNetHeartbeatInterval.Context = "SfNetwork";
			sfNetHeartbeatInterval.Key = "HeartbeatInterval";
			sfNetHeartbeatInterval.DefaultFloat = 0.25f;
			ConfigAccess.SfNetHeartbeatInterval = sfNetHeartbeatInterval;
			ConfigInstance sfNetConnectionTimeout = default(ConfigInstance);
			sfNetConnectionTimeout.Context = "SfNetwork";
			sfNetConnectionTimeout.Key = "ConnectionTimeout";
			sfNetConnectionTimeout.DefaultInt = 10;
			ConfigAccess.SfNetConnectionTimeout = sfNetConnectionTimeout;
			ConfigInstance sfNetMaximumTransmissionUnit = default(ConfigInstance);
			sfNetMaximumTransmissionUnit.Context = "SfNetwork";
			sfNetMaximumTransmissionUnit.Key = "MaximumTransmissionUnit";
			sfNetMaximumTransmissionUnit.DefaultInt = 508;
			ConfigAccess.SfNetMaximumTransmissionUnit = sfNetMaximumTransmissionUnit;
			ConfigInstance sfNetAutoExpandMtu = default(ConfigInstance);
			sfNetAutoExpandMtu.Context = "SfNetwork";
			sfNetAutoExpandMtu.Key = "AutoExpandMTU";
			sfNetAutoExpandMtu.DefaultBool = false;
			ConfigAccess.SfNetAutoExpandMtu = sfNetAutoExpandMtu;
			ConfigInstance sfNetServerAverageStatisticsInterval = default(ConfigInstance);
			sfNetServerAverageStatisticsInterval.Context = "SfNetwork";
			sfNetServerAverageStatisticsInterval.Key = "ServerAverageStatisticsInterval";
			sfNetServerAverageStatisticsInterval.DefaultInt = 60;
			ConfigAccess.SfNetServerAverageStatisticsInterval = sfNetServerAverageStatisticsInterval;
			ConfigInstance sfNetClientAverageStatisticsInterval = default(ConfigInstance);
			sfNetClientAverageStatisticsInterval.Context = "SfNetwork";
			sfNetClientAverageStatisticsInterval.Key = "ClientAverageStatisticsInterval";
			sfNetClientAverageStatisticsInterval.DefaultInt = 60;
			ConfigAccess.SfNetClientAverageStatisticsInterval = sfNetClientAverageStatisticsInterval;
			ConfigInstance sfNetReliableSendCount = default(ConfigInstance);
			sfNetReliableSendCount.Context = "SfNetwork";
			sfNetReliableSendCount.Key = "ReliableSendCount";
			sfNetReliableSendCount.DefaultInt = 3;
			ConfigAccess.SfNetReliableSendCount = sfNetReliableSendCount;
			ConfigInstance sfNetStatisticsLogFilePath = default(ConfigInstance);
			sfNetStatisticsLogFilePath.Context = "SfNetwork";
			sfNetStatisticsLogFilePath.Key = "StatisticsLogFilePath";
			sfNetStatisticsLogFilePath.DefaultString = ".\\logs";
			ConfigAccess.SfNetStatisticsLogFilePath = sfNetStatisticsLogFilePath;
			ConfigInstance sfNetSimulatedMinimumLatency = default(ConfigInstance);
			sfNetSimulatedMinimumLatency.Context = "SfNetwork";
			sfNetSimulatedMinimumLatency.Key = "SimulatedMinimumLatency";
			sfNetSimulatedMinimumLatency.DefaultFloat = 0.0375f;
			ConfigAccess.SfNetSimulatedMinimumLatency = sfNetSimulatedMinimumLatency;
			ConfigInstance sfNetSimulatedRandomLatency = default(ConfigInstance);
			sfNetSimulatedRandomLatency.Context = "SfNetwork";
			sfNetSimulatedRandomLatency.Key = "SimulatedRandomLatency";
			sfNetSimulatedRandomLatency.DefaultFloat = 0.005f;
			ConfigAccess.SfNetSimulatedRandomLatency = sfNetSimulatedRandomLatency;
			ConfigInstance sfNetSimulatedLoss = default(ConfigInstance);
			sfNetSimulatedLoss.Context = "SfNetwork";
			sfNetSimulatedLoss.Key = "SimulatedLoss";
			sfNetSimulatedLoss.DefaultFloat = 0f;
			ConfigAccess.SfNetSimulatedLoss = sfNetSimulatedLoss;
			ConfigInstance sfNetSimulatedDuplicates = default(ConfigInstance);
			sfNetSimulatedDuplicates.Context = "SfNetwork";
			sfNetSimulatedDuplicates.Key = "SimulatedDuplicates";
			sfNetSimulatedDuplicates.DefaultFloat = 0f;
			ConfigAccess.SfNetSimulatedDuplicates = sfNetSimulatedDuplicates;
			ConfigInstance sfNetSimulatedDuplicatesCount = default(ConfigInstance);
			sfNetSimulatedDuplicatesCount.Context = "SfNetwork";
			sfNetSimulatedDuplicatesCount.Key = "SimulatedDuplicatesCount";
			sfNetSimulatedDuplicatesCount.DefaultInt = 0;
			ConfigAccess.SfNetSimulatedDuplicatesCount = sfNetSimulatedDuplicatesCount;
			ConfigInstance sfNetSimulatedOutOfOrder = default(ConfigInstance);
			sfNetSimulatedOutOfOrder.Context = "SfNetwork";
			sfNetSimulatedOutOfOrder.Key = "SimulatedOutOfOrder";
			sfNetSimulatedOutOfOrder.DefaultFloat = 0f;
			ConfigAccess.SfNetSimulatedOutOfOrder = sfNetSimulatedOutOfOrder;
			ConfigInstance sfNetSimulatedCorruption = default(ConfigInstance);
			sfNetSimulatedCorruption.Context = "SfNetwork";
			sfNetSimulatedCorruption.Key = "SimulatedCorruption";
			sfNetSimulatedCorruption.DefaultFloat = 0f;
			ConfigAccess.SfNetSimulatedCorruption = sfNetSimulatedCorruption;
			ConfigInstance sfNetClockSyncTimeout = default(ConfigInstance);
			sfNetClockSyncTimeout.Context = "SfNetwork";
			sfNetClockSyncTimeout.Key = "ClockSyncTimeout";
			sfNetClockSyncTimeout.DefaultInt = 15;
			ConfigAccess.SfNetClockSyncTimeout = sfNetClockSyncTimeout;
			ConfigInstance miniMapUpdateDelay = default(ConfigInstance);
			miniMapUpdateDelay.Context = "Game";
			miniMapUpdateDelay.Key = "MinimapUpdateDelay";
			miniMapUpdateDelay.DefaultFloat = 0.032f;
			ConfigAccess.MiniMapUpdateDelay = miniMapUpdateDelay;
			ConfigInstance afklimit = default(ConfigInstance);
			afklimit.Context = "Game";
			afklimit.Key = "AFKLimit";
			afklimit.DefaultFloat = 120f;
			ConfigAccess.AFKLimit = afklimit;
			ConfigInstance showWelcomePage = default(ConfigInstance);
			showWelcomePage.Context = "Game";
			showWelcomePage.Key = "ShowWelcomePage";
			showWelcomePage.DefaultBool = true;
			ConfigAccess.ShowWelcomePage = showWelcomePage;
			ConfigInstance welcomePageURL = default(ConfigInstance);
			welcomePageURL.Context = "Game";
			welcomePageURL.Key = "WelcomePageURL";
			welcomePageURL.DefaultString = string.Empty;
			ConfigAccess.WelcomePageURL = welcomePageURL;
			ConfigInstance debugClusterAuth = default(ConfigInstance);
			debugClusterAuth.Context = "Debug";
			debugClusterAuth.Key = "ClusterAuth";
			debugClusterAuth.DefaultBool = false;
			ConfigAccess.DebugClusterAuth = debugClusterAuth;
			ConfigInstance narratorHostFilter = default(ConfigInstance);
			narratorHostFilter.Context = "Game";
			narratorHostFilter.Key = "NarratorHostFilter";
			narratorHostFilter.DefaultString = "37.58.69.242";
			ConfigAccess.NarratorHostFilter = narratorHostFilter;
			ConfigInstance forcePvP = default(ConfigInstance);
			forcePvP.Context = "Debug";
			forcePvP.Key = "ForcePvP";
			forcePvP.DefaultBool = false;
			ConfigAccess.ForcePvP = forcePvP;
			ConfigInstance skipSFMissionConfigHack = default(ConfigInstance);
			skipSFMissionConfigHack.Context = "Debug";
			skipSFMissionConfigHack.Key = "SkipSFMissionConfigHack";
			skipSFMissionConfigHack.DefaultString = string.Empty;
			ConfigAccess.SkipSFMissionConfigHack = skipSFMissionConfigHack;
			ConfigInstance ignoreAutoTournamentJoin = default(ConfigInstance);
			ignoreAutoTournamentJoin.Context = "Debug";
			ignoreAutoTournamentJoin.Key = "IgnoreAutoTournamentJoin";
			ignoreAutoTournamentJoin.DefaultBool = false;
			ConfigAccess.IgnoreAutoTournamentJoin = ignoreAutoTournamentJoin;
			ConfigInstance defaultRegionName = default(ConfigInstance);
			defaultRegionName.Context = "RegionServer";
			defaultRegionName.Key = "DefaultRegionName";
			defaultRegionName.DefaultString = "EUR-AMSTERDAM";
			ConfigAccess.DefaultRegionName = defaultRegionName;
			ConfigInstance blueMMR = default(ConfigInstance);
			blueMMR.Context = "Debug";
			blueMMR.Key = "BlueMMR";
			blueMMR.DefaultInt = 500;
			ConfigAccess.BlueMMR = blueMMR;
			ConfigInstance redMMR = default(ConfigInstance);
			redMMR.Context = "Debug";
			redMMR.Key = "RedMMR";
			redMMR.DefaultInt = 500;
			ConfigAccess.RedMMR = redMMR;
			ConfigInstance flipVfxTeamColors = default(ConfigInstance);
			flipVfxTeamColors.Context = "Debug";
			flipVfxTeamColors.Key = "FlipVfxTeamColors";
			flipVfxTeamColors.DefaultBool = false;
			ConfigAccess.FlipVfxTeamColors = flipVfxTeamColors;
			ConfigInstance teamsMainMenuTimeoutInSeconds = default(ConfigInstance);
			teamsMainMenuTimeoutInSeconds.Context = "Game";
			teamsMainMenuTimeoutInSeconds.Key = "TeamsMainMenuTimeoutInSeconds";
			teamsMainMenuTimeoutInSeconds.DefaultInt = 30;
			ConfigAccess.TeamsMainMenuTimeoutInSeconds = teamsMainMenuTimeoutInSeconds;
			ConfigInstance supportPageURL = default(ConfigInstance);
			supportPageURL.Context = "Game";
			supportPageURL.Key = "SupportPageURL";
			supportPageURL.DefaultString = "http://www.heavymetalmachines.com/support/";
			ConfigAccess.SupportPageURL = supportPageURL;
			ConfigInstance facebookBRPageURL = default(ConfigInstance);
			facebookBRPageURL.Context = "Game";
			facebookBRPageURL.Key = "FacebookBRPageURL";
			facebookBRPageURL.DefaultString = "http://t.hoplon.com/tracker?offer_id=1384";
			ConfigAccess.FacebookBRPageURL = facebookBRPageURL;
			ConfigInstance facebookPageURL = default(ConfigInstance);
			facebookPageURL.Context = "Game";
			facebookPageURL.Key = "FacebookPageURL";
			facebookPageURL.DefaultString = "http://t.hoplon.com/tracker?offer_id=1383";
			ConfigAccess.FacebookPageURL = facebookPageURL;
			ConfigInstance vkPageURL = default(ConfigInstance);
			vkPageURL.Context = "Game";
			vkPageURL.Key = "VkPageURL";
			vkPageURL.DefaultString = "http://t.hoplon.com/tracker?offer_id=1390";
			ConfigAccess.VkPageURL = vkPageURL;
			ConfigInstance discordPageURL = default(ConfigInstance);
			discordPageURL.Context = "Game";
			discordPageURL.Key = "DiscordPageURL";
			discordPageURL.DefaultString = "http://t.hoplon.com/tracker?offer_id=1387";
			ConfigAccess.DiscordPageURL = discordPageURL;
			ConfigInstance instagramBRPageURL = default(ConfigInstance);
			instagramBRPageURL.Context = "Game";
			instagramBRPageURL.Key = "InstagramBRPageURL";
			instagramBRPageURL.DefaultString = "http://t.hoplon.com/tracker?offer_id=1385";
			ConfigAccess.InstagramBRPageURL = instagramBRPageURL;
			ConfigInstance instagramPageURL = default(ConfigInstance);
			instagramPageURL.Context = "Game";
			instagramPageURL.Key = "InstagramPageURL";
			instagramPageURL.DefaultString = "http://t.hoplon.com/tracker?offer_id=1386";
			ConfigAccess.InstagramPageURL = instagramPageURL;
			ConfigInstance combatTestKnowMoreURL = default(ConfigInstance);
			combatTestKnowMoreURL.Context = "Game";
			combatTestKnowMoreURL.Key = "CombatTestKnowMorePageURL";
			combatTestKnowMoreURL.DefaultString = "http://heavymetalmachines.com/moreinfo/?lang={0}";
			ConfigAccess.CombatTestKnowMoreURL = combatTestKnowMoreURL;
			ConfigInstance twitterBRPageURL = default(ConfigInstance);
			twitterBRPageURL.Context = "Game";
			twitterBRPageURL.Key = "TwitterBRPageURL";
			twitterBRPageURL.DefaultString = "http://t.hoplon.com/tracker?offer_id=3958";
			ConfigAccess.TwitterBRPageURL = twitterBRPageURL;
			ConfigInstance twitterPageURL = default(ConfigInstance);
			twitterPageURL.Context = "Game";
			twitterPageURL.Key = "TwitterPageURL";
			twitterPageURL.DefaultString = "http://t.hoplon.com/tracker?offer_id=3959";
			ConfigAccess.TwitterPageURL = twitterPageURL;
			ConfigInstance characterGuideURL = default(ConfigInstance);
			characterGuideURL.Context = "Game";
			characterGuideURL.Key = "CharacterGuideURL";
			characterGuideURL.DefaultString = "http://heavymetalmachines.com/site/{0}?lang={1}";
			ConfigAccess.CharacterGuideURL = characterGuideURL;
			ConfigInstance enableRedShell = default(ConfigInstance);
			enableRedShell.Context = "Tools";
			enableRedShell.Key = "EnableRedShell";
			enableRedShell.DefaultBool = true;
			ConfigAccess.EnableRedShell = enableRedShell;
			ConfigInstance enableHoplonTT = default(ConfigInstance);
			enableHoplonTT.Context = "Tools";
			enableHoplonTT.Key = "EnableHoplonTT";
			enableHoplonTT.DefaultBool = true;
			ConfigAccess.EnableHoplonTT = enableHoplonTT;
			ConfigInstance hoplonTTUrl = default(ConfigInstance);
			hoplonTTUrl.Context = "Tools";
			hoplonTTUrl.Key = "HoplonTTUrl";
			hoplonTTUrl.DefaultString = "http://t.hoplon.com/conversion";
			ConfigAccess.HoplonTTUrl = hoplonTTUrl;
			ConfigInstance enableHoplonTTEvent = default(ConfigInstance);
			enableHoplonTTEvent.Context = "Tools";
			enableHoplonTTEvent.Key = "EnableHoplonTTEvent";
			enableHoplonTTEvent.DefaultBool = true;
			ConfigAccess.EnableHoplonTTEvent = enableHoplonTTEvent;
			ConfigInstance hoplonTTEventUrl = default(ConfigInstance);
			hoplonTTEventUrl.Context = "Tools";
			hoplonTTEventUrl.Key = "HoplonTTEventUrl";
			hoplonTTEventUrl.DefaultString = "http://t.hoplon.com/event";
			ConfigAccess.HoplonTTEventUrl = hoplonTTEventUrl;
			ConfigInstance horta = default(ConfigInstance);
			horta.Context = "Tools";
			horta.Key = "HORTA";
			horta.DefaultBool = false;
			ConfigAccess.HORTA = horta;
			ConfigInstance hortafile = default(ConfigInstance);
			hortafile.Context = "Tools";
			hortafile.Key = "HORTAFile";
			hortafile.DefaultString = string.Empty;
			ConfigAccess.HORTAFile = hortafile;
			ConfigInstance hortadestFolder = default(ConfigInstance);
			hortadestFolder.Context = "Server";
			hortadestFolder.Key = "MatchFileFolder";
			hortadestFolder.DefaultString = ".\\logs\\";
			ConfigAccess.HORTADestFolder = hortadestFolder;
			ConfigInstance disableRecorder = default(ConfigInstance);
			disableRecorder.Context = "Server";
			disableRecorder.Key = "DisableRecorder";
			disableRecorder.DefaultBool = false;
			ConfigAccess.DisableRecorder = disableRecorder;
			ConfigInstance hortafileUri = default(ConfigInstance);
			hortafileUri.Context = "Tools";
			hortafileUri.Key = "FileUri";
			hortafileUri.DefaultString = string.Empty;
			ConfigAccess.HORTAFileUri = hortafileUri;
			ConfigInstance sfquizUrl = default(ConfigInstance);
			sfquizUrl.Context = "Swordfish";
			sfquizUrl.Key = "QuizUrl";
			sfquizUrl.DefaultString = "http://www.heavymetalmachines.com/quiz/";
			ConfigAccess.SFQuizUrl = sfquizUrl;
			ConfigInstance spectatorZoomClose = default(ConfigInstance);
			spectatorZoomClose.Context = "Game";
			spectatorZoomClose.Key = "SpectatorZoomClose";
			spectatorZoomClose.DefaultFloat = 0.8f;
			ConfigAccess.SpectatorZoomClose = spectatorZoomClose;
			ConfigInstance spectatorZoomNear = default(ConfigInstance);
			spectatorZoomNear.Context = "Game";
			spectatorZoomNear.Key = "SpectatorZoomNear";
			spectatorZoomNear.DefaultFloat = 1.2f;
			ConfigAccess.SpectatorZoomNear = spectatorZoomNear;
			ConfigInstance spectatorZoomFar = default(ConfigInstance);
			spectatorZoomFar.Context = "Game";
			spectatorZoomFar.Key = "SpectatorZoomFar";
			spectatorZoomFar.DefaultFloat = 4.8f;
			ConfigAccess.SpectatorZoomFar = spectatorZoomFar;
			ConfigInstance orbitalCameraZoomSpeed = default(ConfigInstance);
			orbitalCameraZoomSpeed.Context = "Game";
			orbitalCameraZoomSpeed.Key = "OrbitalCameraZoomSpeed";
			ConfigAccess.OrbitalCameraZoomSpeed = orbitalCameraZoomSpeed;
			ConfigInstance orbitalCameraRotationSpeed = default(ConfigInstance);
			orbitalCameraRotationSpeed.Context = "Game";
			orbitalCameraRotationSpeed.Key = "OrbitalCameraRotationSpeed";
			ConfigAccess.OrbitalCameraRotationSpeed = orbitalCameraRotationSpeed;
			ConfigInstance orbitalCameraUnlockSpin = default(ConfigInstance);
			orbitalCameraUnlockSpin.Context = "Game";
			orbitalCameraUnlockSpin.Key = "OrbitalCameraUnlockSpin";
			ConfigAccess.OrbitalCameraUnlockSpin = orbitalCameraUnlockSpin;
			ConfigInstance disableCarRendering = default(ConfigInstance);
			disableCarRendering.Context = "Rendering";
			disableCarRendering.Key = "DisableCarRendering";
			disableCarRendering.DefaultInt = 0;
			ConfigAccess.DisableCarRendering = disableCarRendering;
			ConfigInstance tournamentURL = default(ConfigInstance);
			tournamentURL.Context = "Game";
			tournamentURL.Key = "TournamentURL";
			tournamentURL.DefaultString = "https://www.heavymetalmachines.com/tournament/";
			ConfigAccess.TournamentURL = tournamentURL;
			ConfigInstance enableDrafter = default(ConfigInstance);
			enableDrafter.Context = "Game";
			enableDrafter.Key = "EnableDrafter";
			enableDrafter.DefaultBool = true;
			ConfigAccess.EnableDrafter = enableDrafter;
			ConfigInstance enableToCreateLauncherFileQuiz = default(ConfigInstance);
			enableToCreateLauncherFileQuiz.Context = "Game";
			enableToCreateLauncherFileQuiz.Key = "EnableToCreateLauncherFileQuiz";
			enableToCreateLauncherFileQuiz.DefaultBool = true;
			ConfigAccess.EnableToCreateLauncherFileQuiz = enableToCreateLauncherFileQuiz;
			ConfigInstance periodicRefreshTimeInSeconds = default(ConfigInstance);
			periodicRefreshTimeInSeconds.Context = "Game";
			periodicRefreshTimeInSeconds.Key = "TeamAndTournamentRefreshTimeInSeconds";
			periodicRefreshTimeInSeconds.DefaultFloat = 10f;
			ConfigAccess.PeriodicRefreshTimeInSeconds = periodicRefreshTimeInSeconds;
			ConfigInstance tournamentJoinSolo = default(ConfigInstance);
			tournamentJoinSolo.Context = "Game";
			tournamentJoinSolo.Key = "TournamentJoinSolo";
			tournamentJoinSolo.DefaultBool = false;
			ConfigAccess.TournamentJoinSolo = tournamentJoinSolo;
			ConfigInstance customMatchPS = default(ConfigInstance);
			customMatchPS.Context = "Game";
			customMatchPS.Key = "CustomMatchPS4";
			customMatchPS.DefaultBool = true;
			ConfigAccess.CustomMatchPS4 = customMatchPS;
			ConfigInstance mockedLobby = default(ConfigInstance);
			mockedLobby.Context = "Game";
			mockedLobby.Key = "MockedLobby";
			mockedLobby.DefaultBool = false;
			ConfigAccess.MockedLobby = mockedLobby;
			ConfigInstance redirectUrl = default(ConfigInstance);
			redirectUrl.Context = "Game";
			redirectUrl.Key = "RedirectUrl";
			redirectUrl.DefaultString = "https://redirect.heavymetalmachines.com/";
			ConfigAccess.RedirectUrl = redirectUrl;
			ConfigInstance enableProfileRefactor = default(ConfigInstance);
			enableProfileRefactor.Context = "Game";
			enableProfileRefactor.Key = "EnableProfileRefactor";
			enableProfileRefactor.DefaultBool = false;
			ConfigAccess.EnableProfileRefactor = enableProfileRefactor;
			ConfigInstance publisherIndex = default(ConfigInstance);
			publisherIndex.Context = "Swordfish";
			publisherIndex.Key = "PublisherIndex";
			publisherIndex.DefaultInt = 0;
			ConfigAccess.PublisherIndex = publisherIndex;
			ConfigInstance customMatchEnableRankedWithBots = default(ConfigInstance);
			customMatchEnableRankedWithBots.Context = "Game";
			customMatchEnableRankedWithBots.Key = "CustomMatchEnableRankedWithBots";
			customMatchEnableRankedWithBots.DefaultBool = false;
			ConfigAccess.CustomMatchEnableRankedWithBots = customMatchEnableRankedWithBots;
		}

		public static readonly ConfigInstance[] Team1Characters;

		public static readonly ConfigInstance[] Team2Characters;

		public static readonly ConfigInstance[] Team1Skins;

		public static readonly ConfigInstance[] Team2Skins;

		public static readonly ConfigInstance ForcedFakePublisher;

		public static readonly ConfigInstance ForcedFakePsnCrossplayEnabled;

		public static readonly ConfigInstance ForcedFakePsnCrossplayValue;

		public static readonly ConfigInstance VoiceVolumeAt100;

		public static readonly ConfigInstance VoiceVolumeAt200;

		public static readonly ConfigInstance CustomGCControl;

		public static readonly ConfigInstance MaxGCAllocatedSize;

		public const string CtxLog = "Log";

		public static readonly ConfigInstance MatchLogFile;

		public static readonly ConfigInstance MatchLogEnabled;

		public static readonly ConfigInstance LogLevel;

		public static readonly ConfigInstance LogFilter;

		public static readonly ConfigInstance FreezeLogCount;

		public const string CtxServer = "Server";

		public static readonly ConfigInstance ServerPort;

		public static readonly ConfigInstance ServerIP;

		public static readonly ConfigInstance ServerLoneTimeoutSeconds;

		public static readonly ConfigInstance ServerTutorialLoneTimeoutSeconds;

		public static readonly ConfigInstance TargetFixedStepServer;

		public static readonly ConfigInstance AFKDisconnectionLimit;

		public static readonly ConfigInstance AFKModifierLimit;

		public static readonly ConfigInstance AFKInputLimit;

		public static readonly ConfigInstance LoadingTimeoutSeconds;

		public static readonly ConfigInstance ForceRookie;

		public static readonly ConfigInstance ForceAlwaysRunPathFind;

		public static readonly ConfigInstance EnableTrainingPopUpV3;

		public static readonly ConfigInstance DisablePlayback2;

		public static readonly ConfigInstance DisableHORTACam;

		public static readonly ConfigInstance DisableBotsV2;

		public static readonly ConfigInstance EnableBotsV3;

		public static readonly ConfigInstance DisableCamV2;

		public static readonly ConfigInstance EnableVoice2;

		public static readonly ConfigInstance WelcomeState;

		public const string CtxDebug = "Debug";

		public const string CtxSwfsh = "Swordfish";

		public const string CtxSfNet = "SfNetwork";

		public const string CtxTools = "Tools";

		public const string CtxRendering = "Rendering";

		public const string CtxRegionServer = "RegionServer";

		public static readonly ConfigInstance TargetFramerateServer;

		public static readonly ConfigInstance ForceUnityLog;

		public static readonly ConfigInstance SkipSwordfish;

		public static readonly ConfigInstance PlayerName;

		public static readonly ConfigInstance IsDebug;

		public static readonly ConfigInstance AutoTest;

		public static readonly ConfigInstance AutoTestTimeInSeconds;

		public static readonly ConfigInstance FastTestChar;

		public static readonly ConfigInstance DelegateDebug;

		public static readonly ConfigInstance FMODLiveUpdate;

		public static readonly ConfigInstance FMODDebug;

		public static readonly ConfigInstance IgnoreTournamentClientRequirements;

		public static readonly ConfigInstance IgnoreTeamInviteRequirements;

		public static readonly ConfigInstance EnableAltF4Hack;

		public static readonly ConfigInstance PrefabUsageReport;

		public static readonly ConfigInstance EnableFakeRankingDataHack;

		public static readonly ConfigInstance DirectMatch;

		public static readonly ConfigInstance Rotation;

		public static readonly ConfigInstance AllowSameCharacter;

		public static readonly ConfigInstance ForceEmoteEquip;

		public static readonly ConfigInstance MatchKind;

		public static readonly ConfigInstance SFGameUser;

		public static readonly ConfigInstance SFGamePass;

		public static readonly ConfigInstance StartingScrap;

		public static readonly ConfigInstance ServerTimeZone;

		public static readonly ConfigInstance PickMode;

		public static readonly ConfigInstance ArenaIndex;

		public static readonly ConfigInstance PlayerCount;

		public static readonly ConfigInstance AllPlayersOnBluTeam;

		public static readonly ConfigInstance FirstPlayerOnRedTeam;

		public static readonly ConfigInstance AllowCameraMovement;

		public static readonly ConfigInstance DisableAudioKindParticle;

		public static readonly ConfigInstance RedTeamBotsCount;

		public static readonly ConfigInstance BluTeamBotsCount;

		public static readonly ConfigInstance SkipTutorial;

		public static readonly ConfigInstance SkipSplashPlayer;

		public static readonly ConfigInstance SkipTutorialSplashes;

		public static readonly ConfigInstance TargetFixedStepClient;

		public static readonly ConfigInstance ClipCursor;

		public static readonly ConfigInstance AllCharactersFree;

		public static readonly ConfigInstance ChatComponentEnable;

		public static readonly ConfigInstance NetClientAuthenticationMaxAttempts;

		public static readonly ConfigInstance NetClientAuthenticationRetryWaitTimeInSeconds;

		public static readonly ConfigInstance RaceStartCursorLockEnable;

		public static readonly ConfigInstance DisabledTestAB;

		public static readonly ConfigInstance NoviceTrials;

		public static readonly ConfigInstance NoviceTrialsABTest;

		public static readonly ConfigInstance SFGameName;

		public static readonly ConfigInstance SFMatchMakingAcceptTimeout;

		public static readonly ConfigInstance SFBaseUrl;

		public static readonly ConfigInstance SFTimeout;

		public static readonly ConfigInstance SFMaxRetries;

		public static readonly ConfigInstance SFMatchMakingTimeout;

		public static readonly ConfigInstance SFDisableBI;

		public static readonly ConfigInstance SFCycleIntervalToDetectOffline;

		public static readonly ConfigInstance SFNumberOfRetryToDetectOffline;

		public static readonly ConfigInstance SFWebRequestTimeoutInMillis;

		public static readonly ConfigInstance SFNewsUrl;

		public static readonly ConfigInstance SFHelpUrl;

		public static readonly ConfigInstance MetalSponsorsUrl;

		public static readonly ConfigInstance SFLeaderboardUrl;

		public static readonly ConfigInstance SFTeamsUrl;

		public static readonly ConfigInstance SFTeamsSearchUrl;

		public static readonly ConfigInstance SfNetMaximumConnections;

		public static readonly ConfigInstance SfNetClientRecycledMessagesPoolSize;

		public static readonly ConfigInstance SfNetServerRecycledMessagesPoolSize;

		public static readonly ConfigInstance SfNetHeartbeatInterval;

		public static readonly ConfigInstance SfNetConnectionTimeout;

		public static readonly ConfigInstance SfNetMaximumTransmissionUnit;

		public static readonly ConfigInstance SfNetAutoExpandMtu;

		public static readonly ConfigInstance SfNetServerAverageStatisticsInterval;

		public static readonly ConfigInstance SfNetClientAverageStatisticsInterval;

		public static readonly ConfigInstance SfNetReliableSendCount;

		public static readonly ConfigInstance SfNetStatisticsLogFilePath;

		public static readonly ConfigInstance SfNetSimulatedMinimumLatency;

		public static readonly ConfigInstance SfNetSimulatedRandomLatency;

		public static readonly ConfigInstance SfNetSimulatedLoss;

		public static readonly ConfigInstance SfNetSimulatedDuplicates;

		public static readonly ConfigInstance SfNetSimulatedDuplicatesCount;

		public static readonly ConfigInstance SfNetSimulatedOutOfOrder;

		public static readonly ConfigInstance SfNetSimulatedCorruption;

		public static readonly ConfigInstance SfNetClockSyncTimeout;

		public static readonly ConfigInstance MiniMapUpdateDelay;

		public static readonly ConfigInstance AFKLimit;

		public static readonly ConfigInstance ShowWelcomePage;

		public static readonly ConfigInstance WelcomePageURL;

		public static readonly ConfigInstance DebugClusterAuth;

		public static readonly ConfigInstance NarratorHostFilter;

		public static readonly ConfigInstance ForcePvP;

		public static readonly ConfigInstance SkipSFMissionConfigHack;

		public static readonly ConfigInstance IgnoreAutoTournamentJoin;

		public static readonly ConfigInstance DefaultRegionName;

		public static readonly ConfigInstance BlueMMR;

		public static readonly ConfigInstance RedMMR;

		public static readonly ConfigInstance FlipVfxTeamColors;

		public static readonly ConfigInstance TeamsMainMenuTimeoutInSeconds;

		public static readonly ConfigInstance SupportPageURL;

		public static readonly ConfigInstance FacebookBRPageURL;

		public static readonly ConfigInstance FacebookPageURL;

		public static readonly ConfigInstance VkPageURL;

		public static readonly ConfigInstance DiscordPageURL;

		public static readonly ConfigInstance InstagramBRPageURL;

		public static readonly ConfigInstance InstagramPageURL;

		public static readonly ConfigInstance CombatTestKnowMoreURL;

		public static readonly ConfigInstance TwitterBRPageURL;

		public static readonly ConfigInstance TwitterPageURL;

		public static readonly ConfigInstance CharacterGuideURL;

		public static readonly ConfigInstance EnableRedShell;

		public static readonly ConfigInstance EnableHoplonTT;

		public static readonly ConfigInstance HoplonTTUrl;

		public static readonly ConfigInstance EnableHoplonTTEvent;

		public static readonly ConfigInstance HoplonTTEventUrl;

		public static readonly ConfigInstance HORTA;

		public static readonly ConfigInstance HORTAFile;

		public static readonly ConfigInstance HORTADestFolder;

		public static readonly ConfigInstance DisableRecorder;

		public static readonly ConfigInstance HORTAFileUri;

		public static readonly ConfigInstance SFQuizUrl;

		public static readonly ConfigInstance SpectatorZoomClose;

		public static readonly ConfigInstance SpectatorZoomNear;

		public static readonly ConfigInstance SpectatorZoomFar;

		public static readonly ConfigInstance OrbitalCameraZoomSpeed;

		public static readonly ConfigInstance OrbitalCameraRotationSpeed;

		public static readonly ConfigInstance OrbitalCameraUnlockSpin;

		public static readonly ConfigInstance DisableCarRendering;

		public static readonly ConfigInstance TournamentURL;

		public static readonly ConfigInstance EnableDrafter;

		public static readonly ConfigInstance EnableToCreateLauncherFileQuiz;

		public static readonly ConfigInstance PeriodicRefreshTimeInSeconds;

		public static readonly ConfigInstance TournamentJoinSolo;

		public static readonly ConfigInstance CustomMatchPS4;

		public static readonly ConfigInstance MockedLobby;

		public static readonly ConfigInstance RedirectUrl;

		public static readonly ConfigInstance EnableProfileRefactor;

		public static readonly ConfigInstance PublisherIndex;

		public static readonly ConfigInstance CustomMatchEnableRankedWithBots;
	}
}
