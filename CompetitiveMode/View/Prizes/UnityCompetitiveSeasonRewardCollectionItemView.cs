using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Prizes
{
	public class UnityCompetitiveSeasonRewardCollectionItemView : MonoBehaviour, ICompetitiveSeasonRewardCollectionItemView
	{
		public IDynamicImage ThumbnailImage
		{
			get
			{
				return this._thumbnailImage;
			}
		}

		public ILabel NameLabel
		{
			get
			{
				return this._nameTooltipLabel;
			}
		}

		[SerializeField]
		private UnityDynamicRawImage _thumbnailImage;

		[SerializeField]
		private UnityTooltipLabel _nameTooltipLabel;
	}
}
