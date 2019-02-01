using System;
using HeavyMetalMachines.Tutorial.InGame;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class RemovePassiveModifierTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this.PassiveModifierRef.RemovePassiveModifier();
			this.CompleteBehaviourAndSync();
		}

		public AddPassiveModifierTutorialBehaviour PassiveModifierRef;
	}
}
