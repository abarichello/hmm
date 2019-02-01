using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class PhysicsMaterialSetHelper : MonoBehaviour
	{
		public void SetMaterial()
		{
			foreach (Collider collider in base.GetComponentsInChildren<Collider>(true))
			{
				collider.sharedMaterial = this.Material;
			}
		}

		public PhysicMaterial Material;

		[ExecuteMethod("SetMaterial")]
		public bool SetMaterialDummy;
	}
}
