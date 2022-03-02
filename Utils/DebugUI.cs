using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace HeavyMetalMachines.Utils
{
	public class DebugUI
	{
		[DllImport("particlesystem.plugin")]
		private static extern void DebugUISetActive(bool value);

		[DllImport("particlesystem.plugin")]
		private static extern IntPtr DebugUIGetRenderEventPtr();

		[DllImport("particlesystem.plugin")]
		private static extern void DebugUIPauseHistogram();

		[DllImport("particlesystem.plugin")]
		private static extern void DebugUIResumeHistogram();

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
				DebugUI._cachedCamera.AddCommandBuffer(20, DebugUI._cmdBuffer);
			}
			else
			{
				DebugUI._cachedCamera.RemoveCommandBuffer(20, DebugUI._cmdBuffer);
				DebugUI._cachedCamera = null;
			}
			DebugUI._enabled = !DebugUI._enabled;
			DebugUI.DebugUISetActive(DebugUI._enabled);
		}

		public static void PauseHistogram()
		{
			DebugUI.DebugUIPauseHistogram();
		}

		public static void ResumeHistogram()
		{
			DebugUI.DebugUIResumeHistogram();
		}

		private static CommandBuffer _cmdBuffer;

		private static Camera _cachedCamera;

		private static bool _enabled;
	}
}
