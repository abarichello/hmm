using System;

namespace HeavyMetalMachines.Scene
{
	public enum ActionType
	{
		EnableOnSpawn,
		DisableOnSpawn,
		EnableOnUnspawn,
		DisableOnUnspawn,
		EnableOnSpawnDisableOnUnspawn,
		EnableOnUnspawnDisableOnSpawn,
		EnableOnEnter,
		EnableOnExit,
		DisableOnEnter,
		DisableOnExit,
		GamePhaseStart
	}
}
