using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class StickToGroundVFX : BaseVFX
	{
		private void Start()
		{
		}

		private void LateUpdate()
		{
			if (!this.activated)
			{
				return;
			}
			Vector3 position = base.transform.position;
			position.y = 0.1f;
			base.transform.position = position;
		}

		protected override void OnActivate()
		{
			this.activated = true;
			Vector3 position = base.transform.position;
			position.y = 0.1f;
			base.transform.position = position;
		}

		protected override void WillDeactivate()
		{
			this.activated = false;
		}

		protected override void OnDeactivate()
		{
			this.activated = false;
		}

		private bool activated;
	}
}
