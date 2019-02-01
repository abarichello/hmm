using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class TrailPieceDropper
	{
		public int EffectOwnerId;

		public int TrailPiecesCount;

		public CombatObject CombatObject;

		public Vector3 TrailDirection;

		public Vector3 TrailStartPosition;

		public Vector3 LastPiecePosition;

		public TimedUpdater TrailPieceDropTimer = new TimedUpdater(1000, true, false);
	}
}
