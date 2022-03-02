using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Bank
{
	public class ScrapBank : GameHubBehaviour
	{
		public int StartingScrap
		{
			get
			{
				return this._startingScrap;
			}
		}

		private void Awake()
		{
			this._updater = new TimedUpdater(1000 * GameHubBehaviour.Hub.ScrapLevel.TimedScrapInterval, true, false);
			this._startingScrap = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.StartingScrap, GameHubBehaviour.Hub.ScrapLevel.StartingScrap);
			this.RedScrap = (this.BluScrap = this._startingScrap);
		}

		private void Update()
		{
			if (this.PlayerAccounts.Count > 0 && GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.BombDelivery && !this._updater.ShouldHalt())
			{
				foreach (KeyValuePair<int, PlayerStats> keyValuePair in this.PlayerAccounts)
				{
					keyValuePair.Value.AddScrap(GameHubBehaviour.Hub.ScrapLevel.TimedScrapValue, false, ScrapBank.ScrapReason.time);
				}
				this.RedScrap += GameHubBehaviour.Hub.ScrapLevel.TimedScrapValue;
				this.BluScrap += GameHubBehaviour.Hub.ScrapLevel.TimedScrapValue;
			}
		}

		public bool RegisterPlayer(int id, PlayerStats scraps)
		{
			if (this.PlayerAccounts.ContainsKey(id))
			{
				return false;
			}
			scraps.AddScrap(this._startingScrap, false, ScrapBank.ScrapReason.none);
			this.PlayerAccounts.Add(id, scraps);
			TeamKind team = scraps.Combat.Team;
			if (team != TeamKind.Blue)
			{
				if (team == TeamKind.Red)
				{
					this.TeamRedAccounts.Add(id, scraps);
					this.RedTeamCombatObjects.Add(scraps.Combat);
				}
			}
			else
			{
				this.TeamBluAccounts.Add(id, scraps);
				this.BluTeamCombatObjects.Add(scraps.Combat);
			}
			return true;
		}

		private void ScrapCollected(int player, int amount, ScrapIncomeKind kind)
		{
			PlayerStats playerStats;
			if (this.PlayerAccounts.TryGetValue(player, out playerStats))
			{
				playerStats.ScrapCollected += amount;
			}
		}

		public void AddPickupReward(int kilerId, int index, Vector3 position)
		{
			this.AddPlayerReward(kilerId, GameHubBehaviour.Hub.ScrapLevel.ScrapPickupValue[index], position, ScrapBank.ScrapReason.creep);
		}

		public void AddPlayerReward(int killerId, ScrapInfo scrapInfo, Vector3 position, ScrapBank.ScrapReason reason)
		{
			int value = scrapInfo.Value;
			ScrapTeamDivisionKind teamDivision = scrapInfo.TeamDivision;
			bool reliable = scrapInfo.Reliable;
			bool useBonus = scrapInfo.UseBonus;
			this.ScrapCollected(killerId, value, ScrapIncomeKind.CreepKill);
			this.AddReward(killerId, TeamKind.Neutral, value, teamDivision, reliable, useBonus, position, reason);
		}

		public void AddReward(int killerId, TeamKind killerTeam, int scrapAmount, ScrapTeamDivisionKind teamDivision, bool reliable, bool useBonus, Vector3 position, ScrapBank.ScrapReason reason)
		{
			if (scrapAmount <= 0)
			{
				return;
			}
			if (this.PlayerAccounts.ContainsKey(killerId))
			{
				this.AddRewardToPlayer(killerId, scrapAmount, teamDivision, reliable, useBonus, position, reason);
			}
			else
			{
				this.AddRewardToTeam(killerTeam, scrapAmount, teamDivision, reliable, reason);
			}
		}

		private void AddRewardToPlayer(int id, int scrapAmount, ScrapTeamDivisionKind teamDivision, bool reliable, bool useBonus, Vector3 position, ScrapBank.ScrapReason reason)
		{
			if (useBonus)
			{
				CombatAttributes attributes = this.PlayerAccounts[id].Combat.Attributes;
				scrapAmount += (int)((float)scrapAmount * attributes.ScrapBonusPct) + attributes.ScrapBonus;
			}
			TeamKind team = this.PlayerAccounts[id].Combat.Team;
			Dictionary<int, PlayerStats> dictionary;
			if (team != TeamKind.Blue)
			{
				if (team != TeamKind.Red)
				{
					return;
				}
				dictionary = this.TeamRedAccounts;
				this.RedScrap += scrapAmount;
			}
			else
			{
				dictionary = this.TeamBluAccounts;
				this.BluScrap += scrapAmount;
			}
			switch (teamDivision)
			{
			case ScrapTeamDivisionKind.None:
				this.PlayerAccounts[id].AddScrap(scrapAmount, reliable, reason);
				break;
			case ScrapTeamDivisionKind.GiveAll:
				foreach (KeyValuePair<int, PlayerStats> keyValuePair in dictionary)
				{
					keyValuePair.Value.AddScrap(scrapAmount, reliable, reason);
				}
				break;
			case ScrapTeamDivisionKind.StrifeSplit:
			{
				List<PlayerStats> list = new List<PlayerStats>();
				foreach (KeyValuePair<int, PlayerStats> keyValuePair2 in dictionary)
				{
					if ((keyValuePair2.Value.Combat.Transform.position - position).sqrMagnitude <= GameHubBehaviour.Hub.ScrapLevel.StrifeSplitRangeSqr)
					{
						list.Add(keyValuePair2.Value);
					}
				}
				if (!list.Contains(this.PlayerAccounts[id]))
				{
					list.Add(this.PlayerAccounts[id]);
				}
				if (list.Count > 1)
				{
					scrapAmount /= list.Count;
					for (int i = 0; i < list.Count; i++)
					{
						list[i].AddScrap(scrapAmount, reliable, reason);
					}
				}
				else
				{
					if (list.Count == 1)
					{
						scrapAmount /= 2;
						list[0].AddScrap(scrapAmount, reliable, reason);
					}
					scrapAmount /= GameHubBehaviour.Hub.ScrapLevel.ScrapSplitTeamSize;
					foreach (KeyValuePair<int, PlayerStats> keyValuePair3 in dictionary)
					{
						keyValuePair3.Value.AddScrap(scrapAmount, reliable, reason);
					}
				}
				break;
			}
			case ScrapTeamDivisionKind.LaneSplit:
			{
				List<PlayerStats> list = new List<PlayerStats>();
				foreach (KeyValuePair<int, PlayerStats> keyValuePair4 in dictionary)
				{
					if ((keyValuePair4.Value.Combat.Transform.position - position).sqrMagnitude <= GameHubBehaviour.Hub.ScrapLevel.StrifeSplitRangeSqr)
					{
						list.Add(keyValuePair4.Value);
					}
				}
				if (!list.Contains(this.PlayerAccounts[id]))
				{
					list.Add(this.PlayerAccounts[id]);
				}
				scrapAmount /= list.Count;
				for (int j = 0; j < list.Count; j++)
				{
					list[j].AddScrap(scrapAmount, reliable, reason);
				}
				break;
			}
			}
		}

		public void AddBombReward(TeamKind team, int causer, int round)
		{
			ScrapInfo scrapInfo = GameHubBehaviour.Hub.ScrapLevel.ScrapPerBombDelivery[round];
			if (team != TeamKind.Red)
			{
				if (team == TeamKind.Blue)
				{
					if (this.TeamBluAccounts.ContainsKey(causer))
					{
						this.ScrapCollected(causer, scrapInfo.Value, ScrapIncomeKind.BombDelivery);
					}
				}
			}
			else if (this.TeamRedAccounts.ContainsKey(causer))
			{
				this.ScrapCollected(causer, scrapInfo.Value, ScrapIncomeKind.BombDelivery);
			}
			this.AddRewardToTeam(team, scrapInfo.Value, scrapInfo.TeamDivision, scrapInfo.Reliable, ScrapBank.ScrapReason.bomb);
		}

		private void AddRewardToTeam(TeamKind killerTeamKind, int scrapAmount, ScrapTeamDivisionKind teamDivision, bool reliable, ScrapBank.ScrapReason reason)
		{
			Dictionary<int, PlayerStats> dictionary;
			if (killerTeamKind != TeamKind.Blue)
			{
				if (killerTeamKind != TeamKind.Red)
				{
					return;
				}
				dictionary = this.TeamRedAccounts;
				this.RedScrap += scrapAmount;
			}
			else
			{
				dictionary = this.TeamBluAccounts;
				this.BluScrap += scrapAmount;
			}
			switch (teamDivision)
			{
			case ScrapTeamDivisionKind.None:
				scrapAmount /= GameHubBehaviour.Hub.ScrapLevel.ScrapSplitTeamSize;
				foreach (KeyValuePair<int, PlayerStats> keyValuePair in dictionary)
				{
					keyValuePair.Value.AddScrap(scrapAmount, reliable, reason);
				}
				break;
			case ScrapTeamDivisionKind.GiveAll:
				foreach (KeyValuePair<int, PlayerStats> keyValuePair2 in dictionary)
				{
					keyValuePair2.Value.AddScrap(scrapAmount, reliable, reason);
				}
				break;
			case ScrapTeamDivisionKind.StrifeSplit:
				scrapAmount /= GameHubBehaviour.Hub.ScrapLevel.ScrapSplitTeamSize;
				foreach (KeyValuePair<int, PlayerStats> keyValuePair3 in dictionary)
				{
					keyValuePair3.Value.AddScrap(scrapAmount, reliable, reason);
				}
				break;
			case ScrapTeamDivisionKind.LaneSplit:
				scrapAmount /= GameHubBehaviour.Hub.ScrapLevel.ScrapSplitTeamSize;
				foreach (KeyValuePair<int, PlayerStats> keyValuePair4 in dictionary)
				{
					keyValuePair4.Value.AddScrap(scrapAmount, reliable, reason);
				}
				break;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ScrapBank));

		public readonly Dictionary<int, PlayerStats> PlayerAccounts = new Dictionary<int, PlayerStats>();

		public readonly Dictionary<int, PlayerStats> TeamBluAccounts = new Dictionary<int, PlayerStats>();

		public readonly Dictionary<int, PlayerStats> TeamRedAccounts = new Dictionary<int, PlayerStats>();

		public readonly List<CombatObject> RedTeamCombatObjects = new List<CombatObject>();

		public readonly List<CombatObject> BluTeamCombatObjects = new List<CombatObject>();

		private int _startingScrap;

		public int TotalScrapMetalRed;

		public int TotalScrapMetalBlue;

		public int RedScrap;

		public int BluScrap;

		private TimedUpdater _updater;

		public enum ScrapReason
		{
			kill = 1,
			creep,
			time,
			turret,
			none,
			sellUpgrade,
			sellUpgradeBuyback,
			bomb
		}
	}
}
