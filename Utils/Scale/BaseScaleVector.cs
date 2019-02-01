using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils.Scale
{
	public abstract class BaseScaleVector
	{
		public BaseScaleVector(Vector3 from, Vector3 to)
		{
			this.SetValuesAndReset(from, to);
		}

		public void SetValuesAndReset(Vector3 from, Vector3 to)
		{
			this._from = from;
			this._to = to;
		}

		protected Vector3 GetTo()
		{
			return this._to;
		}

		protected Vector3 TransformVector(float factor)
		{
			return this._from * (1f - factor) + this._to * factor;
		}

		private Vector3 _from;

		private Vector3 _to;
	}
}
