using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using HeavyMetalMachines.DataTransferObjects.Server;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishComm
	{
		public SwordfishComm(long jobId)
		{
			this.JobId = jobId;
			this.StartBeating(1000);
		}

		[DllImport("Hoplon.ClusterManager.Comm.dll")]
		private static extern void UpdateOnlineUsers(long jobId, int count);

		[DllImport("Hoplon.ClusterManager.Comm.dll")]
		private static extern void Heartbeat(long jobId);

		[DllImport("Hoplon.ClusterManager.Comm.dll")]
		private static extern void RaisePriority(long jobId);

		[DllImport("Hoplon.ClusterManager.Comm.dll")]
		private static extern void JobDone(long jobId);

		[DllImport("Hoplon.ClusterManager.Comm.dll")]
		private static extern int GetPort(long jobId);

		[DllImport("Hoplon.ClusterManager.Comm.dll")]
		public static extern int GetJobId();

		[DllImport("Hoplon.ClusterManager.Comm.dll")]
		private static extern void UpdateGameServerStatus(long jobId, string bag);

		[DllImport("Hoplon.ClusterManager.Comm.dll")]
		private static extern long GetCurrentRegionId();

		public void UpdateOnlineUsers(int count)
		{
			SwordfishComm.UpdateOnlineUsers(this.JobId, count);
		}

		public void UpdateGameServerStatus(ServerStatusBag bag)
		{
			SwordfishComm.UpdateGameServerStatus(this.JobId, bag.ToString());
		}

		public void RaisePriority()
		{
			SwordfishComm.RaisePriority(this.JobId);
		}

		public void LowerPriority()
		{
			using (Process currentProcess = Process.GetCurrentProcess())
			{
				currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
			}
		}

		public void JobDone()
		{
			SwordfishComm.JobDone(this.JobId);
		}

		public int GetPort()
		{
			return SwordfishComm.GetPort(this.JobId);
		}

		public long GetRegionId()
		{
			return SwordfishComm.GetCurrentRegionId();
		}

		public void Tick()
		{
			this._tick++;
		}

		private void StartBeating(int intervalMillis)
		{
			this._interval = intervalMillis;
			if (!this._running)
			{
				this._tick = 0;
				this._running = true;
				this._interval = intervalMillis;
				this._unityThread = Thread.CurrentThread;
				this._heartbeat = new Thread(new ThreadStart(this.Beat))
				{
					Name = "SwordfishHeartbeat",
					IsBackground = true
				};
				this._heartbeat.Start();
			}
		}

		private void Beat()
		{
			int num = this._tick;
			TimeSpan t = TimeSpan.FromMinutes(1.0);
			DateTime utcNow = DateTime.UtcNow;
			while (this._running)
			{
				Thread.Sleep(this._interval);
				try
				{
					SwordfishComm.Heartbeat(this.JobId);
				}
				catch (Exception e)
				{
					SwordfishComm.Log.Warn("Exception during Swordfish's Heartbeat:\n", e);
				}
				int tick = this._tick;
				if (tick != num)
				{
					num = tick;
					utcNow = DateTime.UtcNow;
				}
				else if (DateTime.UtcNow - utcNow > t)
				{
					SwordfishComm.Log.FatalFormat("Application not responding, will break process. Alive={0}", new object[]
					{
						this._unityThread.IsAlive
					});
					this.LowerPriority();
					this.JobDone();
					Process.GetCurrentProcess().Kill();
					this._running = false;
				}
			}
		}

		public void Dispose()
		{
			this._running = false;
			this._heartbeat.Join();
			this._heartbeat = null;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SwordfishComm));

		public readonly long JobId;

		private Thread _unityThread;

		private volatile int _tick;

		private volatile bool _running;

		private volatile int _interval;

		private Thread _heartbeat;
	}
}
