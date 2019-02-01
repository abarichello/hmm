using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Scene
{
	public class GameActivator : GameHubBehaviour
	{
		private void Start()
		{
			if (!this.Parent)
			{
				this.Parent = base.Id;
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
		}

		private void OnPhaseChange(BombScoreBoard.State state)
		{
			for (int i = 0; i < this.Targets.Length; i++)
			{
				if (!this.Targets[i].ServerOnly || !GameHubBehaviour.Hub.Net.IsClient())
				{
					this.Targets[i].Activate(state == this.ActivatedState, (!this.Parent) ? -1 : this.Parent.ObjId);
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GameActivator));

		public BombScoreBoard.State ActivatedState = BombScoreBoard.State.BombDelivery;

		public Identifiable Parent;

		public Activation[] Targets;
	}
}
