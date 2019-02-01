using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public static class InputUtils
	{
		public static bool IsPressed(bool LCtrl, bool LAlt, bool LShift, bool LWin, string Key)
		{
			return Input.GetKeyDown(Key) && LCtrl == Input.GetKey(KeyCode.LeftControl) && LAlt == Input.GetKey(KeyCode.LeftAlt) && LShift == Input.GetKey(KeyCode.LeftShift) && LWin == Input.GetKey(KeyCode.LeftWindows);
		}

		public static bool IsPressed(bool LCtrl, bool LAlt, bool LShift, bool LWin, KeyCode keyCode)
		{
			return Input.GetKeyDown(keyCode) && LCtrl == Input.GetKey(KeyCode.LeftControl) && LAlt == Input.GetKey(KeyCode.LeftAlt) && LShift == Input.GetKey(KeyCode.LeftShift) && LWin == Input.GetKey(KeyCode.LeftWindows);
		}
	}
}
