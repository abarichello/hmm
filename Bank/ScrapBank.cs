using System;
using System.Collections.Generic;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Combat;
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
			if (this.PlayerAccounts.Count > 0 && GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.BombDelivery && !this._updater.ShouldHalt())
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

		public bool HaveEnoughScrap(int id, int scrapAmount)
		{
			if (GameHubBehaviour.Hub.ScrapLevel.FreeUpgrades)
			{
				return true;
			}
			PlayerStats playerStats;
			if (!this.PlayerAccounts.TryGetValue(id, out playerStats))
			{
				ScrapBank.Log.ErrorFormat("Failed to find scrap acount for player={0}", new object[]
				{
					id
				});
				return false;
			}
			return playerStats.Scrap >= scrapAmount;
		}

		public int GetScraps(int id)
		{
			PlayerStats playerStats;
			if (!this.PlayerAccounts.TryGetValue(id, out playerStats))
			{
				ScrapBank.Log.ErrorFormat("Failed to find scrap acount for player={0}", new object[]
				{
					id
				});
				return -1;
			}
			return playerStats.Scrap;
		}

		public bool SpendScrap(int id, int scrapAmount)
		{
			if (GameHubBehaviour.Hub.ScrapLevel.FreeUpgrades)
			{
				return true;
			}
			PlayerStats playerStats;
			if (!this.PlayerAccounts.TryGetValue(id, out playerStats))
			{
				ScrapBank.Log.ErrorFormat("Failed to find scrap acount for player={0}", new object[]
				{
					id
				});
				return false;
			}
			if (!playerStats.SpendScrap(scrapAmount))
			{
				return false;
			}
			playerStats.ScrapSpent += scrapAmount;
			return true;
		}

		public bool AddScrap(int id, int scrapAmount, ScrapBank.ScrapReason reason)
		{
			PlayerStats playerStats;
			if (!this.PlayerAccounts.TryGetValue(id, out playerStats))
			{
				ScrapBank.Log.ErrorFormat("AddScrap - Failed to find scrap acount for player={0}", new object[]
				{
					id
				});
				return false;
			}
			playerStats.AddScrap(scrapAmount, true, reason);
			return true;
		}

		public int GetPlayerBounty(int playerId, int killerId, CombatObject killer, int possibleKillerId)
		{
			PlayerStats playerStats;
			if (!this.PlayerAccounts.TryGetValue(playerId, out playerStats))
			{
				ScrapBank.Log.ErrorFormat("Not really a player victim id={0}", new object[]
				{
					playerId
				});
				return 0;
			}
			PlayerStats playerStats2;
			if (!this.PlayerAccounts.TryGetValue(killerId, out playerStats2))
			{
				CombatObject combatObject = (!(killer != null) || !killer.IsCreep) ? null : killer;
				if (combatObject != null && this.PlayerAccounts.TryGetValue(combatObject.Creep.ParentId, out playerStats2))
				{
					killerId = combatObject.Creep.ParentId;
					this.PlayerAccounts.TryGetValue(killerId, out playerStats2);
				}
				else if (possibleKillerId != -1)
				{
					killerId = possibleKillerId;
					this.PlayerAccounts.TryGetValue(killerId, out playerStats2);
				}
			}
			Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(killerId);
			TeamKind teamKind = TeamKind.Zero;
			CombatObject component;
			if (@object != null && (component = @object.GetComponent<CombatObject>()) != null)
			{
				teamKind = component.Team;
			}
			else
			{
				TeamKind team = playerStats.Combat.Team;
				if (team != TeamKind.Red)
				{
					if (team == TeamKind.Blue)
					{
						teamKind = TeamKind.Red;
					}
				}
				else
				{
					teamKind = TeamKind.Blue;
				}
			}
			int num = -1;
			if (playerStats2 != null)
			{
				num = playerStats2.Level;
			}
			else
			{
				Dictionary<int, PlayerStats> dictionary = null;
				if (teamKind != TeamKind.Blue)
				{
					if (teamKind == TeamKind.Red)
					{
						dictionary = this.TeamRedAccounts;
					}
				}
				else
				{
					dictionary = this.TeamBluAccounts;
				}
				if (dictionary != null && dictionary.Count > 0)
				{
					foreach (PlayerStats playerStats3 in dictionary.Values)
					{
						num += playerStats3.Level;
					}
					num /= dictionary.Count;
				}
			}
			int value = GameHubBehaviour.Hub.ScrapLevel.ScrapPerPlayerKill.Value;
			int num2 = (num > 0) ? (Mathf.Clamp(playerStats.Level - num, -GameHubBehaviour.Hub.ScrapLevel.ScrapPerPlayerKillLevelDiff, GameHubBehaviour.Hub.ScrapLevel.ScrapPerPlayerKillLevelDiff) * GameHubBehaviour.Hub.ScrapLevel.ScrapPerPlayerKillLevelBonus) : 0;
			float num3 = GameHubBehaviour.Hub.ScrapLevel.ScrapPerPlayerKillDeathStreakMaxReduction * Mathf.InverseLerp(0f, (float)GameHubBehaviour.Hub.ScrapLevel.ScrapPerPlayerKillDeathStreakMaxAmount, (float)playerStats.CurrentDeathStreak);
			int num4 = Mathf.RoundToInt((float)(value + num2) * (1f - num3));
			int num5 = GameHubBehaviour.Hub.MatchHistory.FirstBloodOccurred ? 0 : GameHubBehaviour.Hub.ScrapLevel.ScrapPerFirstBlood.Value;
			int num6 = GameHubBehaviour.Hub.ScrapLevel.ScrapPerKillStreakEndBase.Value * GameHubBehaviour.Hub.ScrapLevel.ScrapPerKillStreakEndLevels[Mathf.Min(playerStats.CurrentKillingStreak, GameHubBehaviour.Hub.ScrapLevel.ScrapPerKillStreakEndLevels.Length - 1)];
			int num7 = num4 + num5 + num6;
			return (num7 <= 0) ? 0 : num7;
		}

		public void CreepKilled(int creep, int killer)
		{
			PlayerStats playerStats;
			if (!this.PlayerAccounts.TryGetValue(killer, out playerStats))
			{
				return;
			}
			playerStats.CreepKills++;
		}

		private void ScrapCollected(int player, int amount, ScrapIncomeKind kind)
		{
			PlayerStats playerStats;
			if (this.PlayerAccounts.TryGetValue(player, out playerStats))
			{
				playerStats.ScrapCollected += amount;
				BombMatchBI.ScrapContributed(player, amount, kind);
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
