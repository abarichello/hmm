using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class PrefabViewerAttribute : PropertyAttribute
	{
		public PrefabViewerAttribute(string customLabelText = "")
		{
			this.CustomLabelText = customLabelText;
		}

		public string CustomLabelText { get; private set; }
	}
}
