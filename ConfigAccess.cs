using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public static class ConfigAccess
	{
		public const string CtxLog = "Log";

		public const string CtxDebug = "Debug";

		public const string CtxServer = "Server";

		public const string CtxGame = "Game";

		public const string CtxSwfsh = "Swordfish";

		public const string CtxSfNet = "SfNetwork";

		public const string CtxTools = "Tools";

		public static readonly ConfigLoader.ConfigInstance ServerPort = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "Port",
			DefaultInt = 9696
		};

		public static readonly ConfigLoader.ConfigInstance ServerIP = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "IP",
			DefaultString = "127.0.0.1"
		};

		public static readonly ConfigLoader.ConfigInstance ServerLoneTimeoutSeconds = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "LoneTimeout",
			DefaultInt = 300
		};

		public static readonly ConfigLoader.ConfigInstance TargetFixedStepServer = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "TargetFixedStep",
			DefaultFloat = 0.0166666675f
		};

		public static readonly ConfigLoader.ConfigInstance TargetFramerateServer = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "TargetFramerate",
			DefaultInt = 60
		};

		public static readonly ConfigLoader.ConfigInstance ForceUnityLog = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "ForceUnityLog",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance SkipSwordfish = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "SkipSwordfish",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance PlayerName = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "PlayerName",
			DefaultString = "DefaultPlayerName" + UnityEngine.Random.Range(0, 1000)
		};

		public static readonly ConfigLoader.ConfigInstance SkipMatchmaking = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "SkipMatchmaking",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance IsDebug = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "IsDebug",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance AutoTest = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "AutoTest",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance AutoTestTimeInSeconds = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "AutoTestTimeInSeconds",
			DefaultInt = 600
		};

		public static readonly ConfigLoader.ConfigInstance FastTestChar = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "FastTestChar",
			DefaultInt = -1
		};

		public static readonly ConfigLoader.ConfigInstance DelegateDebug = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "DelegateDebug",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance FMODLiveUpdate = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "FMODLiveUpdate",
			DefaultBool = true
		};

		public static readonly ConfigLoader.ConfigInstance FMODLiveUpdatePort = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "FMODLiveUpdatePort",
			DefaultInt = 9264
		};

		public static readonly ConfigLoader.ConfigInstance FMODDebug = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "FMODDebug",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance IgnoreReconnect = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "IgnoreReconnect",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance EnableAltF4Hack = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "EnableAltF4Hack",
			DefaultBool = true
		};

		public static readonly ConfigLoader.ConfigInstance NoRotation = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "NoRotation",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance RotationVeterans = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "RotationVeterans",
			DefaultString = string.Empty
		};

		public static readonly ConfigLoader.ConfigInstance RotationRookies = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "RotationRookies",
			DefaultString = string.Empty
		};

		public static readonly ConfigLoader.ConfigInstance RotationVeteranLevel = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "RotationVeteranLevel",
			DefaultInt = 3
		};

		public static readonly ConfigLoader.ConfigInstance MatchLogFile = new ConfigLoader.ConfigInstance
		{
			Context = "Log",
			Key = "MatchLogFile",
			DefaultString = "match.log"
		};

		public static readonly ConfigLoader.ConfigInstance MatchLogEnabled = new ConfigLoader.ConfigInstance
		{
			Context = "Log",
			Key = "MatchLogEnabled",
			DefaultBool = true
		};

		public static readonly ConfigLoader.ConfigInstance SFGameUser = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "SFGameUser",
			DefaultString = "FakeUser"
		};

		public static readonly ConfigLoader.ConfigInstance SFGamePass = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "SFGamePass",
			DefaultString = "FakeUserPass"
		};

		public static readonly ConfigLoader.ConfigInstance StartingScrap = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "StartingScrap",
			DefaultInt = 300
		};

		public static readonly ConfigLoader.ConfigInstance ServerTimeZone = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "ServerTimeZone",
			DefaultInt = 0
		};

		public static readonly ConfigLoader.ConfigInstance PickMode = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "PickMode",
			DefaultInt = 1
		};

		public static readonly ConfigLoader.ConfigInstance ArenaIndex = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "ArenaIndex",
			DefaultInt = 1
		};

		public static readonly ConfigLoader.ConfigInstance PlayerCount = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "PlayerCount",
			DefaultInt = 1
		};

		public static readonly ConfigLoader.ConfigInstance AllPlayersOnBluTeam = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "AllPlayersOnBluTeam",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance FirstPlayerOnRedTeam = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "FirstPlayerOnRedTeam",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance AllowCameraMovement = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "AllowCameraMovement",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance DisableAudioKindParticle = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "DisableAudioKindParticle",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance RedTeamBotsCount = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "RedTeamBotsCount",
			DefaultInt = 0
		};

		public static readonly ConfigLoader.ConfigInstance BluTeamBotsCount = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "BluTeamBotsCount",
			DefaultInt = 0
		};

		public static readonly ConfigLoader.ConfigInstance SkipTutorial = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "SkipTutorial",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance ForceTutorial = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "ForceTutorial",
			DefaultBool = true
		};

		public static readonly ConfigLoader.ConfigInstance SkipSplashPlayer = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "SkipSplashPlayer",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance SkipTutorialSplashes = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "SkipTutorialSplashes",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance TargetFixedStepClient = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "TargetFixedStep",
			DefaultFloat = 0.25f
		};

		public static readonly ConfigLoader.ConfigInstance ClipCursor = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "ClipCursor",
			DefaultBool = true
		};

		public static readonly ConfigLoader.ConfigInstance AllCharactersFree = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "AllCharactersFree",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance ChatComponentEnable = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "ChatComponentEnable",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance NetClientAuthenticationMaxAttempts = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "AuthenticationMaxAttempts",
			DefaultInt = 7
		};

		public static readonly ConfigLoader.ConfigInstance NetClientAuthenticationRetryWaitTimeInSeconds = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "AuthenticationRetryWaitTimeInSeconds",
			DefaultInt = 3
		};

		public static readonly ConfigLoader.ConfigInstance RaceStartCursorLockEnable = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "RaceStartCursorLockEnable",
			DefaultBool = true
		};

		public static readonly ConfigLoader.ConfigInstance SFGameName = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "GameName",
			DefaultString = "FakeGame"
		};

		public static readonly ConfigLoader.ConfigInstance SFMatchMakingAcceptTimeout = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "SFMatchMakingAcceptTimeout",
			DefaultInt = 30000
		};

		public static readonly ConfigLoader.ConfigInstance SFBaseUrl = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "BaseUrl",
			DefaultString = "http://localhost/swordfish/"
		};

		public static readonly ConfigLoader.ConfigInstance SFTimeout = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "Timeout",
			DefaultInt = 75000
		};

		public static readonly ConfigLoader.ConfigInstance SFMatchMakingTimeout = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "SFMatchMakingTimeout",
			DefaultInt = 30000
		};

		public static readonly ConfigLoader.ConfigInstance SFDisableBI = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "DisableBI",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance SFCycleIntervalToDetectOffline = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "SFCycleIntervalToDetectOffline",
			DefaultInt = 10000
		};

		public static readonly ConfigLoader.ConfigInstance SFNumberOfRetryToDetectOffline = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "SFNumberOfRetryToDetectOffline",
			DefaultInt = 5
		};

		public static readonly ConfigLoader.ConfigInstance SFWebRequestTimeoutInMillis = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "SFWebRequestTimeoutInMillis",
			DefaultInt = 5000
		};

		public static readonly ConfigLoader.ConfigInstance SFNewsUrl = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "NewsUrl",
			DefaultString = "http://heavymetalmachines.com/publisher/ingame/v2/"
		};

		public static readonly ConfigLoader.ConfigInstance SFHelpUrl = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "HelpUrl",
			DefaultString = "http://www.heavymetalmachines.com/help/index.php"
		};

		public static readonly ConfigLoader.ConfigInstance SFLeaderboardUrl = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "LeaderboardUrl",
			DefaultString = "http://www.heavymetalmachines.com/leaderboard/index.php"
		};

		public static readonly ConfigLoader.ConfigInstance SFTeamsUrl = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "TeamsUrl",
			DefaultString = "http://www.heavymetalmachines.com/team/"
		};

		public static readonly ConfigLoader.ConfigInstance SfNetMaximumConnections = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "MaximumConnections",
			DefaultInt = 20
		};

		public static readonly ConfigLoader.ConfigInstance SfNetClientRecycledMessagesPoolSize = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "ClientRecycledMessagesPoolSize",
			DefaultInt = 500
		};

		public static readonly ConfigLoader.ConfigInstance SfNetServerRecycledMessagesPoolSize = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "ServerRecycledMessagesPoolSize",
			DefaultInt = 1000
		};

		public static readonly ConfigLoader.ConfigInstance SfNetHeartbeatInterval = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "HeartbeatInterval",
			DefaultFloat = 0.25f
		};

		public static readonly ConfigLoader.ConfigInstance SfNetConnectionTimeout = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "ConnectionTimeout",
			DefaultInt = 10
		};

		public static readonly ConfigLoader.ConfigInstance SfNetMaximumTransmissionUnit = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "MaximumTransmissionUnit",
			DefaultInt = 508
		};

		public static readonly ConfigLoader.ConfigInstance SfNetAutoExpandMtu = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "AutoExpandMTU",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance SfNetServerAverageStatisticsInterval = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "ServerAverageStatisticsInterval",
			DefaultInt = 60
		};

		public static readonly ConfigLoader.ConfigInstance SfNetClientAverageStatisticsInterval = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "ClientAverageStatisticsInterval",
			DefaultInt = 60
		};

		public static readonly ConfigLoader.ConfigInstance SfNetReliableSendCount = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "ReliableSendCount",
			DefaultInt = 3
		};

		public static readonly ConfigLoader.ConfigInstance SfNetStatisticsLogFilePath = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "StatisticsLogFilePath",
			DefaultString = ".\\logs"
		};

		public static readonly ConfigLoader.ConfigInstance SfNetSimulatedMinimumLatency = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "SimulatedMinimumLatency",
			DefaultFloat = 0.0375f
		};

		public static readonly ConfigLoader.ConfigInstance SfNetSimulatedRandomLatency = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "SimulatedRandomLatency",
			DefaultFloat = 0.005f
		};

		public static readonly ConfigLoader.ConfigInstance SfNetSimulatedLoss = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "SimulatedLoss",
			DefaultFloat = 0f
		};

		public static readonly ConfigLoader.ConfigInstance SfNetSimulatedDuplicates = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "SimulatedDuplicates",
			DefaultFloat = 0f
		};

		public static readonly ConfigLoader.ConfigInstance SfNetSimulatedDuplicatesCount = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "SimulatedDuplicatesCount",
			DefaultInt = 0
		};

		public static readonly ConfigLoader.ConfigInstance SfNetSimulatedOutOfOrder = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "SimulatedOutOfOrder",
			DefaultFloat = 0f
		};

		public static readonly ConfigLoader.ConfigInstance SfNetSimulatedCorruption = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "SimulatedCorruption",
			DefaultFloat = 0f
		};

		public static readonly ConfigLoader.ConfigInstance SfNetClockSyncTimeout = new ConfigLoader.ConfigInstance
		{
			Context = "SfNetwork",
			Key = "ClockSyncTimeout",
			DefaultInt = 15
		};

		public static readonly ConfigLoader.ConfigInstance MiniMapUpdateDelay = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "MinimapUpdateDelay",
			DefaultFloat = 0.032f
		};

		public static readonly ConfigLoader.ConfigInstance AFKDisconnectionLimit = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "AFKDisconnectionLimit",
			DefaultFloat = 300f
		};

		public static readonly ConfigLoader.ConfigInstance AFKModifierLimit = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "AFKModifierLimit",
			DefaultFloat = 300f
		};

		public static readonly ConfigLoader.ConfigInstance AFKInputLimit = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "AFKInputLimit",
			DefaultFloat = 15f
		};

		public static readonly ConfigLoader.ConfigInstance AFKLimit = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "AFKLimit",
			DefaultFloat = 120f
		};

		public static readonly ConfigLoader.ConfigInstance LoadingTimeoutSeconds = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "LoadingTimeoutSeconds",
			DefaultInt = 60
		};

		public static readonly ConfigLoader.ConfigInstance DataServiceAddress = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "DataServiceAddress",
			DefaultString = string.Empty
		};

		public static readonly ConfigLoader.ConfigInstance DataServicePort = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "DataServicePort",
			DefaultInt = 0
		};

		public static readonly ConfigLoader.ConfigInstance ForceRookie = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "ForceRookie",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance IgnoreMemoryCheck = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "IgnoreMemoryCheck",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance IgnoreVideoCardCheck = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "IgnoreVideoCardCheck",
			DefaultBool = true
		};

		public static readonly ConfigLoader.ConfigInstance ShowWelcomePage = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "ShowWelcomePage",
			DefaultBool = true
		};

		public static readonly ConfigLoader.ConfigInstance WelcomePageURL = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "WelcomePageURL",
			DefaultString = string.Empty
		};

		public static readonly ConfigLoader.ConfigInstance CrashPageURL = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "CrashPageURL",
			DefaultString = "http://www.heavymetalmachines.com/crash/?lang={0}&steamid={1}"
		};

		public static readonly ConfigLoader.ConfigInstance EnableNarrator = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "EnableNarrator",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance NarratorHostFilter = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "NarratorHostFilter",
			DefaultString = "37.58.69.242"
		};

		public static readonly ConfigLoader.ConfigInstance ForcePvP = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "ForcePvP",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance SkipSFMissionConfigHack = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "SkipSFMissionConfigHack",
			DefaultString = string.Empty
		};

		public static readonly ConfigLoader.ConfigInstance[] RedForcedBot = new ConfigLoader.ConfigInstance[]
		{
			new ConfigLoader.ConfigInstance
			{
				Context = "Debug",
				Key = "RedForcedBot0",
				DefaultInt = -1
			},
			new ConfigLoader.ConfigInstance
			{
				Context = "Debug",
				Key = "RedForcedBot1",
				DefaultInt = -1
			},
			new ConfigLoader.ConfigInstance
			{
				Context = "Debug",
				Key = "RedForcedBot2",
				DefaultInt = -1
			},
			new ConfigLoader.ConfigInstance
			{
				Context = "Debug",
				Key = "RedForcedBot3",
				DefaultInt = -1
			}
		};

		public static readonly ConfigLoader.ConfigInstance[] BlueForcedBot = new ConfigLoader.ConfigInstance[]
		{
			new ConfigLoader.ConfigInstance
			{
				Context = "Debug",
				Key = "BlueForcedBot0",
				DefaultInt = -1
			},
			new ConfigLoader.ConfigInstance
			{
				Context = "Debug",
				Key = "BlueForcedBot1",
				DefaultInt = -1
			},
			new ConfigLoader.ConfigInstance
			{
				Context = "Debug",
				Key = "BlueForcedBot2",
				DefaultInt = -1
			},
			new ConfigLoader.ConfigInstance
			{
				Context = "Debug",
				Key = "BlueForcedBot3",
				DefaultInt = -1
			}
		};

		public static readonly ConfigLoader.ConfigInstance BlueMMR = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "BlueMMR",
			DefaultInt = 500
		};

		public static readonly ConfigLoader.ConfigInstance RedMMR = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "RedMMR",
			DefaultInt = 500
		};

		public static readonly ConfigLoader.ConfigInstance FlipVfxTeamColors = new ConfigLoader.ConfigInstance
		{
			Context = "Debug",
			Key = "FlipVfxTeamColors",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance TeamsMainMenuTimeoutInSeconds = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "TeamsMainMenuTimeoutInSeconds",
			DefaultInt = 30
		};

		public static readonly ConfigLoader.ConfigInstance SupportPageURL = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "SupportPageURL",
			DefaultString = "http://www.heavymetalmachines.com/support/"
		};

		public static readonly ConfigLoader.ConfigInstance FacebookPageURL = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "FacebookPageURL",
			DefaultString = "https://www.facebook.com/HeavyMetalMachinesBR"
		};

		public static readonly ConfigLoader.ConfigInstance VkPageURL = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "VkPageURL",
			DefaultString = "https://vk.com/heavymetalmachines"
		};

		public static readonly ConfigLoader.ConfigInstance DiscordPageURL = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "DiscordPageURL",
			DefaultString = "https://discord.gg/heavymetalmachines"
		};

		public static readonly ConfigLoader.ConfigInstance InstagramBRPageURL = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "InstagramBRPageURL",
			DefaultString = "https://www.instagram.com/heavymetalmachinesbr/"
		};

		public static readonly ConfigLoader.ConfigInstance InstagramPageURL = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "InstagramPageURL",
			DefaultString = "https://www.instagram.com/officialhmm/"
		};

		public static readonly ConfigLoader.ConfigInstance CombatTestKnowMoreURL = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "CombatTestKnowMorePageURL",
			DefaultString = "http://heavymetalmachines.com/moreinfo/?lang={0}"
		};

		public static readonly ConfigLoader.ConfigInstance CharacterGuideURL = new ConfigLoader.ConfigInstance
		{
			Context = "Game",
			Key = "CharacterGuideURL",
			DefaultString = "http://heavymetalmachines.com/site/{0}?lang={1}"
		};

		public static readonly ConfigLoader.ConfigInstance EnableRedShell = new ConfigLoader.ConfigInstance
		{
			Context = "Tools",
			Key = "EnableRedShell",
			DefaultBool = true
		};

		public static readonly ConfigLoader.ConfigInstance EnableHoplonTT = new ConfigLoader.ConfigInstance
		{
			Context = "Tools",
			Key = "EnableHoplonTT",
			DefaultBool = true
		};

		public static readonly ConfigLoader.ConfigInstance HoplonTTUrl = new ConfigLoader.ConfigInstance
		{
			Context = "Tools",
			Key = "HoplonTTUrl",
			DefaultString = "http://t.hoplon.com/conversion"
		};

		public static readonly ConfigLoader.ConfigInstance EnableHoplonTTEvent = new ConfigLoader.ConfigInstance
		{
			Context = "Tools",
			Key = "EnableHoplonTTEvent",
			DefaultBool = true
		};

		public static readonly ConfigLoader.ConfigInstance HoplonTTEventUrl = new ConfigLoader.ConfigInstance
		{
			Context = "Tools",
			Key = "HoplonTTEventUrl",
			DefaultString = "http://t.hoplon.com/event"
		};

		public static readonly ConfigLoader.ConfigInstance HORTA = new ConfigLoader.ConfigInstance
		{
			Context = "Tools",
			Key = "HORTA",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance HORTADestFolder = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "MatchFileFolder",
			DefaultString = ".\\logs\\"
		};

		public static readonly ConfigLoader.ConfigInstance DisableRecorder = new ConfigLoader.ConfigInstance
		{
			Context = "Server",
			Key = "DisableRecorder",
			DefaultBool = false
		};

		public static readonly ConfigLoader.ConfigInstance SFQuizUrl = new ConfigLoader.ConfigInstance
		{
			Context = "Swordfish",
			Key = "QuizUrl",
			DefaultString = "http://www.heavymetalmachines.com/quiz/"
		};
	}
}
