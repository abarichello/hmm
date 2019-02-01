using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	public interface IGadgetBodyMovement
	{
		Vector3 GetPosition(float elapsedTime);

		Vector3 GetDirection();

		void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext);

		bool Finished { get; }

		void Destroy();
	}
}
