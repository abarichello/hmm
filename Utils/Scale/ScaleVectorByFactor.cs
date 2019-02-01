using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils.Scale
{
	public class ScaleVectorByFactor : BaseScaleVector
	{
		public ScaleVectorByFactor(Vector3 from, Vector3 to) : base(from, to)
		{
		}

		public virtual Vector3 Update(float factor)
		{
			return base.TransformVector(factor);
		}
	}
}
