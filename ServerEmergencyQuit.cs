using System;
using System.Diagnostics;
using UnityEngine;

namespace HeavyMetalMachines
{
	public static class ServerEmergencyQuit
	{
		public static void Quit()
		{
			Debug.LogError(string.Format("ServerEmergencyQuit now={0:O}", DateTime.Now));
			Process.GetCurrentProcess().Kill();
		}
	}
}
