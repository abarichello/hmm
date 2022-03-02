using System;
using System.Collections;
using System.Collections.Generic;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Tutorial.InGame;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class UnlockInputsOnCompleteBehaviour : InGameTutorialBehaviourBase
	{
		protected override void OnStepCompletedOnServer()
		{
			base.OnStepCompletedOnServer();
			UnlockInputsOnCompleteBehaviour.UnlockInputs();
		}

		protected override void OnStepCompletedOnClient()
		{
			base.OnStepCompletedOnClient();
			UnlockInputsOnCompleteBehaviour.UnlockInputs();
		}

		private static void UnlockInputs()
		{
			List<ControllerInputActions> list = new List<ControllerInputActions>();
			IEnumerator enumerator = Enum.GetValues(typeof(ControllerInputActions)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					ControllerInputActions item = (ControllerInputActions)obj;
					list.Add(item);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			ControllerInputActions[] inputActions = list.ToArray();
			ControlOptions.UnlockControlAction(inputActions);
		}
	}
}
