using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class PingDrawerAttribute : PropertyAttribute
	{
		public PingDrawerAttribute(Type aType)
		{
			this.PropType = aType;
		}

		public Type PropType;
	}
}
