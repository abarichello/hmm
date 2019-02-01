using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public static class GUILayoutUtil
	{
		public static void TextAreaToInt(ref int value)
		{
			string text = GUILayout.TextArea(value.ToString(), new GUILayoutOption[0]);
			if (string.IsNullOrEmpty(text))
			{
				value = 0;
				return;
			}
			int num;
			if (int.TryParse(text, out num))
			{
				value = num;
			}
		}
	}
}
