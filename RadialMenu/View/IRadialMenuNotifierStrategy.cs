using System;
using UnityEngine;

namespace HeavyMetalMachines.RadialMenu.View
{
	public interface IRadialMenuNotifierStrategy
	{
		void Reset();

		void SetPosition();

		void GetDirectionData(out Vector3 normalizedDirection, out float magnitude);
	}
}
