using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	[Serializable]
	public class UnityRankBadgeComponents : IRankBadgeComponents
	{
		public IActivatable Group
		{
			get
			{
				return this._group;
			}
		}

		public IAnimation BadgeAnimation
		{
			get
			{
				return this._badgeAnimation;
			}
		}

		public IDynamicImage SubleagueDynamicImage
		{
			get
			{
				return this._subleagueDynamicImage;
			}
		}

		[SerializeField]
		private GameObjectActivatable _group;

		[SerializeField]
		private UnityAnimation _badgeAnimation;

		[SerializeField]
		private UnityDynamicImage _subleagueDynamicImage;
	}
}
