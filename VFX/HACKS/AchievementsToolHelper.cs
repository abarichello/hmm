using System;
using System.Collections;
using System.Text;
using HeavyMetalMachines.Achievements;
using HeavyMetalMachines.Achievements.DataTransferObject;

namespace HeavyMetalMachines.VFX.HACKS
{
	public static class AchievementsToolHelper
	{
		public static bool TryToGetPlayerAchievement(IGetAchievement getAchievement, string achievementNameOrEnumIndex, out PlayerAchievement playerAchievement)
		{
			AchievementObjective achievementObjective;
			if (!AchievementsToolHelper.TryToGetAchievementObjective(out achievementObjective, achievementNameOrEnumIndex))
			{
				AchievementsToolHelper.ShowAllAchievementsError();
				playerAchievement = null;
				return false;
			}
			playerAchievement = getAchievement.Get(achievementObjective);
			if (playerAchievement == null)
			{
				AchievementsToolHelper.ShowAchievementNotFoundError(achievementObjective);
				return false;
			}
			return true;
		}

		private static void ShowAllAchievementsError()
		{
			Array values = Enum.GetValues(typeof(AchievementObjective));
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine();
			IEnumerator enumerator = values.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					stringBuilder.AppendLine();
					stringBuilder.Append((int)obj + " - " + obj);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			Platform.Current.ErrorMessageBox("Invalid achievement name or index. Valid values are:" + stringBuilder, "Hack Failed");
		}

		private static void ShowAchievementNotFoundError(AchievementObjective achievementObjective)
		{
			Platform.Current.ErrorMessageBox("Achievement not found: " + achievementObjective, "Hack Failed");
		}

		private static bool TryToGetAchievementObjective(out AchievementObjective achievementObjective, string achievementNameOrEnumIndex)
		{
			bool result;
			try
			{
				int num;
				if (int.TryParse(achievementNameOrEnumIndex, out num) && !Enum.IsDefined(typeof(AchievementObjective), num))
				{
					achievementObjective = 0;
					result = false;
				}
				else
				{
					achievementObjective = (AchievementObjective)Enum.Parse(typeof(AchievementObjective), achievementNameOrEnumIndex, true);
					result = true;
				}
			}
			catch (Exception ex)
			{
				achievementObjective = 0;
				result = false;
			}
			return result;
		}
	}
}
