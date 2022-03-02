using System;
using HeavyMetalMachines.Arena.Infra;
using UnityEngine;

namespace HeavyMetalMachines.Arena
{
	public interface IGameplayGameArenaInfo
	{
		float CursorLockTimeInSeconds { get; }

		Vector2 CursorLockOffset { get; }

		float RespawnTimeSeconds { get; }

		float RespawningTimeSeconds { get; }

		float KillCamWaitTimeSeconds { get; }

		int WarmupTimeSeconds { get; }

		float FirstShopExtraTimeSeconds { get; }

		int ShopPhaseSeconds { get; }

		Vector3 BombSpawnPoint { get; }

		ArenaModifierConfiguration[] ModifiersToApply { get; }

		int ArenaModifierDistanceToBombToApply { get; }

		int ReplayDelaySeconds { get; }

		float RoundTimeSeconds { get; }

		float OvertimeDurationSeconds { get; }

		int TimeBeforeOvertime { get; }

		float OvertimeGuiDeliveryScaleModifier { get; }

		float NearGoalDistance { get; }

		bool IsGPSDisabled { get; }
	}
}
