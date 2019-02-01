using System;
using HeavyMetalMachines.Tutorial.InGame;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.TutorialBehaviours
{
	public class ForceStepTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			TutorialStepsController componentInParent = base.GetComponentInParent<TutorialStepsController>();
			componentInParent.ForceStep(this._stepIndex);
		}

		[SerializeField]
		private int _stepIndex;
	}
}
