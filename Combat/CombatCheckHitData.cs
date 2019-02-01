using System;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class CombatCheckHitData
	{
		public bool CheckHit(CombatObject causer, CombatObject taker)
		{
			if (taker == null)
			{
				return false;
			}
			if (taker.IsWard && (taker.WardEffect == null || (causer == taker.WardEffect.Attached && !this.CheckHit(causer, causer))))
			{
				return false;
			}
			bool flag = taker == causer && this.HitSelf;
			bool flag2 = taker.Team == causer.Team;
			bool flag3 = (flag2 && this.HitFriends && taker != causer) || (!flag2 && this.HitEnemies);
			bool flag4 = (this.HitCreeps && taker.IsCreep) || (this.HitTurrets && taker.IsTurret) || (this.HitWards && taker.IsWard) || (this.HitBuildings && taker.IsBuilding) || (this.HitPlayers && taker.IsPlayer) || (this.HitBoss && taker.IsBoss);
			bool flag5 = !taker.Attributes.CurrentStatus.HasFlag(StatusKind.Banished) || (taker.Attributes.CurrentBanishCauserCombat == causer && this.HitBanished);
			return flag4 && (flag || flag3) && flag5;
		}

		public bool HitSelf;

		public bool HitEnemies = true;

		public bool HitFriends;

		public bool HitWards = true;

		public bool HitTurrets = true;

		public bool HitBuildings = true;

		public bool HitCreeps = true;

		public bool HitPlayers = true;

		public bool HitBoss = true;

		public bool HitBanished;
	}
}
