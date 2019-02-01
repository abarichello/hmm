using System;
using System.Collections;
using System.Collections.Generic;
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
			List<ControlAction> list = new List<ControlAction>();
			IEnumerator enumerator = Enum.GetValues(typeof(ControlAction)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					ControlAction item = (ControlAction)obj;
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
			ControlAction[] controlActions = list.ToArray();
			ControlOptions.UnlockControlAction(controlActions);
		}
	}
}
