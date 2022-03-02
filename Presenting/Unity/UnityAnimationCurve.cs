using System;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityAnimationCurve : IAnimationCurve
	{
		public float Evaluate(float value)
		{
			return this._animationCurve.Evaluate(value);
		}

		[SerializeField]
		private AnimationCurve _animationCurve;
	}
}
