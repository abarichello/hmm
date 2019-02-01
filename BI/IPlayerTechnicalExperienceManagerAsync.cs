using System;
using Pocketverse;

namespace HeavyMetalMachines.BI
{
	public interface IPlayerTechnicalExperienceManagerAsync : IAsync
	{
		IFuture ReceivePlayerExperienceData(ExperienceDataSet data);
	}
}
