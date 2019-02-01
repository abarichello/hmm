using System;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class HitMask : IHitMask
	{
		public bool Self
		{
			get
			{
				return this.HitSelf;
			}
		}

		public bool Enemies
		{
			get
			{
				return this.HitEnemies;
			}
		}

		public bool Friends
		{
			get
			{
				return this.HitFriends;
			}
		}

		public bool Bomb
		{
			get
			{
				return this.HitBomb;
			}
		}

		public bool Wards
		{
			get
			{
				return this.HitWards;
			}
		}

		public bool Turrets
		{
			get
			{
				return this.HitTurrets;
			}
		}

		public bool Buildings
		{
			get
			{
				return this.HitBuildings;
			}
		}

		public bool Creeps
		{
			get
			{
				return this.HitCreeps;
			}
		}

		public bool Players
		{
			get
			{
				return this.HitPlayers;
			}
		}

		public bool Boss
		{
			get
			{
				return this.HitBoss;
			}
		}

		public bool Banished
		{
			get
			{
				return this.HitBanished;
			}
		}

		public bool HitSelf;

		public bool HitEnemies = true;

		public bool HitFriends;

		public bool HitBomb;

		public bool HitWards = true;

		public bool HitTurrets = true;

		public bool HitBuildings = true;

		public bool HitCreeps = true;

		public bool HitPlayers = true;

		public bool HitBoss = true;

		public bool HitBanished;
	}
}
