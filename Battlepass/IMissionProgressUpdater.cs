using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;

namespace HeavyMetalMachines.Battlepass
{
	public interface IMissionProgressUpdater
	{
		void Update(MissionProgressValue progressValue, Objectives missionObjectives);
	}
}
