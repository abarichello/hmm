using System;
using Pocketverse;

namespace HeavyMetalMachines.UpdateStream
{
	public abstract class MovementStream : GameHubBehaviour
	{
		public abstract void Read(BitStream stream, double offset);

		public abstract void Write(BitStream stream);

		protected virtual void Awake()
		{
			base.Id.EvtIdentifiableRegisterStatusChange += this.OnIdentifiableRegisterStatusChange;
		}

		protected virtual void OnDestroy()
		{
			base.Id.EvtIdentifiableRegisterStatusChange -= this.OnIdentifiableRegisterStatusChange;
		}

		private void OnIdentifiableRegisterStatusChange(bool isRegistering)
		{
			if (isRegistering)
			{
				GameHubBehaviour.Hub.UpdateManager.AddMovementStream(base.Id.ObjId, this);
			}
			else
			{
				GameHubBehaviour.Hub.UpdateManager.RemoveMovementStream(base.Id.ObjId);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MovementStream));
	}
}
