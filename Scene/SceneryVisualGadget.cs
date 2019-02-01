using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	public abstract class SceneryVisualGadget : GameHubBehaviour
	{
		public abstract void PlayVfxClient(Transform target);

		[SerializeField]
		public CurveInfo curveInfo;
	}
}
