using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	public class FollowPositionGadgetMovement : MonoBehaviour, IGadgetBodyMovement
	{
		public Vector3 GetPosition(float elapsedTime)
		{
			return this.Position.GetValue(this.context);
		}

		public Vector3 GetDirection()
		{
			return Vector3.forward;
		}

		public void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			this.context = gadgetContext;
		}

		public bool Finished { get; private set; }

		public void Destroy()
		{
			this.context = null;
		}

		public Vector3Parameter Position;

		private IGadgetContext context;
	}
}
