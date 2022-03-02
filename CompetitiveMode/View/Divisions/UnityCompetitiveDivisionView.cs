using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Divisions
{
	[Serializable]
	public class UnityCompetitiveDivisionView : ICompetitiveDivisionView
	{
		public IToggle Toggle
		{
			get
			{
				return this._toggle;
			}
		}

		public ILabel ScoreRangesLabel
		{
			get
			{
				return this._scoreRangesLabel;
			}
		}

		public IActivatable PlayerDivisionIndicator
		{
			get
			{
				return this._playerDivisionIndicator;
			}
		}

		public IAnimation Animation
		{
			get
			{
				return this._animation;
			}
		}

		[SerializeField]
		private UnityToggle _toggle;

		[SerializeField]
		private UnityLabel _scoreRangesLabel;

		[SerializeField]
		private GameObjectActivatable _playerDivisionIndicator;

		[SerializeField]
		private UnityAnimation _animation;
	}
}
