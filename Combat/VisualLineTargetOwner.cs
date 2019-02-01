using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class VisualLineTargetOwner : GameHubBehaviour
	{
		private void Start()
		{
			this.Line.SetVertexCount(2);
		}

		private void Update()
		{
			Identifiable target = this.Effect.Target;
			Identifiable owner = this.Effect.Owner;
			if (!target || !owner)
			{
				return;
			}
			this.Line.SetPosition(0, target.transform.position);
			this.Line.SetPosition(1, owner.transform.position);
		}

		public BaseFX Effect;

		public LineRenderer Line;
	}
}
