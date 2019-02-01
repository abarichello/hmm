using System;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial
{
	public class TutorialDataReferenceAttribute : PropertyAttribute
	{
		public TutorialDataReferenceAttribute(bool canHaveNull)
		{
			this.CanHaveNull = canHaveNull;
		}

		public readonly bool CanHaveNull;
	}
}
