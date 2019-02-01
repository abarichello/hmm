using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class ItemTypeAttribute : PropertyAttribute
	{
		public ItemTypeAttribute(Type aType)
		{
			this.PropType = aType;
		}

		public Type PropType;
	}
}
