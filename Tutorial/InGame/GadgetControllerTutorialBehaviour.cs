using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Utils;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class GadgetControllerTutorialBehaviour : InGameTutorialBehaviourBase
	{
		private void Awake()
		{
			Debug.Assert(ControlOptions.IsControlActionUnlocked(9), "Control Action GadgetDropBomb must be unlocked before this step!!!", Debug.TargetTeam.All);
		}

		protected override void StartBehaviourOnServer()
		{
			base.SetPlayerInputsActive(true);
		}

		public GadgetSlot Gadget;
	}
}
