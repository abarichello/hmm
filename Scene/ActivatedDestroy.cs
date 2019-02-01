using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Scene
{
	public class ActivatedDestroy : GameHubBehaviour, IActivatable
	{
		public void Activate(bool enable, int causer)
		{
			if (!enable)
			{
				return;
			}
			for (int i = 0; i < this.Targets.Length; i++)
			{
				CombatObject combatObject = this.Targets[i];
				if (combatObject && combatObject.IsAlive())
				{
					combatObject.Kill();
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ActivatedDestroy));

		public CombatObject[] Targets;
	}
}
