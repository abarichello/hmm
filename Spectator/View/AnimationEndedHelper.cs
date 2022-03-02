using System;
using UnityEngine;

namespace HeavyMetalMachines.Spectator.View
{
	public class AnimationEndedHelper : MonoBehaviour
	{
		public void AnimationOnWindowExit()
		{
			base.gameObject.SetActive(false);
		}
	}
}
