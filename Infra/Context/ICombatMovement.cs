using System;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Context
{
	public interface ICombatMovement
	{
		Vector3 GetClosestValidPosition(Vector3 position, bool useRespawnMesh = true);
	}
}
