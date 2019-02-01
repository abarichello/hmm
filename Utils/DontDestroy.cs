using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class DontDestroy : GameHubBehaviour
	{
		private void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}
}
