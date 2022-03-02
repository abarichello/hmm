using System;

namespace HeavyMetalMachines.Arena.Infra
{
	public enum ArenaModifierCondition
	{
		None,
		PlayerAllTime,
		EnemiesAllTime,
		AlliesAllTime,
		PlayerCarryingBomb,
		EnemyCarryingBomb,
		AllyCarryingBomb,
		PlayerWithBombApplyToAllAllies,
		EnemyWithBombApplyToAllAllies,
		AllyWithBombApplyToAllAllies,
		PlayerWithBombApplyToAllEnemies,
		EnemyWithBombApplyToAllEnemies,
		AllyWithBombApplyToAllEnemies,
		EnemyWithBombApplyToPlayer,
		AllyWithBombApplyToPlayer,
		PlayerWithoutBombApplyToAllAllies = 16,
		EnemyWithoutBombApplyToAllAllies,
		AllyWithoutBombApplyToAllAllies,
		PlayerWithoutBombApplyToAllEnemies,
		EnemyWithoutBombApplyToAllEnemies,
		AllyWithoutBombApplyToAllEnemies,
		EnemyWithoutBombApplyToPlayer,
		AllyWithoutBombApplyToPlayer,
		PlayerWithoutBombAndDistanceToBombApplyToAllies = 25,
		PlayerWithoutBombAndDistanceToBombApplyToEnemies
	}
}
