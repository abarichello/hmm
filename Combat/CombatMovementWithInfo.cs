using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[Obsolete]
	public class CombatMovementWithInfo : CombatMovement
	{
		public override MovementInfo Info
		{
			get
			{
				return this._movementInfo;
			}
		}

		[SerializeField]
		private MovementInfo _movementInfo;
	}
}
