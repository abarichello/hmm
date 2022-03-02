using System;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	public class UnityTextColorFormatter : ITextColorFormatter
	{
		public string Format(string text, Color color)
		{
			string arg = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", new object[]
			{
				Mathf.RoundToInt(color.R * 255f),
				Mathf.RoundToInt(color.G * 255f),
				Mathf.RoundToInt(color.B * 255f),
				Mathf.RoundToInt(color.A * 255f)
			});
			return string.Format("<color={0}>{1}</color>", arg, text);
		}
	}
}
