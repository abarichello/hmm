using System;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class StringUnityDropdown : UnityDropdown<string>
	{
		public StringUnityDropdown(Dropdown dropdown) : base(dropdown)
		{
		}
	}
}
