using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	public class CarIndicatorArrowSettings : GameHubScriptableObject
	{
		[Header("Scale")]
		public float ScaleArrowsModifierX;

		public float ScaleArrowsModifierY;

		public float ScaleArrowsModifierZ;

		[Header("Arrow Distance")]
		public int CarArrowDistance;

		[Header("Colors")]
		public Color NeutralColor;

		public Color AllyColor;

		public Color EnemyColor;
	}
}
