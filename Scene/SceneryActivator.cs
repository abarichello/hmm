using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Scene
{
	public class SceneryActivator : GameHubBehaviour
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub == null || this.Parent == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient() && !this.ClientEnabled)
			{
				return;
			}
			this.Parent.OnUnspawn += this.OnTargetUnspawn;
			this.Parent.OnSpawn += this.OnTargetSpawn;
		}

		private void OnTargetSpawn(SpawnEvent evt)
		{
			if (!base.enabled)
			{
				return;
			}
			for (int i = 0; i < this.Targets.Length; i++)
			{
				Activation activation = this.Targets[i];
				if (activation != null && (!activation.ServerOnly || !GameHubBehaviour.Hub.Net.IsClient()))
				{
					switch (activation.Action)
					{
					case ActionType.EnableOnSpawn:
					case ActionType.EnableOnSpawnDisableOnUnspawn:
						activation.Activate(true, this.Parent.Id.ObjId);
						break;
					case ActionType.DisableOnSpawn:
					case ActionType.EnableOnUnspawnDisableOnSpawn:
						activation.Activate(false, this.Parent.Id.ObjId);
						break;
					}
				}
			}
		}

		private void OnTargetUnspawn(UnspawnEvent evt)
		{
			if (!base.enabled)
			{
				return;
			}
			for (int i = 0; i < this.Targets.Length; i++)
			{
				Activation activation = this.Targets[i];
				if (activation != null && (!activation.ServerOnly || !GameHubBehaviour.Hub.Net.IsClient()))
				{
					switch (activation.Action)
					{
					case ActionType.EnableOnUnspawn:
					case ActionType.EnableOnUnspawnDisableOnSpawn:
						activation.Activate(true, evt.Causer);
						break;
					case ActionType.DisableOnUnspawn:
					case ActionType.EnableOnSpawnDisableOnUnspawn:
						activation.Activate(false, evt.Causer);
						break;
					}
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SceneryActivator));

		public SpawnController Parent;

		public bool ClientEnabled;

		public Activation[] Targets;
	}
}
