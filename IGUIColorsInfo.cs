using System;
using UnityEngine;

namespace HeavyMetalMachines
{
	public interface IGUIColorsInfo
	{
		Color MyColor { get; }

		Color BlueTeamColor { get; }

		Color BlueTeamColorOld { get; }

		Color RedTeamColor { get; }

		Color Victory { get; }

		Color Defeat { get; }

		Color DifficultyLevel0 { get; }

		Color DifficultyLevel1 { get; }

		Color DifficultyLevel2 { get; }

		Color DifficultyLevel3 { get; }

		Color DifficultyLevel4 { get; }

		Color DifficultyLevel5 { get; }

		Color HealColor { get; }

		Color DamageColor { get; }

		Color OwnDamageColor { get; }

		Color RespawnCrosshairColor { get; }

		Color HighPingColor { get; }

		Color HighPingColorSecondary { get; }

		Color MediumPingColor { get; }

		Color MediumPingColorSecondary { get; }

		Color LowPingColor { get; }

		Color LowPingColorSecondary { get; }

		Color NoPingColor { get; }

		Color PingHighGradientTopColor { get; }

		Color PingHighGradientBottonColor { get; }

		Color PingMediumGradientTopColor { get; }

		Color PingMediumGradientBottonColor { get; }

		Color PingLowGradientTopColor { get; }

		Color PingLowGradientBottonColor { get; }

		Color MyChatColor { get; }

		Color MyTeamChatColor { get; }

		Color EnemyTeamChatColor { get; }

		Color StorytellerChatColor { get; }

		Color TeamTagColor { get; }

		Color InstanceUtilityColor { get; }

		Color InstanceOffensiveColor { get; }

		Color InstanceSupportColor { get; }

		Color InstanceSurvivalColor { get; }

		PlayerColorsInfo PlayerColorsInfo { get; }
	}
}
