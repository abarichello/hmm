using System;
using Pocketverse;

namespace HeavyMetalMachines.UpdateStream
{
	public abstract class MovementStream : GameHubBehaviour
	{
		public abstract void Read(BitStream stream, double offset);

		public abstract void Write(BitStream stream);

		public abstract void Clear();

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
				MovementStream.Log.DebugFormat("{0} started id={1}", new object[]
				{
					base.name,
					base.Id.ObjId
				});
				GameHubBehaviour.Hub.UpdateManager.AddMovementStream(base.Id.ObjId, this);
			}
			else
			{
				MovementStream.Log.DebugFormat("{0} stopped", new object[]
				{
					base.name
				});
				GameHubBehaviour.Hub.UpdateManager.RemoveMovementStream(base.Id.ObjId);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MovementStream));
	}
}
