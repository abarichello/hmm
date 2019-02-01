using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class GUIColorsInfo : GameHubScriptableObject
	{
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

		public static Color GetChatColor(long playerId, TeamKind teamKind)
		{
			if (playerId == GameHubScriptableObject.Hub.Players.CurrentPlayerData.PlayerId)
			{
				return GUIColorsInfo.Instance.MyChatColor;
			}
			if (teamKind == GameHubScriptableObject.Hub.Players.CurrentPlayerData.Team)
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
		public Color MyColor;

		public Color BlueTeamColor;

		public Color BlueTeamColorOld;

		public Color RedTeamColor;

		[Header("State Color")]
		public Color Victory;

		public Color Defeat;

		[Header("Character Info Colors")]
		public Color DifficultyLevel0;

		public Color DifficultyLevel1;

		public Color DifficultyLevel2;

		public Color DifficultyLevel3;

		public Color DifficultyLevel4;

		public Color DifficultyLevel5;

		[Header("Event Colors")]
		public Color HealColor;

		public Color DamageColor;

		public Color OwnDamageColor;

		public Color RespawnCrosshairColor;

		[Header("[Ping Colors]")]
		public Color HighPingColor;

		public Color HighPingColorSecondary;

		public Color MediumPingColor;

		public Color MediumPingColorSecondary;

		public Color LowPingColor;

		public Color LowPingColorSecondary;

		public Color NoPingColor;

		public Color PingHighGradientTopColor;

		public Color PingHighGradientBottonColor;

		public Color PingMediumGradientTopColor;

		public Color PingMediumGradientBottonColor;

		public Color PingLowGradientTopColor;

		public Color PingLowGradientBottonColor;

		[Header("[Chat Name Colors]")]
		public Color MyChatColor;

		public Color MyTeamChatColor;

		public Color EnemyTeamChatColor;

		[Header("[Team Colors]")]
		public Color TeamTagColor;

		[Header("[Instance Category Colors]")]
		public Color InstanceUtilityColor;

		public Color InstanceOffensiveColor;

		public Color InstanceSupportColor;

		public Color InstanceSurvivalColor;

		public PlayerColorsInfo PlayerColorsInfo;
	}
}
