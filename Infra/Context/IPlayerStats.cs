using System;
using System.Collections.Generic;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.Items.DataTransferObjects;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IPlayerStats
	{
		int Kills { get; }

		int Deaths { get; }

		int Assists { get; }

		List<MissionCompleted> MissionsCompletedIndex { get; }

		bool MatchWon { get; }

		float DamageDealtToPlayers { get; }

		float HealingProvided { get; }

		int NumberOfMedals { get; }

		int BombsDelivered { get; }

		float GetDamagePerMinuteDealt(IMatchStats matchStats);

		float GetHealingPerMinuteProvided(IMatchStats matchStats);

		TeamKind Team { get; }

		Guid CharacterItemTypeGuid { get; }

		CustomizationContent Customizations { get; }

		DriverRoleKind CharacterRole { get; }

		void RegisterGadgetActivation(GadgetSlot slot);

		int GetGadgetUses(GadgetSlot slot);

		int GetCategoryUses(Guid category);

		int GetItemTypeUses(Guid itemTypeId);

		void IncreaseDebuffTime(float debuffTime);

		void IncreaseBombGadgetPowerShotScoreCount(int shotCount);
	}
}
