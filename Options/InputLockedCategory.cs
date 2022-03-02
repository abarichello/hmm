using System;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public struct InputLockedCategory
	{
		[Header("[Title Name - Options Sheet]")]
		public string TitleDraft;

		public KeyboardMouseCodeCategory[] KeyboardMouseCodeCategories;
	}
}
