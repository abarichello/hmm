using System;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class IntUnityDropdown : UnityDropdown<int>
	{
		public IntUnityDropdown(Dropdown dropdown) : base(dropdown)
		{
		}
	}
}
