using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	public class TriggerActivator : GameHubBehaviour
	{
		private int GetObjectId(Collider other)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			if (combat == null)
			{
				return -1;
			}
			return combat.Id.ObjId;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this.EnableOnEnterInverval && this._triggerEnterUpdater.ShouldHalt())
			{
				return;
			}
			int objectId = this.GetObjectId(other);
			if (objectId == -1)
			{
				return;
			}
			for (int i = 0; i < this.Targets.Length; i++)
			{
				Activation activation = this.Targets[i];
				if (activation != null)
				{
					ActionType action = activation.Action;
					if (action != ActionType.EnableOnEnter)
					{
						if (action == ActionType.DisableOnEnter)
						{
							activation.Activate(false, objectId);
						}
					}
					else
					{
						activation.Activate(true, objectId);
					}
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			int objectId = this.GetObjectId(other);
			if (objectId == -1)
			{
				return;
			}
			for (int i = 0; i < this.Targets.Length; i++)
			{
				Activation activation = this.Targets[i];
				if (activation != null)
				{
					ActionType action = activation.Action;
					if (action != ActionType.EnableOnExit)
					{
						if (action == ActionType.DisableOnExit)
						{
							activation.Activate(false, objectId);
						}
					}
					else
					{
						activation.Activate(true, objectId);
					}
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(TriggerActivator));

		public Activation[] Targets;

		private TimedUpdater _triggerEnterUpdater = new TimedUpdater
		{
			PeriodMillis = 3000
		};

		public bool EnableOnEnterInverval;
	}
}
