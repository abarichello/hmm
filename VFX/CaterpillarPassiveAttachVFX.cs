using System;
using HeavyMetalMachines.Render;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class CaterpillarPassiveAttachVFX : AttachVFX
	{
		protected override void OnActivate()
		{
			this.syncRotation = AttachVFX.SynchRotationType.Disabled;
			base.OnActivate();
			if (this._targetFXInfo.Owner)
			{
				this._listener = this._targetFXInfo.Owner.gameObject.GetComponentInChildren<CaterpillarDamageListener>();
			}
			if (this._listener == null)
			{
				base.enabled = false;
				return;
			}
			base.transform.rotation = Quaternion.LookRotation(-this._listener.Direction);
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			if (this._listener)
			{
				base.transform.rotation = Quaternion.LookRotation(-this._listener.Direction);
			}
		}

		protected override void WillDeactivate()
		{
			base.WillDeactivate();
			this._listener = null;
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			this._listener = null;
		}

		private CaterpillarDamageListener _listener;
	}
}
