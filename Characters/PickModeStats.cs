using System;
using UnityEngine;

namespace HeavyMetalMachines.Characters
{
	[Serializable]
	public struct PickModeStats
	{
		[Range(0f, 1f)]
		public float Durability;

		[Range(0f, 1f)]
		public float Repair;

		[Range(0f, 1f)]
		public float Control;

		[Range(0f, 1f)]
		public float Damage;

		[Range(0f, 1f)]
		public float Mobility;
	}
}
