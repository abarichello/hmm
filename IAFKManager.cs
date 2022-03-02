using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines
{
	public interface IAFKManager
	{
		void ResetValues();

		void AddEntry(byte connectionId);

		float GetAFKTime(byte playerAddress);

		bool CheckLeaver(PlayerData playerData);

		bool CheckLeaver(byte pPlayerAdress, long playerId, string publisherUserId);

		void LeaverWarningCallback(bool timedOut, byte playerAddress);

		void InputChanged(PlayerData player);

		void AddModifier(CombatObject causer);

		List<AFKController.AFKEntry> Entries { get; }
	}
}
