using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class BitMaskAttribute : PropertyAttribute
	{
		public BitMaskAttribute(Type aType)
		{
			this.PropType = aType;
		}

		public Type PropType;
	}
}
