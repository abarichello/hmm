using System;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class Event
	{
		[SerializeField]
		public string Name;

		[SerializeField]
		public UnityAnimation[] Animations;
	}
}
