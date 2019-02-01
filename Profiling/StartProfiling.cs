using System;
using System.Diagnostics;
using UnityEngine;

namespace HeavyMetalMachines.Profiling
{
	internal class StartProfiling : MonoBehaviour
	{
		private void Awake()
		{
			StartProfiling.updateWatch = new Stopwatch();
			StartProfiling.lateUpdateWatch = new Stopwatch();
			StartProfiling.renderingWatch = new Stopwatch();
		}

		private void Update()
		{
			StartProfiling.updateWatch.Start();
		}

		private void LateUpdate()
		{
			StartProfiling.lateUpdateWatch.Start();
		}

		private void OnPreRender()
		{
			StartProfiling.renderingWatch.Start();
		}

		public static Stopwatch updateWatch;

		public static Stopwatch lateUpdateWatch;

		public static Stopwatch renderingWatch;
	}
}
