using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMTweenAlphaHack : MonoBehaviour
	{
		public void OnTweenEnd()
		{
			base.GetComponent<UIRect>().Invalidate(true);
		}
	}
}
