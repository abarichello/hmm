using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class FXInfo : IHitMask
	{
		public bool Exists()
		{
			return !string.IsNullOrEmpty(this.Effect);
		}

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

		[ScriptId]
		public int EffectId;

		public string Effect;

		public float Height;

		public float MaxYDiff;

		public CDummy.ShotPosAndDir ShotPosAndDir;

		public bool Instantaneous;

		public bool PrioritizeBarrier;

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

		[Header("Consider a single character when setting this number!")]
		[Tooltip("When the same prefab is shared by more than one effect, this number is cumulative.")]
		public int EffectPreCacheCount = 1;

		public bool ForceCreation;
	}
}
