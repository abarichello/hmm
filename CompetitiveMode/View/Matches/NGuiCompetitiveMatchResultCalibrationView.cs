using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class NGuiCompetitiveMatchResultCalibrationView : MonoBehaviour, ICompetitiveMatchResultCalibrationView
	{
		public IActivatable Group
		{
			get
			{
				return this._group;
			}
		}

		public IButton ContinueButton
		{
			get
			{
				return this._continueButton;
			}
		}

		public ILabel SeasonNameLabel
		{
			get
			{
				return this._seasonNameLabel;
			}
		}

		public ILabel MatchesPlayedLabel
		{
			get
			{
				return this._matchesPlayedLabel;
			}
		}

		public ILabel TotalMatchesLabel
		{
			get
			{
				return this._totalMatchesPlayed;
			}
		}

		public IAnimation ShowAnimation
		{
			get
			{
				return this._showAnimation;
			}
		}

		public IAnimation HideAnimation
		{
			get
			{
				return this._hideAnimation;
			}
		}

		public IAnimation TransitionAnimation
		{
			get
			{
				return this._transitionAnimation;
			}
		}

		public IEnumerable<ICompetitiveMatchResultCalibrationMatchView> MatchesViews
		{
			get
			{
				return this._matchesViews;
			}
		}

		public int MillisecondsBetweenAnimationsOfMatches
		{
			get
			{
				return this._millisecondsBetweenAnimationsOfMatches;
			}
		}

		public IAnimation ShowButtonAnimation
		{
			get
			{
				return this._showButtonAnimation;
			}
		}

		public IActivatable LeavingCalibrationAnimationGroup
		{
			get
			{
				return this._leavingCalibrationAnimationGroup;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		[SerializeField]
		private GameObjectActivatable _group;

		[SerializeField]
		private NGuiButton _continueButton;

		[SerializeField]
		private NGuiLabel _seasonNameLabel;

		[SerializeField]
		private NGuiLabel _matchesPlayedLabel;

		[SerializeField]
		private NGuiLabel _totalMatchesPlayed;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityAnimation _transitionAnimation;

		[SerializeField]
		private UnityCompetitiveMatchResultCalibrationMatchView[] _matchesViews;

		[SerializeField]
		private int _millisecondsBetweenAnimationsOfMatches = 250;

		[SerializeField]
		private UnityAnimation _showButtonAnimation;

		[SerializeField]
		private GameObjectActivatable _leavingCalibrationAnimationGroup;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
