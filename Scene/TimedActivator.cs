using System;
using Pocketverse;

namespace HeavyMetalMachines.Scene
{
	public class TimedActivator : GameHubBehaviour
	{
		private void Start()
		{
			this._warmup = true;
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			if (!this.Parent)
			{
				this.Parent = base.Id;
			}
		}

		private void CheckWarmup()
		{
			if (!GameHubBehaviour.Hub.MatchMan.WarmupDone)
			{
				return;
			}
			this._warmup = false;
			if (this.Delay > 0)
			{
				this._updater.PeriodSeconds = this.Delay;
				this._updater.ShouldHalt();
				this._inDelay = true;
				return;
			}
			this._updater.PeriodSeconds = this.Cycle;
			this._inDelay = false;
		}

		private void Update()
		{
			if (this._warmup)
			{
				this.CheckWarmup();
				return;
			}
			if (this._updater.ShouldHalt())
			{
				return;
			}
			if (this._inDelay)
			{
				this._inDelay = false;
				this._updater.PeriodSeconds = this.Cycle;
				this._updater.Reset();
				this._updater.ShouldHalt();
			}
			for (int i = 0; i < this.Targets.Length; i++)
			{
				this.Targets[i].Activate(true, this.Parent.ObjId);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(TimedActivator));

		public Identifiable Parent;

		public Activation[] Targets;

		public int Cycle;

		public int Delay;

		private TimedUpdater _updater;

		private bool _inDelay;

		private bool _warmup;
	}
}
