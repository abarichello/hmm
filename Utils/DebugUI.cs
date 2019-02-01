using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace HeavyMetalMachines.Utils
{
	public class DebugUI
	{
		[DllImport("NativeRendering.dll")]
		private static extern void DebugUISetActive(bool value);

		[DllImport("NativeRendering.dll")]
		private static extern IntPtr DebugUIGetRenderEventPtr();

		public static void Toggle()
		{
			if (!DebugUI._enabled)
			{
				if (DebugUI._cmdBuffer == null)
				{
					DebugUI._cmdBuffer = new CommandBuffer();
					DebugUI._cmdBuffer.name = "DebugUI";
					DebugUI._cmdBuffer.IssuePluginEvent(DebugUI.DebugUIGetRenderEventPtr(), 0);
				}
				DebugUI._cachedCamera = UICamera.mainCamera;
				if (DebugUI._cachedCamera == null)
				{
					DebugUI._cachedCamera = Camera.main;
				}
				if (DebugUI._cachedCamera == null)
				{
					return;
				}
				DebugUI._cachedCamera.AddCommandBuffer(CameraEvent.AfterEverything, DebugUI._cmdBuffer);
			}
			else
			{
				DebugUI._cachedCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, DebugUI._cmdBuffer);
				DebugUI._cachedCamera = null;
			}
			DebugUI._enabled = !DebugUI._enabled;
			DebugUI.DebugUISetActive(DebugUI._enabled);
		}

		private static CommandBuffer _cmdBuffer;

		private static Camera _cachedCamera;

		private static bool _enabled;
	}
}
