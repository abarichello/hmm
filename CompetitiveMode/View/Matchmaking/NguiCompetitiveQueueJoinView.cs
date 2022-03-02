using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public class NguiCompetitiveQueueJoinView : MonoBehaviour, ICompetitiveQueueJoinView
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
			this._viewProvider.Bind<ICompetitiveQueueJoinView>(this, NguiCompetitiveQueueJoinView.ViewProviderContext);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICompetitiveQueueJoinView>(NguiCompetitiveQueueJoinView.ViewProviderContext);
		}

		public static readonly string ViewProviderContext = "GameModes";

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private NGuiButton _joinButton;

		[SerializeField]
		private GameObjectActivatable _unjoinableIndicator;

		[SerializeField]
		private UnityAnimation _showGroupAnimation;

		[SerializeField]
		private UnityAnimation _hideGroupAnimation;

		[SerializeField]
		private GameObjectActivatable _modeLockedGroup;

		[SerializeField]
		private NGuiLabel _matchesPlayedToUnlockLabel;

		[SerializeField]
		private NGuiLabel _totalMatchesToUnlockLabel;

		[SerializeField]
		private GameObjectActivatable _waitingNextQueuePeriodGroup;

		[SerializeField]
		private NGuiLabel _nextQueuePeriodOpenDateLabel;

		[SerializeField]
		private NGuiLabel _nextQueuePeriodOpenTimeLabel;

		[SerializeField]
		private GameObjectActivatable _unjoinableQueueGroup;

		[SerializeField]
		private NGuiLabel _unjoinableQueueMessageLabel;

		[SerializeField]
		private GameObjectActivatable _calibrationGroup;

		[SerializeField]
		private NGuiLabel _matchesPlayedToCalibrateLabel;

		[SerializeField]
		private NGuiLabel _totalMatchesToCalibrateLabel;

		[SerializeField]
		private GameObjectActivatable _banGroup;

		[SerializeField]
		private NGuiLabel _banTimerLabel;

		[SerializeField]
		private GameObjectActivatable _rankedGroup;

		[SerializeField]
		private NGuiLabel _scoreLabel;

		[SerializeField]
		private NGuiDynamicImage _divisionImage;

		[SerializeField]
		private GameObjectActivatable _topPlacementGroup;

		[SerializeField]
		private NGuiLabel _topPlacementLabel;

		[SerializeField]
		private NGuiLabel _scoreWithTopPlacementLabel;
	}
}
