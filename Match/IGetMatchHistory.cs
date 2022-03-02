using System;

namespace HeavyMetalMachines.Match
{
	public interface IGetMatchHistory
	{
		bool HasStartedTraining();

		bool HasDoneTraining();

		bool HasStartedTutorial();

		bool HasDoneTutorial();
	}
}
