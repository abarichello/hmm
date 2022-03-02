using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Divisions
{
	[Serializable]
	public class UnityCompetitiveSubdivisionView : ICompetitiveSubdivisionView
	{
		public ILabel NameLabel
		{
			get
			{
				return this._nameLabel;
			}
		}

		public ILabel RangeLabel
		{
			get
			{
				return this._rangeLabel;
			}
		}

		public IImage ArrowImage
		{
			get
			{
				return this._arrowImage;
			}
		}

		public IDynamicImage IconImage
		{
			get
			{
				return this._iconImage;
			}
		}

		[SerializeField]
		private UnityLabel _nameLabel;

		[SerializeField]
		private UnityLabel _rangeLabel;

		[SerializeField]
		private UnityDynamicImage _iconImage;

		[SerializeField]
		private UnityImage _arrowImage;
	}
}
