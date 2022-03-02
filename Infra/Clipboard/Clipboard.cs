using System;
using UnityEngine;

namespace HeavymetalMachines.Infra.Clipboard
{
	public static class Clipboard
	{
		public static bool HasClipboardContent()
		{
			return !string.IsNullOrEmpty(GUIUtility.systemCopyBuffer);
		}

		public static string GetClipboard()
		{
			return GUIUtility.systemCopyBuffer;
		}

		public static void SetClipboard(string text)
		{
			GUIUtility.systemCopyBuffer = text;
		}
	}
}
