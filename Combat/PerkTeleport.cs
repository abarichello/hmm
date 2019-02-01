using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	internal class PerkTeleport : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			Transform targetTransform = base.GetTargetTransform(this.Effect, this.Target);
			Transform targetTransform2 = base.GetTargetTransform(this.Effect, this.Destination);
			targetTransform.position = targetTransform2.position;
		}

		public BasePerk.PerkTarget Target;

		public BasePerk.PerkTarget Destination;
	}
}
