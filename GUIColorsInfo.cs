using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines
{
	public class GUIColorsInfo : GameHubScriptableObject, IGUIColorsInfo
	{
		public Color MyColor
		{
			get
			{
				return this._myColor;
			}
		}

		public Color BlueTeamColor
		{
			get
			{
				return this._blueTeamColor;
			}
		}

		public Color BlueTeamColorOld
		{
			get
			{
				return this._blueTeamColorOld;
			}
		}

		public Color RedTeamColor
		{
			get
			{
				return this._redTeamColor;
			}
		}

		public Color Victory
		{
			get
			{
				return this._victory;
			}
		}

		public Color Defeat
		{
			get
			{
				return this._defeat;
			}
		}

		public Color DifficultyLevel0
		{
			get
			{
				return this._difficultyLevel0;
			}
		}

		public Color DifficultyLevel1
		{
			get
			{
				return this._difficultyLevel1;
			}
		}

		public Color DifficultyLevel2
		{
			get
			{
				return this._difficultyLevel2;
			}
		}

		public Color DifficultyLevel3
		{
			get
			{
				return this._difficultyLevel3;
			}
		}

		public Color DifficultyLevel4
		{
			get
			{
				return this._difficultyLevel4;
			}
		}

		public Color DifficultyLevel5
		{
			get
			{
				return this._difficultyLevel5;
			}
		}

		public Color HealColor
		{
			get
			{
				return this._healColor;
			}
		}

		public Color DamageColor
		{
			get
			{
				return this._damageColor;
			}
		}

		public Color OwnDamageColor
		{
			get
			{
				return this._ownDamageColor;
			}
		}

		public Color RespawnCrosshairColor
		{
			get
			{
				return this._respawnCrosshairColor;
			}
		}

		public Color HighPingColor
		{
			get
			{
				return this._highPingColor;
			}
		}

		public Color HighPingColorSecondary
		{
			get
			{
				return this._highPingColorSecondary;
			}
		}

		public Color MediumPingColor
		{
			get
			{
				return this._mediumPingColor;
			}
		}

		public Color MediumPingColorSecondary
		{
			get
			{
				return this._mediumPingColorSecondary;
			}
		}

		public Color LowPingColor
		{
			get
			{
				return this._lowPingColor;
			}
		}

		public Color LowPingColorSecondary
		{
			get
			{
				return this._lowPingColorSecondary;
			}
		}

		public Color NoPingColor
		{
			get
			{
				return this._noPingColor;
			}
		}

		public Color PingHighGradientTopColor
		{
			get
			{
				return this._pingHighGradientTopColor;
			}
		}

		public Color PingHighGradientBottonColor
		{
			get
			{
				return this._pingHighGradientBottonColor;
			}
		}

		public Color PingMediumGradientTopColor
		{
			get
			{
				return this._pingMediumGradientTopColor;
			}
		}

		public Color PingMediumGradientBottonColor
		{
			get
			{
				return this._pingMediumGradientBottonColor;
			}
		}

		public Color PingLowGradientTopColor
		{
			get
			{
				return this._pingLowGradientTopColor;
			}
		}

		public Color PingLowGradientBottonColor
		{
			get
			{
				return this._pingLowGradientBottonColor;
			}
		}

		public Color MyChatColor
		{
			get
			{
				return this._myChatColor;
			}
		}

		public Color MyTeamChatColor
		{
			get
			{
				return this._myTeamChatColor;
			}
		}

		public Color EnemyTeamChatColor
		{
			get
			{
				return this._enemyTeamChatColor;
			}
		}

		public Color StorytellerChatColor
		{
			get
			{
				return this._storytellerChatColor;
			}
		}

		public Color TeamTagColor
		{
			get
			{
				return this._teamTagColor;
			}
		}

		public Color InstanceUtilityColor
		{
			get
			{
				return this._instanceUtilityColor;
			}
		}

		public Color InstanceOffensiveColor
		{
			get
			{
				return this._instanceOffensiveColor;
			}
		}

		public Color InstanceSupportColor
		{
			get
			{
				return this._instanceSupportColor;
			}
		}

		public Color InstanceSurvivalColor
		{
			get
			{
				return this._instanceSurvivalColor;
			}
		}

		public PlayerColorsInfo PlayerColorsInfo
		{
			get
			{
				return this._playerColorsInfo;
			}
		}

		public static GUIColorsInfo Instance
		{
			get
			{
				return GameHubScriptableObject.Hub.GuiScripts.GUIColors;
			}
		}

		public static Color GetPlayerColor(long playerId, TeamKind teamKind)
		{
			if (playerId == GameHubScriptableObject.Hub.Players.CurrentPlayerData.PlayerId)
			{
				return GUIColorsInfo.Instance.MyColor;
			}
			if (teamKind == GameHubScriptableObject.Hub.Players.CurrentPlayerData.Team)
			{
				return GUIColorsInfo.Instance.BlueTeamColor;
			}
			return GUIColorsInfo.Instance.RedTeamColor;
		}

		public static Color GetOldPlayerColor(long playerId, TeamKind teamKind)
		{
			if (playerId == GameHubScriptableObject.Hub.Players.CurrentPlayerData.PlayerId)
			{
				return GUIColorsInfo.Instance.MyColor;
			}
			if (teamKind == GameHubScriptableObject.Hub.Players.CurrentPlayerData.Team)
			{
				return GUIColorsInfo.Instance.BlueTeamColorOld;
			}
			return GUIColorsInfo.Instance.RedTeamColor;
		}

		public static Color GetChatColor(long playerId, TeamKind teamKind, bool isStoryteller)
		{
			PlayerData currentPlayerData = GameHubScriptableObject.Hub.Players.CurrentPlayerData;
			if (playerId == currentPlayerData.PlayerId)
			{
				return GUIColorsInfo.Instance.MyChatColor;
			}
			if (isStoryteller)
			{
				return GUIColorsInfo.Instance.StorytellerChatColor;
			}
			if (currentPlayerData.IsNarrator)
			{
				if (teamKind == TeamKind.Blue)
				{
					return GUIColorsInfo.Instance.MyTeamChatColor;
				}
				if (teamKind == TeamKind.Red)
				{
					return GUIColorsInfo.Instance.EnemyTeamChatColor;
				}
			}
			else if (teamKind == currentPlayerData.Team)
			{
				return GUIColorsInfo.Instance.MyTeamChatColor;
			}
			return GUIColorsInfo.Instance.EnemyTeamChatColor;
		}

		public static Color GetColorByPlayerCarId(int id, bool tryToUseTeamColors = false)
		{
			if (id < 0 || id > 9)
			{
				return Color.black;
			}
			if (GameHubScriptableObject.Hub.Options == null)
			{
				return Color.white;
			}
			if (!tryToUseTeamColors || !GameHubScriptableObject.Hub.Options.Game.UseTeamColor)
			{
				return GUIColorsInfo.Instance.PlayerColorsInfo.PlayerColors[id];
			}
			int instanceId = GameHubScriptableObject.Hub.Players.CurrentPlayerData.PlayerCarId.GetInstanceId();
			if (id == instanceId)
			{
				return GUIColorsInfo.Instance.MyColor;
			}
			int num = instanceId % 2;
			int num2 = id % 2;
			return (num != num2) ? GUIColorsInfo.Instance.RedTeamColor : GUIColorsInfo.Instance.BlueTeamColor;
		}

		[Header("Team Color")]
		[SerializeField]
		[FormerlySerializedAs("MyColor")]
		private Color _myColor;

		[SerializeField]
		[FormerlySerializedAs("BlueTeamColor")]
		private Color _blueTeamColor;

		[SerializeField]
		[FormerlySerializedAs("BlueTeamColorOld")]
		private Color _blueTeamColorOld;

		[SerializeField]
		[FormerlySerializedAs("RedTeamColor")]
		private Color _redTeamColor;

		[Header("State Color")]
		[SerializeField]
		[FormerlySerializedAs("Victory")]
		private Color _victory;

		[SerializeField]
		[FormerlySerializedAs("Defeat")]
		private Color _defeat;

		[Header("Character Info Colors")]
		[SerializeField]
		[FormerlySerializedAs("DifficultyLevel0")]
		private Color _difficultyLevel0;

		[SerializeField]
		[FormerlySerializedAs("DifficultyLevel1")]
		private Color _difficultyLevel1;

		[SerializeField]
		[FormerlySerializedAs("DifficultyLevel2")]
		private Color _difficultyLevel2;

		[SerializeField]
		[FormerlySerializedAs("DifficultyLevel3")]
		private Color _difficultyLevel3;

		[SerializeField]
		[FormerlySerializedAs("DifficultyLevel4")]
		private Color _difficultyLevel4;

		[SerializeField]
		[FormerlySerializedAs("DifficultyLevel5")]
		private Color _difficultyLevel5;

		[Header("Event Colors")]
		[SerializeField]
		[FormerlySerializedAs("HealColor")]
		private Color _healColor;

		[SerializeField]
		[FormerlySerializedAs("DamageColor")]
		private Color _damageColor;

		[SerializeField]
		[FormerlySerializedAs("OwnDamageColor")]
		private Color _ownDamageColor;

		[SerializeField]
		[FormerlySerializedAs("RespawnCrosshairColor")]
		private Color _respawnCrosshairColor;

		[Header("[Ping Colors]")]
		[SerializeField]
		[FormerlySerializedAs("HighPingColor")]
		private Color _highPingColor;

		[SerializeField]
		[FormerlySerializedAs("HighPingColorSecondary")]
		private Color _highPingColorSecondary;

		[SerializeField]
		[FormerlySerializedAs("MediumPingColor")]
		private Color _mediumPingColor;

		[SerializeField]
		[FormerlySerializedAs("MediumPingColorSecondary")]
		private Color _mediumPingColorSecondary;

		[SerializeField]
		[FormerlySerializedAs("LowPingColor")]
		private Color _lowPingColor;

		[SerializeField]
		[FormerlySerializedAs("LowPingColorSecondary")]
		private Color _lowPingColorSecondary;

		[SerializeField]
		[FormerlySerializedAs("NoPingColor")]
		private Color _noPingColor;

		[SerializeField]
		[FormerlySerializedAs("PingHighGradientTopColor")]
		private Color _pingHighGradientTopColor;

		[SerializeField]
		[FormerlySerializedAs("PingHighGradientBottonColor")]
		private Color _pingHighGradientBottonColor;

		[SerializeField]
		[FormerlySerializedAs("PingMediumGradientTopColor")]
		private Color _pingMediumGradientTopColor;

		[SerializeField]
		[FormerlySerializedAs("PingMediumGradientBottonColor")]
		private Color _pingMediumGradientBottonColor;

		[SerializeField]
		[FormerlySerializedAs("PingLowGradientTopColor")]
		private Color _pingLowGradientTopColor;

		[SerializeField]
		[FormerlySerializedAs("PingLowGradientBottonColor")]
		private Color _pingLowGradientBottonColor;

		[Header("[Chat Name Colors]")]
		[SerializeField]
		[FormerlySerializedAs("MyChatColor")]
		private Color _myChatColor;

		[SerializeField]
		[FormerlySerializedAs("MyTeamChatColor")]
		private Color _myTeamChatColor;

		[SerializeField]
		[FormerlySerializedAs("EnemyTeamChatColor")]
		private Color _enemyTeamChatColor;

		[SerializeField]
		[FormerlySerializedAs("StorytellerChatColor")]
		private Color _storytellerChatColor;

		[Header("[Team Colors]")]
		[SerializeField]
		[FormerlySerializedAs("TeamTagColor")]
		private Color _teamTagColor;

		[Header("[Instance Category Colors]")]
		[SerializeField]
		[FormerlySerializedAs("InstanceUtilityColor")]
		private Color _instanceUtilityColor;

		[SerializeField]
		[FormerlySerializedAs("InstanceOffensiveColor")]
		private Color _instanceOffensiveColor;

		[SerializeField]
		[FormerlySerializedAs("InstanceSupportColor")]
		private Color _instanceSupportColor;

		[SerializeField]
		[FormerlySerializedAs("InstanceSurvivalColor")]
		private Color _instanceSurvivalColor;

		[SerializeField]
		[FormerlySerializedAs("PlayerColorsInfo")]
		private PlayerColorsInfo _playerColorsInfo;
	}
}
