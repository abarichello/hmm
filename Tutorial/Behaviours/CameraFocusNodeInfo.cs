using System;
using Holoville.HOTween;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class CameraFocusNodeInfo : TutorialPathNode
	{
		public Transform target;

		public float waitTime = 0.5f;

		public float nextNodeTransitionTime = 1.5f;

		public EaseType easing = EaseType.EaseInOutCubic;
	}
}
