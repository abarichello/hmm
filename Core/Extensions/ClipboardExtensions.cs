using System;
using UnityEngine;

namespace HeavyMetalMachines.Core.Extensions
{
	public static class ClipboardExtensions
	{
		public static void CopyToClipboard(this string value)
		{
			TextEditor textEditor = new TextEditor();
			textEditor.text = value;
			textEditor.SelectAll();
			textEditor.Copy();
		}
	}
}
