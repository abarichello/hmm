using System;
using HeavyMetalMachines.Matches.DataTransferObjects;

namespace HeavyMetalMachines.Pick
{
	public interface IPickModeConfigProvider
	{
		MatchPickModeConfig Get(MatchKind kind);
	}
}
