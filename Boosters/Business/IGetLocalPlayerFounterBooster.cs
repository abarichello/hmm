using System;
using HeavyMetalMachines.DataTransferObjects.Player;

namespace HeavyMetalMachines.Boosters.Business
{
	public interface IGetLocalPlayerFounterBooster
	{
		bool HasFounderBooster();

		FounderLevel GetFounderLevel();
	}
}
