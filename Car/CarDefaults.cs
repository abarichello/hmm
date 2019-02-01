using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	[Serializable]
	public class CarDefaults : GameHubScriptableObject
	{
		public float DeltaPush(int idx, int otherIdx)
		{
			int num = otherIdx - idx;
			for (int i = 0; i < this.MassDeltaPush.Length; i++)
			{
				Vector2 vector = this.MassDeltaPush[i];
				if ((int)vector.x == num)
				{
					return vector.y;
				}
			}
			return this.SceneryBounce;
		}

		public float MinimumSpeed = 1f;

		public float MinimumAngSpeed = 1f;

		public float SceneryBounce = 0.5f;

		public float CollisionInvertedCenterMultiplierX = -1f;

		public float CollisionInvertedCenterMultiplierZ = -3f;

		public float CollisionInvertedCenterHoldSeconds = 1f;

		public Vector2[] MassDeltaPush;
	}
}
