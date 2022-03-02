using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public class UnityCompetitiveQueueJoinView : MonoBehaviour, ICompetitiveQueueJoinView
	{
		public IButton JoinButton
		{
			get
			{
				return this._joinButton;
			}
		}

		public IActivatable UnjoinableIndicator
		{
			get
			{
				return this._unjoinableIndicator;
			}
		}

		public IAnimation ShowGroupAnimation
		{
			get
			{
				return this._showGroupAnimation;
			}
		}

		public IAnimation HideGroupAnimation
		{
			get
			{
				return this._hideGroupAnimation;
			}
		}

		public IActivatable ModeLockedGroup
		{
			get
			{
				return this._modeLockedGroup;
			}
		}

		public ILabel MatchesPlayedToUnlockLabel
		{
			get
			{
				return this._matchesPlayedToUnlockLabel;
			}
		}

		public ILabel TotalMatchesToUnlockLabel
		{
			get
			{
				return this._totalMatchesToUnlockLabel;
			}
		}

		public IActivatable WaitingNextQueuePeriodGroup
		{
			get
			{
				return this._waitingNextQueuePeriodGroup;
			}
		}

		public ILabel NextQueuePeriodOpenDateLabel
		{
			get
			{
				return this._nextQueuePeriodOpenDateLabel;
			}
		}

		public ILabel NextQueuePeriodOpenTimeLabel
		{
			get
			{
				return this._nextQueuePeriodOpenTimeLabel;
			}
		}

		public IActivatable UnjoinableQueueGroup
		{
			get
			{
				return this._unjoinableQueueGroup;
			}
		}

		public ILabel UnjoinableQueueMessageLabel
		{
			get
			{
				return this._unjoinableQueueMessageLabel;
			}
		}

		public IActivatable CalibrationGroup
		{
			get
			{
				return this._calibrationGroup;
			}
		}

		public ILabel MatchesPlayedToCalibrateLabel
		{
			get
			{
				return this._matchesPlayedToCalibrateLabel;
			}
		}

		public ILabel TotalMatchesToCalibrateLabel
		{
			get
			{
				return this._totalMatchesToCalibrateLabel;
			}
		}

		public IActivatable BanGroup
		{
			get
			{
				return this._banGroup;
			}
		}

		public ILabel BanTimerLabel
		{
			get
			{
				return this._banTimerLabel;
			}
		}

		public IActivatable RankedGroup
		{
			get
			{
				return this._rankedGroup;
			}
		}

		public ILabel ScoreLabel
		{
			get
			{
				return this._scoreLabel;
			}
		}

		public IDynamicImage DivisionImage
		{
			get
			{
				return this._divisionImage;
			}
		}

		public IActivatable ScoreWithTopPlacementGroup
		{
			get
			{
				return this._topPlacementGroup;
			}
		}

		public ILabel TopPlacementLabel
		{
			get
			{
				return this._topPlacementLabel;
			}
		}

		public ILabel ScoreWithTopPlacementLabel
		{
			get
			{
				return this._scoreWithTopPlacementLabel;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ICompetitiveQueueJoinView>(this, UnityCompetitiveQueueJoinView.ViewProviderContext);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICompetitiveQueueJoinView>(UnityCompetitiveQueueJoinView.ViewProviderContext);
		}

		public static string ViewProviderContext = "CompetitiveInfo";

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityButton _joinButton;

		[SerializeField]
		private GameObjectActivatable _unjoinableIndicator;

		[SerializeField]
		private UnityAnimation _showGroupAnimation;

		[SerializeField]
		private UnityAnimation _hideGroupAnimation;

		[SerializeField]
		private GameObjectActivatable _modeLockedGroup;

		[SerializeField]
		private UnityLabel _matchesPlayedToUnlockLabel;

		[SerializeField]
		private UnityLabel _totalMatchesToUnlockLabel;

		[SerializeField]
		private GameObjectActivatable _waitingNextQueuePeriodGroup;

		[SerializeField]
		private UnityLabel _nextQueuePeriodOpenDateLabel;

		[SerializeField]
		private UnityLabel _nextQueuePeriodOpenTimeLabel;

		[SerializeField]
		private GameObjectActivatable _unjoinableQueueGroup;

		[SerializeField]
		private UnityLabel _unjoinableQueueMessageLabel;

		[SerializeField]
		private GameObjectActivatable _calibrationGroup;

		[SerializeField]
		private UnityLabel _matchesPlayedToCalibrateLabel;

		[SerializeField]
		private UnityLabel _totalMatchesToCalibrateLabel;

		[SerializeField]
		private GameObjectActivatable _banGroup;

		[SerializeField]
		private UnityLabel _banTimerLabel;

		[SerializeField]
		private GameObjectActivatable _rankedGroup;

		[SerializeField]
		private UnityLabel _scoreLabel;

		[SerializeField]
		private UnityDynamicImage _divisionImage;

		[SerializeField]
		private GameObjectActivatable _topPlacementGroup;

		[SerializeField]
		private UnityLabel _topPlacementLabel;

		[SerializeField]
		private UnityLabel _scoreWithTopPlacementLabel;
	}
}
