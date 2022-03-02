using System;
using HeavyMetalMachines.DataTransferObjects.Server;

namespace HeavyMetalMachines.Storyteller
{
	public interface IStorytellerTranslationProvider
	{
		string GetTranslatedTitle();

		string GetTranslatedRegion(string region);

		string GetTranslatedServerPhase(ServerStatusBag.ServerPhaseKind phase);
	}
}
