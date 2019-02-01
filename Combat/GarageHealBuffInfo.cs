using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class GarageHealBuffInfo : GameHubScriptableObject
	{
		public int MaxTimeInterval;

		[Header("/!\\ There's no more heal buff but this must be in the scene until GarageController is updated /!\\")]
		public ModifierInfo[] Heal;
	}
}
