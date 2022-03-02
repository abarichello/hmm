using System;
using Pocketverse;

namespace HeavyMetalMachines.CompetitiveMode.Infra.News
{
	public class SeasonNewsStorage : GameHubObject, ISeasonNewsStorage
	{
		public long GetCompetitive()
		{
			return SeasonNewsStorage.GetSeasonNews("LastCompetitiveSeasonIdNewsSeen");
		}

		public void SetCompetitive(long seasonId)
		{
			GameHubObject.Hub.PlayerPrefs.SetString("LastCompetitiveSeasonIdNewsSeen", seasonId.ToString());
			GameHubObject.Hub.PlayerPrefs.Save();
		}

		public long GetBattlepass()
		{
			return SeasonNewsStorage.GetSeasonNews("LastBattlepassSeasonIdNewsSeen");
		}

		public void SetBattlepass(long seasonId)
		{
			GameHubObject.Hub.PlayerPrefs.SetString("LastBattlepassSeasonIdNewsSeen", seasonId.ToString());
			GameHubObject.Hub.PlayerPrefs.Save();
		}

		private static long GetSeasonNews(string featureNews)
		{
			if (!GameHubObject.Hub.PlayerPrefs.HasKey(featureNews))
			{
				return -1L;
			}
			string @string = GameHubObject.Hub.PlayerPrefs.GetString(featureNews, null);
			long result;
			if (!long.TryParse(@string, out result))
			{
				result = -1L;
			}
			return result;
		}

		private const string LastCompetitiveSeasonIdNewsSeen = "LastCompetitiveSeasonIdNewsSeen";

		private const string LastBattlepassSeasonIdNewsSeen = "LastBattlepassSeasonIdNewsSeen";
	}
}
