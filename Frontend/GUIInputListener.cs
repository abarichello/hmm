using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ClientAPI;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public static class GUIInputListener
	{
		static GUIInputListener()
		{
			Delegate onKey = UICamera.onKey;
			if (GUIInputListener.<>f__mg$cache0 == null)
			{
				GUIInputListener.<>f__mg$cache0 = new UICamera.KeyCodeDelegate(GUIInputListener.OnKeyPressed);
			}
			UICamera.onKey = (UICamera.KeyCodeDelegate)Delegate.Combine(onKey, GUIInputListener.<>f__mg$cache0);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event ClientAPI.Action OnKeyStartPressed;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event ClientAPI.Action OnKeyYPressed;

		private static void OnKeyPressed(GameObject go, KeyCode key)
		{
			if (key != KeyCode.JoystickButton3)
			{
				if (key == KeyCode.JoystickButton7)
				{
					if (GUIInputListener.OnKeyStartPressed != null)
					{
						GUIInputListener.OnKeyStartPressed();
					}
				}
			}
			else if (GUIInputListener.OnKeyYPressed != null)
			{
				GUIInputListener.OnKeyYPressed();
			}
		}

		[CompilerGenerated]
		private static UICamera.KeyCodeDelegate <>f__mg$cache0;
	}
}
