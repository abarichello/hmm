using System;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera
{
	public interface IGameCameraPlayerSetup
	{
		int PlayerCarId { get; }

		Transform PlayerTransform { get; }

		bool IsAlive();

		bool IsPreSpawning();

		void SetupCurrentPlayer(Transform playerTransform, CombatData playerCombat);

		void Cleanup();
	}
}
