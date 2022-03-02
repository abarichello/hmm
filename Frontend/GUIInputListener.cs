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
		public static event Action OnKeyStartPressed;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action OnKeyYPressed;

		private static void OnKeyPressed(GameObject go, KeyCode key)
		{
			if (key != 333)
			{
				if (key == 337)
				{
					if (GUIInputListener.OnKeyStartPressed != null)
					{
						GUIInputListener.OnKeyStartPressed.Invoke();
					}
				}
			}
			else if (GUIInputListener.OnKeyYPressed != null)
			{
				GUIInputListener.OnKeyYPressed.Invoke();
			}
		}

		[CompilerGenerated]
		private static UICamera.KeyCodeDelegate <>f__mg$cache0;
	}
}
