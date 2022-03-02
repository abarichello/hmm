using System;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Social.Profile.Models;

namespace HeavyMetalMachines.Profile.Models
{
	[Serializable]
	public struct ProfileStatisticViewModel
	{
		public ProfileStatistic Statistic;

		public UnitySprite Icon;
	}
}
