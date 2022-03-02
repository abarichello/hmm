using System;
using Hoplon.Input;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public struct KeyboardMouseCodeCategory
	{
		[Header("[Title Name - Options Sheet]")]
		public string TitleDraft;

		public KeyboardMouseCode KeyboardMouseCode;

		public JoystickTemplateCode JoystickCode;
	}
}
