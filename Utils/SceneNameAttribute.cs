using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class SceneNameAttribute : PropertyAttribute
	{
		public SceneNameAttribute(bool canHaveNull)
		{
			this.CanHaveNull = canHaveNull;
		}

		public int SelectedValue;

		public readonly bool CanHaveNull;
	}
}
