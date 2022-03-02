using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class SceneNameAttribute : PropertyAttribute
	{
		public SceneNameAttribute(bool canHaveNull, bool onlyScenesFromBuild = true)
		{
			this.CanHaveNull = canHaveNull;
			this.OnlyScenesFromBuild = onlyScenesFromBuild;
		}

		public int SelectedValue;

		public readonly bool CanHaveNull;

		public readonly bool OnlyScenesFromBuild;
	}
}
