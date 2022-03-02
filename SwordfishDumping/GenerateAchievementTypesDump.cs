using System;
using System.Linq;
using ClientAPI.Objects;
using HeavyMetalMachines.Achievements;

namespace HeavyMetalMachines.SwordfishDumping
{
	public class GenerateAchievementTypesDump
	{
		public AchievementType[] Generate()
		{
			AchievementIdentification[] array = AchievementIdentifications.GetAll().ToArray<AchievementIdentification>();
			AchievementType[] array2 = new AchievementType[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				AchievementIdentification achievementIdentification = array[i];
				array2[i] = new AchievementType
				{
					Id = achievementIdentification.SwordfishAchievementTypeId,
					Name = achievementIdentification.Name,
					Ammount = achievementIdentification.Amount
				};
			}
			return array2;
		}
	}
}
