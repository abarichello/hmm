using System;
using HeavyMetalMachines.Combat.Gadget;
using UnityEngine;

namespace HeavyMetalMachines
{
	public interface IPlayerController
	{
		event Action ServerListenToReverseUse;

		event CancelActionListener ListenToCancelAction;

		void ActionExecuted(GadgetBehaviour gadget);

		bool MovingCar { get; }

		bool AcceleratingForward { get; }

		Vector3 MousePosition { get; }
	}
}
