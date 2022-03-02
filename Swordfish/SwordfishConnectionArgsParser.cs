using System;
using System.Collections.Generic;
using HeavyMetalMachines.Matches.DataTransferObjects;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public static class SwordfishConnectionArgsParser
	{
		public static string[] AutoCompleteTeamArrayWithBots(string[] parsedTeam, int maxPlayersInTeam)
		{
			string[] array = new string[maxPlayersInTeam];
			int i;
			for (i = 0; i < parsedTeam.Length; i++)
			{
				array[i] = parsedTeam[i];
			}
			while (i < array.Length)
			{
				array[i] = "-1";
				i++;
			}
			return array;
		}

		public static void ParseMatchArgs(string[] args, out string[] redTeam, out string[] bluTeam, out string[] spectators, ref long jobId, ref Guid serverMatchId, ref bool serverConfiguredBySwordfish, ref string serverIp, ref int serverPort, ref MatchKind matchKind, ref int arenaIndex, ref long tournamentStepId, ref string regionName, ref string queueName)
		{
			redTeam = new string[0];
			bluTeam = new string[0];
			spectators = new string[0];
			foreach (string text in args)
			{
				SwordfishConnectionArgsParser.Log.DebugFormat("Parsing argument={0}", new object[]
				{
					text
				});
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
						goto IL_249;
					case "--matchid":
						serverMatchId = new Guid(text3);
						goto IL_249;
					case "--ip":
						serverIp = text3;
						goto IL_249;
					case "--port":
						serverPort = int.Parse(text3);
						goto IL_249;
					case "--queueName":
						queueName = text3;
						goto IL_249;
					case "--arena":
						serverConfiguredBySwordfish = true;
						SwordfishConnectionArgsParser.ParseArenaIndex(text3, ref arenaIndex);
						goto IL_249;
					case "--config":
						SwordfishConnectionArgsParser.ParseConfig(text, ref redTeam, ref bluTeam, ref arenaIndex, ref matchKind);
						goto IL_249;
					case "--kind":
						SwordfishConnectionArgsParser.ParseMatchKindName(text3, ref matchKind);
						goto IL_249;
					case "--team1":
					case "--team2":
						SwordfishConnectionArgsParser.ParseTeamData(text2, text3, ref redTeam, ref bluTeam);
						goto IL_249;
					case "--spectators":
						SwordfishConnectionArgsParser.ParseSpectators(text3, ref spectators);
						goto IL_249;
					case "--tournamentStepId":
						tournamentStepId = long.Parse(text3);
						goto IL_249;
					case "--regionName":
						regionName = text3;
						goto IL_249;
					}
					SwordfishConnectionArgsParser.Log.WarnFormat("Unknow configuration found: {0}", new object[]
					{
						text2
					});
				}
				IL_249:;
			}
		}

		private static void ParseConfig(string arg, ref string[] redTeam, ref string[] bluTeam, ref int arenaIndex, ref MatchKind matchKind)
		{
			string[] array = arg.Split(new char[]
			{
				'='
			});
			if (array.Length == 2)
			{
				string[] array2 = array[1].Split(new char[]
				{
					',',
					':'
				});
				for (int i = 0; i < array2.Length; i += 2)
				{
					if (array2[i].Contains("MatchKind"))
					{
						SwordfishConnectionArgsParser.ParseMatchKindName(array2[i + 1], ref matchKind);
					}
					else if (array2[i].Contains("CustomWithBotsPlayerID"))
					{
						SwordfishConnectionArgsParser.ParseCustomWithBotsPlayerId(array2[i + 1], ref redTeam, ref bluTeam);
					}
					else if (array2[i].Contains("TutorialPlayer"))
					{
						string text = array2[i + 1];
						redTeam = new string[]
						{
							text
						};
						bluTeam = new string[0];
						SwordfishConnectionArgsParser.Log.DebugFormat("Tutorial Player. Team=Red Uid={0}", new object[]
						{
							text
						});
					}
					else if (array2[i].Contains("Config"))
					{
						SwordfishConnectionArgsParser.ParseArenaIndex(array2[i + 1], ref arenaIndex);
					}
				}
			}
		}

		private static void ParseMatchKindName(string matchKindName, ref MatchKind matchKind)
		{
			string[] names = Enum.GetNames(typeof(MatchKind));
			MatchKind[] array = (MatchKind[])Enum.GetValues(typeof(MatchKind));
			for (int i = 0; i < names.Length; i++)
			{
				if (string.CompareOrdinal(names[i], matchKindName) == 0)
				{
					matchKind = array[i];
					SwordfishConnectionArgsParser.Log.DebugFormat("MatchKind found: {0}. MatchKind set to: {1}", new object[]
					{
						matchKindName,
						matchKind
					});
					return;
				}
			}
			matchKind = 0;
			SwordfishConnectionArgsParser.Log.ErrorFormat("MatchKindName not found: {0}. MatchKind set to: {1}", new object[]
			{
				matchKindName,
				matchKind
			});
		}

		private static void ParseArenaIndex(string arenaIndexText, ref int arenaIndex)
		{
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
			SwordfishConnectionArgsParser.Log.DebugFormat("[CustomWithBots] Player. Team=Red Uid={0}", new object[]
			{
				universalId
			});
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
				SwordfishConnectionArgsParser.Log.DebugFormat("Team={0} Uid={1}", new object[]
				{
					(!flag) ? "Blu" : "Red",
					list[j]
				});
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
				SwordfishConnectionArgsParser.Log.DebugFormat("Spectator Uid={0}", new object[]
				{
					list[j]
				});
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

		public static readonly BitLogger Log = new BitLogger(typeof(SwordfishConnectionArgsParser));

		private const char configTagSeparator = '=';
	}
}
