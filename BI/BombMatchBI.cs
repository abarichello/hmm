using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.BI
{
	public static class BombMatchBI
	{
		private static HMMHub Hub
		{
			get
			{
				return GameHubBehaviour.Hub;
			}
		}

		public static string Prefix(this ScrapIncomeKind kind)
		{
			switch (kind)
			{
			default:
				return "K";
			case ScrapIncomeKind.Assist:
				return "A";
			case ScrapIncomeKind.BombDelivery:
				return "B";
			case ScrapIncomeKind.CreepKill:
				return "C";
			}
		}

		public static string Prefix(this GarageController.UpgradeOperationKind kind)
		{
			switch (kind)
			{
			default:
				return "B";
			case GarageController.UpgradeOperationKind.Return:
				return "R";
			case GarageController.UpgradeOperationKind.Sell:
				return "S";
			}
		}

		private static string FileName
		{
			get
			{
				return string.Format("MatchStats-{0}.csv", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
			}
		}

		private static void WriteToFile(string contents)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(BombMatchBI.FileName);
				if (!fileInfo.Directory.Exists)
				{
					fileInfo.Directory.Create();
				}
				StreamWriter streamWriter = new StreamWriter(fileInfo.FullName);
				streamWriter.WriteLine(contents);
				streamWriter.Flush();
				streamWriter.Close();
			}
			catch (Exception e)
			{
				BombMatchBI.Log.Fatal("Failed to write BI log to file.", e);
			}
		}

		private static BombMatchBI.OverallStatistics CurrentRound
		{
			get
			{
				BombMatchBI.OverallStatistics result;
				if ((result = BombMatchBI._currentRound) == null)
				{
					result = (BombMatchBI._currentRound = new BombMatchBI.OverallStatistics());
				}
				return result;
			}
		}

		public static void CreatePlayer(PlayerData data)
		{
			int playerCarObjectId = data.GetPlayerCarObjectId();
			BombMatchBI.PlayerStatistics playerStatistics = new BombMatchBI.PlayerStatistics();
			playerStatistics.ObjectId = playerCarObjectId;
			playerStatistics.Name = data.Name;
			playerStatistics.Team = data.Team;
			playerStatistics.Character = data.Character.BIName;
			BombMatchBI._players[playerCarObjectId] = playerStatistics;
		}

		public static void PlayerDeath(int deadPlayer, int killer, List<int> assists)
		{
			BombMatchBI.PlayerStatistics playerStatistics = BombMatchBI._players[deadPlayer];
			playerStatistics.Deaths++;
			bool flag = false;
			if (BombMatchBI._players.TryGetValue(killer, out playerStatistics))
			{
				playerStatistics = BombMatchBI._players[killer];
				playerStatistics.Kills++;
				flag = true;
			}
			if (flag && assists != null)
			{
				for (int i = 0; i < assists.Count; i++)
				{
					playerStatistics = BombMatchBI._players[assists[i]];
					playerStatistics.Assists++;
				}
			}
			if (flag)
			{
				BombMatchBI.CurrentRound.Kills++;
				BombMatchBI.CurrentRound.Assists += ((assists != null) ? assists.Count : 0);
			}
			BombMatchBI.CurrentRound.Deaths++;
		}

		public static void ScrapContributed(int playerId, int scrapAmount, ScrapIncomeKind kind)
		{
			BombMatchBI.PlayerStatistics playerStatistics = BombMatchBI._players[playerId];
			playerStatistics.ScrapsContribution += scrapAmount;
			playerStatistics.ScrapContributions.Add(scrapAmount);
			playerStatistics.ScrapContributionReasons.Add(kind);
		}

		public static void BombTaken(int playerId)
		{
			BombMatchBI.PlayerStatistics playerStatistics = BombMatchBI._players[playerId];
			playerStatistics.BombsPicked++;
		}

		public static void BombDeliverd(int playerId)
		{
			BombMatchBI.PlayerStatistics playerStatistics = BombMatchBI._players[playerId];
			playerStatistics.BombsDelivered++;
		}

		public static void BombDropped(int playerId, SpawnReason reason)
		{
			if (playerId == -1)
			{
				return;
			}
			BombMatchBI.PlayerStatistics playerStatistics = BombMatchBI._players[playerId];
			if (reason != SpawnReason.Death)
			{
				if (reason != SpawnReason.TriggerDrop)
				{
					if (reason == SpawnReason.InputDrop)
					{
						playerStatistics.BombsDroppedByGadget++;
						BombMatchBI.CurrentRound.BombsDroppedByGadget++;
					}
				}
				else
				{
					playerStatistics.BombsDroppedByYellow++;
					BombMatchBI.CurrentRound.BombsDroppedByYellow++;
				}
			}
			else
			{
				playerStatistics.BombsDroppedByDeath++;
				BombMatchBI.CurrentRound.BombsDroppedByDeath++;
			}
		}

		public static void CreepKilled(int playerId)
		{
			BombMatchBI.PlayerStatistics playerStatistics = BombMatchBI._players[playerId];
			playerStatistics.CreepsKilled++;
			BombMatchBI.CurrentRound.CreepsKilled++;
		}

		public static void PlayerUpgraded(int playerId, string upgradeName, GarageController.UpgradeOperationKind operation)
		{
			BombMatchBI.PlayerStatistics playerStatistics = BombMatchBI._players[playerId];
			playerStatistics.UpgradeOperations.Add(operation);
			playerStatistics.Upgrades.Add(upgradeName);
		}

		public static void RoundOver(float matchTime, int scrapRed, int scrapBlu)
		{
			BombMatchBI.OverallStatistics currentRound = BombMatchBI.CurrentRound;
			BombMatchBI._currentRound = null;
			currentRound.ScrapsCollectedRed = scrapRed;
			currentRound.ScrapsCollectedBlu = scrapBlu;
			currentRound.TimeSeconds = matchTime;
			currentRound.BombsDelivered = 1;
			BombMatchBI._rounds.Add(currentRound);
		}

		public static void MatchOver()
		{
			BombMatchBI.OverallStatistics red;
			BombMatchBI.OverallStatistics blu;
			BombMatchBI.OverallStatistics total;
			List<BombMatchBI.PlayerStatistics> reds;
			List<BombMatchBI.PlayerStatistics> blus;
			BombMatchBI.BuildTeams(out red, out blu, out total, out reds, out blus);
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			stringBuilder.AppendLine(BombMatchBI.Template[num++]);
			BombMatchBI.AppendLineRounds(num++, stringBuilder, (BombMatchBI.OverallStatistics x) => ((int)x.TimeSeconds).ToString(), total, red, blu);
			BombMatchBI.AppendLineRounds(num++, stringBuilder, (BombMatchBI.OverallStatistics x) => x.Kills.ToString(), total, red, blu);
			BombMatchBI.AppendLineRounds(num++, stringBuilder, (BombMatchBI.OverallStatistics x) => x.Assists.ToString(), total, red, blu);
			BombMatchBI.AppendLineRounds(num++, stringBuilder, (BombMatchBI.OverallStatistics x) => x.Deaths.ToString(), total, red, blu);
			BombMatchBI.AppendLineRounds(num++, stringBuilder, (BombMatchBI.OverallStatistics x) => x.BombsDelivered.ToString(), total, red, blu);
			BombMatchBI.AppendLineRounds(num++, stringBuilder, (BombMatchBI.OverallStatistics x) => x.BombsDroppedByGadget.ToString(), total, red, blu);
			BombMatchBI.AppendLineRounds(num++, stringBuilder, (BombMatchBI.OverallStatistics x) => x.BombsDroppedByYellow.ToString(), total, red, blu);
			BombMatchBI.AppendLineRounds(num++, stringBuilder, (BombMatchBI.OverallStatistics x) => x.BombsDroppedByDeath.ToString(), total, red, blu);
			BombMatchBI.AppendLineRounds(num++, stringBuilder, (BombMatchBI.OverallStatistics x) => x.CreepsKilled.ToString(), total, red, blu);
			BombMatchBI.AppendLineRounds(num++, stringBuilder, (BombMatchBI.OverallStatistics x) => x.ScrapsCollectedBlu.ToString(), total, red, blu);
			BombMatchBI.AppendLineRounds(num++, stringBuilder, (BombMatchBI.OverallStatistics x) => x.ScrapsCollectedRed.ToString(), total, red, blu);
			stringBuilder.AppendLine(BombMatchBI.Template[num++]);
			stringBuilder.AppendLine(BombMatchBI.Template[num++]);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.Name, reds, blus);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.Character, reds, blus);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.Kills.ToString(), reds, blus);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.Assists.ToString(), reds, blus);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.Deaths.ToString(), reds, blus);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.BombsPicked.ToString(), reds, blus);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.BombsDelivered.ToString(), reds, blus);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.BombsDroppedByGadget.ToString(), reds, blus);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.BombsDroppedByYellow.ToString(), reds, blus);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.BombsDroppedByDeath.ToString(), reds, blus);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.CreepsKilled.ToString(), reds, blus);
			BombMatchBI.AppendLinePlayers(num++, stringBuilder, (BombMatchBI.PlayerStatistics x) => x.ScrapsContribution.ToString(), reds, blus);
			BombMatchBI.AppendLineUpgradeCount(num++, stringBuilder, GarageController.UpgradeOperationKind.Upgrade, reds, blus);
			BombMatchBI.AppendLineUpgradeCount(num++, stringBuilder, GarageController.UpgradeOperationKind.Return, reds, blus);
			BombMatchBI.AppendLineUpgradeCount(num++, stringBuilder, GarageController.UpgradeOperationKind.Sell, reds, blus);
			BombMatchBI.AppendUpgradeHistory(num++, stringBuilder, reds, blus);
			BombMatchBI.AppendScrapContributionHistory(num++, stringBuilder, reds, blus);
			string contents = stringBuilder.ToString();
			BombMatchBI.WriteToFile(contents);
		}

		private static void AppendLineRounds(int line, StringBuilder builder, BombMatchBI.GetOverallParameter call, BombMatchBI.OverallStatistics total, BombMatchBI.OverallStatistics red, BombMatchBI.OverallStatistics blu)
		{
			builder.Append(BombMatchBI.Template[line]);
			builder.Append(';');
			builder.Append(call(total));
			for (int i = 0; i < BombMatchBI._rounds.Count; i++)
			{
				builder.Append(';');
				builder.Append(call(BombMatchBI._rounds[i]));
			}
			for (int j = 0; j < 5 - BombMatchBI._rounds.Count; j++)
			{
				builder.Append(';');
			}
			builder.Append(';');
			builder.Append(call(blu));
			builder.Append(';');
			builder.Append(call(red));
			builder.AppendLine();
		}

		private static void AppendLinePlayers(int line, StringBuilder builder, BombMatchBI.GetPlayerParameter call, List<BombMatchBI.PlayerStatistics> reds, List<BombMatchBI.PlayerStatistics> blus)
		{
			builder.Append(BombMatchBI.Template[line]);
			for (int i = 0; i < blus.Count; i++)
			{
				builder.Append(';');
				builder.Append(call(blus[i]));
			}
			for (int j = 0; j < 4 - blus.Count; j++)
			{
				builder.Append(';');
			}
			for (int k = 0; k < reds.Count; k++)
			{
				builder.Append(';');
				builder.Append(call(reds[k]));
			}
			builder.AppendLine();
		}

		private static void AppendLineUpgradeCount(int line, StringBuilder builder, GarageController.UpgradeOperationKind kind, List<BombMatchBI.PlayerStatistics> reds, List<BombMatchBI.PlayerStatistics> blus)
		{
			builder.Append(BombMatchBI.Template[line]);
			for (int i = 0; i < blus.Count; i++)
			{
				builder.Append(';');
				int count = blus[i].UpgradeOperations.FindAll((GarageController.UpgradeOperationKind x) => x == kind).Count;
				builder.Append(count);
			}
			for (int j = 0; j < 4 - blus.Count; j++)
			{
				builder.Append(';');
			}
			for (int k = 0; k < reds.Count; k++)
			{
				builder.Append(';');
				int count2 = reds[k].UpgradeOperations.FindAll((GarageController.UpgradeOperationKind x) => x == kind).Count;
				builder.Append(count2);
			}
			builder.AppendLine();
		}

		private static int AppendUpgradeHistory(int line, StringBuilder builder, List<BombMatchBI.PlayerStatistics> reds, List<BombMatchBI.PlayerStatistics> blus)
		{
			int num = 0;
			for (int i = 0; i < blus.Count; i++)
			{
				BombMatchBI.PlayerStatistics playerStatistics = blus[i];
				int count = playerStatistics.UpgradeOperations.Count;
				if (count > num)
				{
					num = count;
				}
			}
			for (int j = 0; j < reds.Count; j++)
			{
				BombMatchBI.PlayerStatistics playerStatistics2 = reds[j];
				int count2 = playerStatistics2.UpgradeOperations.Count;
				if (count2 > num)
				{
					num = count2;
				}
			}
			for (int k = 0; k < num; k++)
			{
				builder.Append(BombMatchBI.Template[line]);
				for (int l = 0; l < blus.Count; l++)
				{
					builder.Append(';');
					if (blus[l].UpgradeOperations.Count > k)
					{
						builder.Append(blus[l].UpgradeOperations[k].Prefix());
						builder.Append('-');
						builder.Append(blus[l].Upgrades[k]);
					}
				}
				for (int m = 0; m < 4 - blus.Count; m++)
				{
					builder.Append(';');
				}
				for (int n = 0; n < reds.Count; n++)
				{
					builder.Append(';');
					if (reds[n].UpgradeOperations.Count > k)
					{
						builder.Append(reds[n].UpgradeOperations[k].Prefix());
						builder.Append('-');
						builder.Append(reds[n].Upgrades[k]);
					}
				}
				builder.AppendLine();
			}
			return num;
		}

		private static int AppendScrapContributionHistory(int line, StringBuilder builder, List<BombMatchBI.PlayerStatistics> reds, List<BombMatchBI.PlayerStatistics> blus)
		{
			int num = 0;
			for (int i = 0; i < blus.Count; i++)
			{
				BombMatchBI.PlayerStatistics playerStatistics = blus[i];
				int count = playerStatistics.ScrapContributions.Count;
				if (count > num)
				{
					num = count;
				}
			}
			for (int j = 0; j < reds.Count; j++)
			{
				BombMatchBI.PlayerStatistics playerStatistics2 = reds[j];
				int count2 = playerStatistics2.ScrapContributions.Count;
				if (count2 > num)
				{
					num = count2;
				}
			}
			for (int k = 0; k < num; k++)
			{
				builder.Append(BombMatchBI.Template[line]);
				for (int l = 0; l < blus.Count; l++)
				{
					builder.Append(';');
					if (blus[l].ScrapContributions.Count > k)
					{
						builder.Append(blus[l].ScrapContributionReasons[k].Prefix());
						builder.Append('-');
						builder.Append(blus[l].ScrapContributions[k]);
					}
				}
				for (int m = 0; m < 4 - blus.Count; m++)
				{
					builder.Append(';');
				}
				for (int n = 0; n < reds.Count; n++)
				{
					builder.Append(';');
					if (reds[n].ScrapContributions.Count > k)
					{
						builder.Append(reds[n].ScrapContributionReasons[k].Prefix());
						builder.Append('-');
						builder.Append(reds[n].ScrapContributions[k]);
					}
				}
				builder.AppendLine();
			}
			return num;
		}

		private static void BuildTeams(out BombMatchBI.OverallStatistics red, out BombMatchBI.OverallStatistics blu, out BombMatchBI.OverallStatistics total, out List<BombMatchBI.PlayerStatistics> reds, out List<BombMatchBI.PlayerStatistics> blus)
		{
			blu = new BombMatchBI.OverallStatistics();
			red = new BombMatchBI.OverallStatistics();
			total = new BombMatchBI.OverallStatistics();
			reds = new List<BombMatchBI.PlayerStatistics>();
			blus = new List<BombMatchBI.PlayerStatistics>();
			for (int i = 0; i < BombMatchBI._rounds.Count; i++)
			{
				BombMatchBI.OverallStatistics overallStatistics = BombMatchBI._rounds[i];
				total.TimeSeconds = overallStatistics.TimeSeconds;
				total.Kills += overallStatistics.Kills;
				total.Assists += overallStatistics.Assists;
				total.Deaths += overallStatistics.Deaths;
				total.BombsDelivered += overallStatistics.BombsDelivered;
				total.BombsDroppedByGadget += overallStatistics.BombsDroppedByGadget;
				total.BombsDroppedByYellow += overallStatistics.BombsDroppedByYellow;
				total.BombsDroppedByDeath += overallStatistics.BombsDroppedByDeath;
				total.CreepsKilled += overallStatistics.CreepsKilled;
				total.ScrapsCollectedRed = overallStatistics.ScrapsCollectedRed;
				total.ScrapsCollectedBlu = overallStatistics.ScrapsCollectedBlu;
			}
			for (int j = BombMatchBI._rounds.Count; j > 1; j--)
			{
				BombMatchBI.OverallStatistics overallStatistics2 = BombMatchBI._rounds[j - 1];
				BombMatchBI.OverallStatistics overallStatistics3 = BombMatchBI._rounds[j - 2];
				overallStatistics2.TimeSeconds -= overallStatistics3.TimeSeconds;
				overallStatistics2.ScrapsCollectedBlu -= overallStatistics3.ScrapsCollectedBlu;
				overallStatistics2.ScrapsCollectedRed -= overallStatistics3.ScrapsCollectedRed;
			}
			foreach (KeyValuePair<int, BombMatchBI.PlayerStatistics> keyValuePair in BombMatchBI._players)
			{
				BombMatchBI.PlayerStatistics value = keyValuePair.Value;
				TeamKind team = value.Team;
				BombMatchBI.OverallStatistics overallStatistics4;
				if (team != TeamKind.Red)
				{
					if (team != TeamKind.Blue)
					{
					}
					blus.Add(value);
					overallStatistics4 = blu;
				}
				else
				{
					reds.Add(value);
					overallStatistics4 = red;
				}
				overallStatistics4.Kills += value.Kills;
				overallStatistics4.Assists += value.Assists;
				overallStatistics4.Deaths += value.Deaths;
				overallStatistics4.BombsDelivered += value.BombsDelivered;
				overallStatistics4.BombsDroppedByDeath += value.BombsDroppedByDeath;
				overallStatistics4.BombsDroppedByGadget += value.BombsDroppedByGadget;
				overallStatistics4.BombsDroppedByYellow += value.BombsDroppedByYellow;
				overallStatistics4.CreepsKilled += value.CreepsKilled;
			}
			blu.ScrapsCollectedBlu = BombMatchBI.Hub.ScrapBank.BluScrap;
			red.ScrapsCollectedRed = BombMatchBI.Hub.ScrapBank.RedScrap;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BombMatchBI));

		private static readonly string[] Template = new string[]
		{
			";Geral / Total;Round 1;Round 2;Round 3;Round 4;Round 5;Time 1;Time 2",
			"Tempo",
			"Abates",
			"Assistencias",
			"Mortes",
			"Quantidade de Entregas",
			"Bombas Derrubadas no chao (F)",
			"Bombas Derrubadas no chao (amarelas)",
			"Bombas Derrubas por abate",
			"Creeps Abatidos",
			"$ Obtido Time 1",
			"$ Obtido Time 2",
			";Time 1;;;;Time 2;;;",
			";Jogador 1;Jogador 2;Jogador 3;Jogador 4;Jogador 5;Jogador 6;Jogador 7;Jogador 8",
			"Jogador",
			"Personagem",
			"Abates",
			"Assistencias",
			"Mortes",
			"Bombas Pegas (no chao)",
			"Bombas Entregues",
			"Bombas Derrubadas no chao (F)",
			"Bombas Derrubadas no chao (amarelas)",
			"Bombas Derrubadas por abate",
			"Creeps Abatidos",
			"$ Contribuido",
			"Quantidade de Upgrades Comprados",
			"Quantidade de Upgrades Devolvidos",
			"Quantidade de Upgrades Vendidos",
			"Historico de Upgrades",
			"Historico de $"
		};

		private const string FileNameRegex = "MatchStats-{0}.csv";

		private const string DateTimeFormat = "yyyyMMdd-HHmmss";

		private static Dictionary<int, BombMatchBI.PlayerStatistics> _players = new Dictionary<int, BombMatchBI.PlayerStatistics>();

		private static List<BombMatchBI.OverallStatistics> _rounds = new List<BombMatchBI.OverallStatistics>();

		private static BombMatchBI.OverallStatistics _currentRound;

		private class PlayerStatistics
		{
			public int ObjectId;

			public string Name;

			public string Character;

			public TeamKind Team;

			public int Kills;

			public int Assists;

			public int Deaths;

			public int BombsPicked;

			public int BombsDelivered;

			public int BombsDroppedByGadget;

			public int BombsDroppedByYellow;

			public int BombsDroppedByDeath;

			public int CreepsKilled;

			public int ScrapsContribution;

			public List<int> ScrapContributions = new List<int>();

			public List<ScrapIncomeKind> ScrapContributionReasons = new List<ScrapIncomeKind>();

			public List<string> Upgrades = new List<string>();

			public List<GarageController.UpgradeOperationKind> UpgradeOperations = new List<GarageController.UpgradeOperationKind>();
		}

		private class OverallStatistics
		{
			public float TimeSeconds;

			public int Kills;

			public int Assists;

			public int Deaths;

			public int BombsDelivered;

			public int BombsDroppedByGadget;

			public int BombsDroppedByYellow;

			public int BombsDroppedByDeath;

			public int CreepsKilled;

			public int ScrapsCollectedRed;

			public int ScrapsCollectedBlu;
		}

		private delegate string GetOverallParameter(BombMatchBI.OverallStatistics stats);

		private delegate string GetPlayerParameter(BombMatchBI.PlayerStatistics stats);
	}
}
