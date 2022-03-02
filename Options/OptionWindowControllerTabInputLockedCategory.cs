using System;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public struct OptionWindowControllerTabInputLockedCategory
	{
		[Header("[Title Name - Options Sheet]")]
		public string TitleDraft;

		public InputLockedCategory[] InputLockedCategories;
	}
}
