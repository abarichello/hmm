using System;
using System.Collections.Generic;
using System.IO;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Characters
{
	public class CharacterBotNameStatic
	{
		public static List<string> GetRandomBotNames(int quantity, List<string> playerNames)
		{
			if (playerNames != null)
			{
				for (int i = 0; i < playerNames.Count; i++)
				{
					CharacterBotNameStatic.Log.DebugFormat("GetRandomBotNames - Player name[{0}]: {1}", new object[]
					{
						i,
						playerNames[i]
					});
				}
			}
			CharacterBotNameStatic._ignoredIndexesList.Clear();
			List<string> list = new List<string>(quantity);
			for (int j = 0; j < quantity; j++)
			{
				list.Add(CharacterBotNameStatic.GetRandomBotName(playerNames));
			}
			CharacterBotNameStatic.BotNamesList = null;
			return list;
		}

		private static string GetRandomBotName(List<string> playerNames)
		{
			int num;
			if (CharacterBotNameStatic.TryToGetValidRandomNameIndex(CharacterBotNameStatic.BotNamesList, CharacterBotNameStatic._ignoredIndexesList, out num, playerNames))
			{
				CharacterBotNameStatic._ignoredIndexesList.Add(num);
				CharacterBotNameStatic.Log.DebugFormat("GetRandomBotName - List[{0}]: {1}", new object[]
				{
					num,
					CharacterBotNameStatic.BotNamesList[num]
				});
				return CharacterBotNameStatic.BotNamesList[num];
			}
			string text = Guid.NewGuid().ToString().Substring(0, 5);
			CharacterBotNameStatic.Log.ErrorFormat("Random name index not found. Fake name: {0}", new object[]
			{
				text
			});
			for (int i = 0; i < CharacterBotNameStatic._ignoredIndexesList.Count; i++)
			{
				CharacterBotNameStatic.Log.ErrorFormat("GetRandomBotName error. IgnoredIndex:[{0}]", new object[]
				{
					CharacterBotNameStatic._ignoredIndexesList[i]
				});
			}
			return text;
		}

		private static List<string> LoadBotNames()
		{
			List<string> list = new List<string>();
			try
			{
				string fullPath = Path.Combine(Application.dataPath, Path.Combine("../", "botnames.txt"));
				list = FileUtil.ReadLines(fullPath);
			}
			catch (FileNotFoundException e)
			{
				CharacterBotNameStatic.Log.Fatal("Failed to load bot names file.", e);
			}
			catch (Exception e2)
			{
				CharacterBotNameStatic.Log.Fatal("Failed to parse bot names file.", e2);
			}
			CharacterBotNameStatic.Log.DebugFormat("BotNames list count: {0}", new object[]
			{
				list.Count
			});
			return list;
		}

		private static bool TryToGetValidRandomNameIndex(List<string> botNamesList, List<int> ignoreIndexes, out int index, List<string> playerNames = null)
		{
			index = -1;
			if (botNamesList.Count == 0)
			{
				CharacterBotNameStatic.Log.Error("TryToGetValidRandomNameIndex. BotNames list is empty.");
				return false;
			}
			for (int i = 0; i < ignoreIndexes.Count; i++)
			{
				if (ignoreIndexes[i] >= botNamesList.Count)
				{
					CharacterBotNameStatic.Log.ErrorFormat("TryToGetValidRandomNameIndex. Invalid index to ignore: [{0}]. BotNamesList count:[{1}]", new object[]
					{
						ignoreIndexes[i],
						botNamesList.Count
					});
					return false;
				}
			}
			if (ignoreIndexes.Count >= botNamesList.Count)
			{
				CharacterBotNameStatic.Log.Error("TryToGetValidRandomNameIndex. Invalid ignoreIndexes list: All bot names are ignored.");
				return false;
			}
			bool flag = false;
			if (playerNames != null)
			{
				if (playerNames.Count + ignoreIndexes.Count < botNamesList.Count)
				{
					flag = true;
				}
				else
				{
					CharacterBotNameStatic.Log.DebugFormat("TryToGetValidRandomNameIndex. Not enough bot names to evaluate player names. PlayerNames[{0}], IgnoreIndexes[{1}], BotNames[{2}]", new object[]
					{
						playerNames.Count,
						ignoreIndexes.Count,
						botNamesList.Count
					});
				}
			}
			while (index == -1)
			{
				int num = SysRandom.Int(0, botNamesList.Count);
				bool flag2 = false;
				for (int j = 0; j < ignoreIndexes.Count; j++)
				{
					if (num == ignoreIndexes[j])
					{
						CharacterBotNameStatic.Log.DebugFormat("TryToGetValidRandomNameIndex. Index is ignored: [{0}]", new object[]
						{
							num
						});
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					if (flag)
					{
						for (int k = 0; k < playerNames.Count; k++)
						{
							if (botNamesList[num] == playerNames[k])
							{
								CharacterBotNameStatic.Log.DebugFormat("TryToGetValidRandomNameIndex. Player name conflict: {0}. Index is ignored: [{1}]", new object[]
								{
									botNamesList[num],
									num
								});
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							continue;
						}
					}
					CharacterBotNameStatic.Log.DebugFormat("TryToGetValidRandomNameIndex. Valid index found: [{0}]", new object[]
					{
						num
					});
					index = num;
					break;
				}
			}
			return true;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CharacterBotNameStatic));

		private const string botNamesFile = "botnames.txt";

		private static List<string> BotNamesList = CharacterBotNameStatic.LoadBotNames();

		private static List<int> _ignoredIndexesList = new List<int>(CharacterBotNameStatic.BotNamesList.Count);
	}
}
