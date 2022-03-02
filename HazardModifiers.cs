using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class HazardModifiers : GameHubScriptableObject, ISerializationCallbackReceiver
	{
		public ModifierData[] Data { get; private set; }

		public void OnAfterDeserialize()
		{
			this.Data = ModifierData.CreateData(this.ModsInfo);
		}

		public void OnBeforeSerialize()
		{
		}

		public ModifierInfo[] ModsInfo;
	}
}
