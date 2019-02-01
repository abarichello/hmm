using System;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IServerMatchStats
	{
		float GetTotalRedTeamDamage();

		float GetTotalBlueTeamDamage();

		float GetTotalRedTeamHeal();

		float GetTotalBlueTeamHeal();

		int GetTotalKillsRed();

		int GetTotalKillsBlue();

		int GetTotalBombCarrierKillsRed();

		int GetTotalBombCarrierKillsBlue();

		int GetTotalTacklerKillsRed();

		int GetTotalTacklerKillsBlue();

		int GetTotalCarrierKillsRed();

		int GetTotalCarrierKillsBlue();

		int GetTotalSupportKillsRed();

		int GetTotalSupportKillsBlue();

		int GetDeliveriesRed();

		int GetDeliveriesBlue();
	}
}
