using System;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera
{
	public class GameCameraPlayerSetup : IGameCameraPlayerSetup
	{
		public int PlayerCarId
		{
			get
			{
				return this._playerCarId;
			}
		}

		public Transform PlayerTransform { get; private set; }

		public bool IsAlive()
		{
			return this._playerCarId != -1 && this._playerCombatData.IsAlive();
		}

		public bool IsPreSpawning()
		{
			return this._playerCarId != -1 && (this._playerSpawnController.State == SpawnStateKind.PreSpawned || this._playerSpawnController.State == SpawnStateKind.Respawning);
		}

		public void SetupCurrentPlayer(Transform playerTransform, CombatData playerCombat)
		{
			this.PlayerTransform = playerTransform;
			this._playerCombatData = playerCombat;
			this._playerSpawnController = playerCombat.Combat.SpawnController;
			this._playerCarId = playerCombat.Id.ObjId;
		}

		public void Cleanup()
		{
			this.PlayerTransform = null;
			this._playerCombatData = null;
			this._playerSpawnController = null;
			this._playerCarId = -1;
		}

		private int _playerCarId = -1;

		private CombatData _playerCombatData;

		private SpawnController _playerSpawnController;
	}
}
