using System;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera
{
	[Obsolete]
	public struct BaseCameraTarget : ICameraTarget
	{
		public CarCameraMode Mode { get; set; }

		public Transform TargetTransform { get; set; }

		public bool Snap { get; set; }

		public bool Follow { get; set; }

		public bool SmoothTeleport { get; set; }

		public Func<bool> Condition { get; set; }

		public override string ToString()
		{
			return string.Format("[Mode={0} Target={1} SFS={2}]", this.Mode, (!(this.TargetTransform == null)) ? this.TargetTransform.name : "null", ((!this.Snap) ? 0 : 100) + ((!this.Follow) ? 0 : 10) + ((!this.SmoothTeleport) ? 0 : 1));
		}
	}
}
