using System;
using HeavyMetalMachines.Fog;
using Pocketverse;

namespace HeavyMetalMachines.UpdateStream
{
	public class StreamObject : GameHubBehaviour
	{
		private void Awake()
		{
			if (!base.Id || !GameHubBehaviour.Hub)
			{
				base.enabled = false;
				return;
			}
			this._interpolator = base.GetComponent<TRCInterpolator>();
			base.Id.EvtIdentifiableRegisterStatusChange += this.OnIdentifiableRegisterStatusChange;
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.UpdateManager.RemoveObject(base.Id.ObjId);
			}
			if (!base.Id || !GameHubBehaviour.Hub)
			{
				return;
			}
			base.Id.EvtIdentifiableRegisterStatusChange -= this.OnIdentifiableRegisterStatusChange;
		}

		public int StreamWrite(ref byte[] data)
		{
			return this._interpolator.GetContents(ref data);
		}

		public void StreamRead(bool vis, BitStream stream, int offset)
		{
			if (stream != null)
			{
				this._interpolator.ApplyContents(stream, offset);
			}
			base.gameObject.SetActive(vis);
		}

		public void OnIdentifiableRegisterStatusChange(bool registeredStatus)
		{
			if (registeredStatus)
			{
				GameHubBehaviour.Hub.UpdateManager.AddObject(base.Id.ObjId, this);
				return;
			}
			GameHubBehaviour.Hub.UpdateManager.RemoveObject(base.Id.ObjId);
		}

		private TRCInterpolator _interpolator;
	}
}
