using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	internal class PositionReseter : MonoBehaviour
	{
		private void LateUpdate()
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
		}
	}
}
