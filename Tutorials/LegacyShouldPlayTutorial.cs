using System;
using HeavyMetalMachines.Tutorial;

namespace HeavyMetalMachines.Tutorials
{
	public class LegacyShouldPlayTutorial : IShouldPlayTutorial
	{
		public LegacyShouldPlayTutorial(TutorialController tutorialController)
		{
			this._tutorialController = tutorialController;
		}

		public bool Check()
		{
			return this._tutorialController.PlayerMustGoToTutorial();
		}

		private readonly TutorialController _tutorialController;
	}
}
