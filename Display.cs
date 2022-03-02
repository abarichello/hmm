using System;
using UnityEngine;

namespace HeavyMetalMachines
{
	public struct Display
	{
		public override string ToString()
		{
			return this.Index.ToString();
		}

		public int Index;

		public RectInt Rect;

		public Resolution Res;
	}
}
