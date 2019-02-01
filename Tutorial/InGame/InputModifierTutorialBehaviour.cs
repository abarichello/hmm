using System;
using System.Collections;
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
			if (this._inputModifierInfo.UnlockedPlayerControls.Contains(ControlAction.MovementForward))
			{
				base.playerController.Combat.Movement.UnlockMovement();
			}
			else
			{
				base.playerController.Combat.Movement.LockMovement();
			}
		}

		public void ApplyInputModifiers()
		{
			IEnumerator enumerator = Enum.GetValues(typeof(ControlAction)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					ControlAction controlAction = (ControlAction)obj;
					if (this._inputModifierInfo.UnlockedPlayerControls.Contains(controlAction))
					{
						ControlOptions.UnlockControlAction(controlAction);
					}
					else
					{
						ControlOptions.LockControlAction(controlAction);
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
