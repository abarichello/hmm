using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public static class SwordfishConnectionArgsParser
	{
		public static void ParseMatchArgs(string[] args, out string[] redTeam, out string[] bluTeam, out string[] spectators, ref long jobId, ref Guid serverMatchId, ref bool serverConfiguredBySwordfish, ref string serverIp, ref int serverPort, ref MatchData.MatchKind matchKind, ref int arenaIndex)
		{
			redTeam = null;
			bluTeam = null;
			spectators = null;
			foreach (string text in args)
			{
				if (SwordfishConnectionArgsParser.IsValidConfigArg(text))
				{
					string[] array = text.Split(new char[]
					{
						'='
					});
					string text2 = array[0];
					string text3 = array[1];
					switch (text2)
					{
					case "--jobid":
						jobId = long.Parse(text3);
						goto IL_1D0;
					case "--matchid":
						serverMatchId = new Guid(text3);
						goto IL_1D0;
					case "--ip":
						serverIp = text3;
						goto IL_1D0;
					case "--port":
						serverPort = int.Parse(text3);
						goto IL_1D0;
					case "--queuename":
					{
						string queueName = text3;
						SwordfishConnectionArgsParser.ParseQueueName(queueName, ref matchKind);
						goto IL_1D0;
					}
					case "--TutorialPlayer":
					{
						string text4 = text3;
						redTeam = new string[]
						{
							text4
						};
						goto IL_1D0;
					}
					case "--config":
						SwordfishConnectionArgsParser.ParseConfig(text, ref arenaIndex, ref serverConfiguredBySwordfish, ref redTeam, ref bluTeam);
						goto IL_1D0;
					case "--team1":
					case "--team2":
						SwordfishConnectionArgsParser.ParseTeamData(text2, text3, ref redTeam, ref bluTeam);
						goto IL_1D0;
					case "--spectators":
						SwordfishConnectionArgsParser.ParseSpectators(text3, ref spectators);
						goto IL_1D0;
					}
					SwordfishConnectionArgsParser.Log.WarnFormat("Unknow configuration found: {0}", new object[]
					{
						text2
					});
				}
				IL_1D0:;
			}
		}

		private static void ParseQueueName(string queueName, ref MatchData.MatchKind matchKind)
		{
			MatchData.MatchKind matchKind2 = MatchData.MatchKind.PvP;
			if (SwordfishConnectionArgsParser.TryGetMatchKindFromQueueName(queueName, ref matchKind2))
			{
				matchKind = matchKind2;
			}
		}

		private static void ParseConfig(string arg, ref int arenaIndex, ref bool serverConfiguredBySwordfish, ref string[] redTeam, ref string[] bluTeam)
		{
			string text = arg.Replace("--config=", string.Empty);
			string[] array = text.Split(new char[]
			{
				'=',
				':'
			});
			for (int i = 0; i < array.Length; i += 2)
			{
				if (array[i].Contains("ArenaIndex"))
				{
					SwordfishConnectionArgsParser.ParseArenaIndex(array[i + 1], ref arenaIndex, out serverConfiguredBySwordfish);
				}
				if (array[i].Contains("CustomWithBotsPlayerID"))
				{
					SwordfishConnectionArgsParser.ParseCustomWithBotsPlayerId(array[i + 1], ref redTeam, ref bluTeam);
				}
			}
		}

		private static void ParseArenaIndex(string arenaIndexText, ref int arenaIndex, out bool serverConfiguredBySwordfish)
		{
			serverConfiguredBySwordfish = true;
			int num;
			if (!int.TryParse(arenaIndexText, out num))
			{
				SwordfishConnectionArgsParser.Log.WarnFormat("Failed to parse ArenaIndex={0}", new object[]
				{
					arenaIndexText
				});
				return;
			}
			arenaIndex = num;
		}

		private static void ParseCustomWithBotsPlayerId(string universalId, ref string[] redTeam, ref string[] bluTeam)
		{
			redTeam = new string[]
			{
				universalId,
				"-1",
				"-1",
				"-1"
			};
			bluTeam = new string[]
			{
				"-1",
				"-1",
				"-1",
				"-1"
			};
		}

		private static void ParseTeamData(string configTag, string configArg, ref string[] redTeam, ref string[] bluTeam)
		{
			string[] array = configTag.Split(new char[]
			{
				'='
			});
			bool flag = array[0].EndsWith("1");
			string[] array2 = configArg.Split(new char[]
			{
				'|'
			});
			List<string> list = new List<string>();
			foreach (string text in array2)
			{
				if (!string.IsNullOrEmpty(text))
				{
					string item = text;
					list.Add(item);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
			}
			if (flag)
			{
				redTeam = list.ToArray();
			}
			else
			{
				bluTeam = list.ToArray();
			}
		}

		private static void ParseSpectators(string configArg, ref string[] spectators)
		{
			string[] array = configArg.Split(new char[]
			{
				'|'
			});
			List<string> list = new List<string>();
			foreach (string text in array)
			{
				if (!string.IsNullOrEmpty(text))
				{
					string item = text;
					list.Add(item);
				}
			}
			spectators = list.ToArray();
			for (int j = 0; j < list.Count; j++)
			{
			}
		}

		private static bool IsValidConfigArg(string arg)
		{
			if (string.IsNullOrEmpty(arg))
			{
				return false;
			}
			if (!arg.StartsWith("--"))
			{
				return false;
			}
			if (arg.Split(new char[]
			{
				'='
			}).Length < 2)
			{
				SwordfishConnectionArgsParser.Log.WarnFormat("Invalid configuration entry found: {0}. Configuration argument is missing!", new object[]
				{
					arg
				});
				return false;
			}
			return true;
		}

		public static bool TryGetMatchKindFromQueueName(string name, ref MatchData.MatchKind matchKind)
		{
			string text = name.ToLower();
			if (text != null)
			{
				if (text == "normal")
				{
					matchKind = MatchData.MatchKind.PvP;
					return true;
				}
				if (text == "coopvsbots")
				{
					matchKind = MatchData.MatchKind.PvE;
					return true;
				}
				if (text == "custom")
				{
					matchKind = MatchData.MatchKind.Custom;
					return true;
				}
				if (text == "playnow.8682b631-5e72-48df-9d50-ef69120aa14e")
				{
					matchKind = MatchData.MatchKind.Tutorial;
					return true;
				}
				if (text == "ranked")
				{
					matchKind = MatchData.MatchKind.Ranked;
					return true;
				}
			}
			return false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SwordfishConnectionArgsParser));

		private const char configTagSeparator = '=';
	}
}
