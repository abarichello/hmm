using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public class RestrictAttribute : PropertyAttribute
	{
		public RestrictAttribute(bool isRequired, params Type[] types)
		{
			this.IsRequired = isRequired;
			this.Types = types;
		}

		public bool IsRequired;

		public Type[] Types;
	}
}
