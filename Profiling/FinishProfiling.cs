using System;
using System.Diagnostics;
using System.IO;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Profiling
{
	internal class FinishProfiling : GameHubBehaviour
	{
		private void Awake()
		{
			this.delayWatch = new Stopwatch();
			this.delayWatch.Start();
			this.logFile = new StreamWriter(Application.dataPath + "/../PerformanceLog.log", false);
			this.logFile.AutoFlush = true;
			this.logFile.WriteLine(string.Format("State;Update Time;Late Update Time;Render Time;Update Spikes;Late Update Spike;Render Pikes;FPS", new object[0]));
		}

		private void Update()
		{
			this.updateCount++;
			if ((float)StartProfiling.updateWatch.ElapsedMilliseconds > this.midUpdateTime * 1.5f)
			{
				this.updateSpikes++;
			}
			this.totalUpdateTime += StartProfiling.updateWatch.ElapsedMilliseconds;
			StartProfiling.updateWatch.Stop();
			StartProfiling.updateWatch.Reset();
		}

		private void LateUpdate()
		{
			this.lateUpdateCount++;
			if ((float)StartProfiling.lateUpdateWatch.ElapsedMilliseconds > this.midLateUpdateTime * 1.5f)
			{
				this.lateUpdateSpikes++;
			}
			this.totalLateUpdateTime += StartProfiling.lateUpdateWatch.ElapsedMilliseconds;
			StartProfiling.lateUpdateWatch.Stop();
			StartProfiling.lateUpdateWatch.Reset();
		}

		private void OnPostRender()
		{
			this.postRenderCount++;
			if ((float)StartProfiling.renderingWatch.ElapsedMilliseconds > this.midRenderTime * 1.5f)
			{
				this.renderSpikes++;
			}
			this.totalRenderTime += StartProfiling.renderingWatch.ElapsedMilliseconds;
			StartProfiling.renderingWatch.Stop();
			StartProfiling.renderingWatch.Reset();
			if (this.delayWatch.ElapsedMilliseconds > 1000L)
			{
				this.delayWatch.Reset();
				this.WriteLogs();
				this.delayWatch.Start();
			}
		}

		private void WriteLogs()
		{
			this.midUpdateTime = (float)this.totalUpdateTime / (float)this.updateCount;
			this.midLateUpdateTime = (float)this.totalLateUpdateTime / (float)this.lateUpdateCount;
			this.midRenderTime = (float)this.totalRenderTime / (float)this.postRenderCount;
			this.logFile.WriteLine(string.Format("{0};{1:0.00##};{2:0.00##};{3:0.00##};{4};{5};{6};{7}", new object[]
			{
				GameHubBehaviour.Hub.State.Current.name,
				this.midUpdateTime / 1f,
				this.midLateUpdateTime / 1f,
				this.midRenderTime / 1f,
				this.updateSpikes,
				this.lateUpdateSpikes,
				this.renderSpikes,
				this.lateUpdateCount
			}));
			this.totalUpdateTime = 0L;
			this.totalLateUpdateTime = 0L;
			this.totalRenderTime = 0L;
			this.updateCount = 0;
			this.lateUpdateCount = 0;
			this.postRenderCount = 0;
			this.updateSpikes = 0;
			this.lateUpdateSpikes = 0;
			this.renderSpikes = 0;
		}

		private StreamWriter logFile;

		private Stopwatch delayWatch;

		private int updateCount;

		private int lateUpdateCount;

		private int postRenderCount;

		private int updateSpikes;

		private int lateUpdateSpikes;

		private int renderSpikes;

		private long totalUpdateTime;

		private long totalLateUpdateTime;

		private long totalRenderTime;

		private float midUpdateTime;

		private float midLateUpdateTime;

		private float midRenderTime;
	}
}
