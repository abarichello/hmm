using System;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class NguiCameraAdapter : ScriptableObject
	{
		public bool RegisterCamera(Camera cam)
		{
			if (this._camera == null)
			{
				this._camera = cam;
				return true;
			}
			return false;
		}

		public bool HasCamera(out Camera cam)
		{
			cam = this._camera;
			return cam != null;
		}

		private Camera _camera;
	}
}
