using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class BombInfo : GameHubScriptableObject
	{
		private void OnEnable()
		{
			BombInfo._instance = this;
		}

		public static BombInfo Instance
		{
			get
			{
				return BombInfo._instance;
			}
		}

		public MovementInfo Movement;

		public TeamKind Team = TeamKind.Neutral;

		public string AssetPickup;

		public string AssetDetonator;

		public ModifierInfo[] Modifiers;

		public int ExplosionRadius = 100;

		public int ExplodeInSeconds = 3;

		public int TimeoutInSeconds = 60;

		public ModifierInfo[] CarrierModifiers;

		private static BombInfo _instance;
	}
}
