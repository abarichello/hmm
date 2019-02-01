using System;
using Pocketverse;

namespace HeavyMetalMachines.BI
{
	public interface IPlayerTechnicalExperienceManagerDispatch : IDispatch
	{
		void ReceivePlayerExperienceData(ExperienceDataSet data);
	}
}
