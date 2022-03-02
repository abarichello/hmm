using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public static class InputUtils
	{
		public static bool IsPressed(bool LCtrl, bool LAlt, bool LShift, bool LWin, string Key)
		{
			return Input.GetKeyDown(Key) && LCtrl == Input.GetKey(306) && LAlt == Input.GetKey(308) && LShift == Input.GetKey(304) && LWin == Input.GetKey(311);
		}

		public static bool IsPressed(bool LCtrl, bool LAlt, bool LShift, bool LWin, KeyCode keyCode)
		{
			return Input.GetKeyDown(keyCode) && LCtrl == Input.GetKey(306) && LAlt == Input.GetKey(308) && LShift == Input.GetKey(304) && LWin == Input.GetKey(311);
		}
	}
}
