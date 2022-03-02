using System;
using System.Collections;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Options;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class InputModifierTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			this.ApplyInputModifiers();
		}

		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			if (this._inputModifierInfo.UnlockedPlayerInputActions.Contains(4))
			{
				base.playerController.Combat.Movement.UnpauseSimulation();
			}
			else
			{
				base.playerController.Combat.Movement.PauseSimulation();
			}
		}

		public void ApplyInputModifiers()
		{
			IEnumerator enumerator = Enum.GetValues(typeof(ControllerInputActions)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					ControllerInputActions controllerInputActions = (ControllerInputActions)obj;
					if (this._inputModifierInfo.UnlockedPlayerInputActions.Contains(controllerInputActions))
					{
						ControlOptions.UnlockControlAction(controllerInputActions);
					}
					else
					{
						ControlOptions.LockControlAction(controllerInputActions);
					}
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
		}

		[SerializeField]
		private InputModifierInfo _inputModifierInfo;
	}
}
