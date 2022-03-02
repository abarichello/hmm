using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.UnityUI;
using Hoplon.Input.UiNavigation;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View
{
	public class UnityCompetitiveModeView : MonoBehaviour, ICompetitiveModeView
	{
		public ITitle Title
		{
			get
			{
				return this._title;
			}
		}

		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
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

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IButton OpenQueuePeriodsButton
		{
			get
			{
				return this._openQueuePeriodsButton;
			}
		}

		public IButton OpenDivisionsButton
		{
			get
			{
				return this._openDivisionsButton;
			}
		}

		public IButton OpenRewardsButton
		{
			get
			{
				return this._openRewardsButton;
			}
		}

		public IButton OpenRankingButton
		{
			get
			{
				return this._openRankingButton;
			}
		}

		public ILabel SeasonNameLabel
		{
			get
			{
				return this._seasonNameLabel;
			}
		}

		public ILabel SeasonStartDateLabel
		{
			get
			{
				return this._seasonStartDateLabel;
			}
		}

		public ILabel SeasonStartTimeLabel
		{
			get
			{
				return this._seasonStartTimeLabel;
			}
		}

		public ILabel SeasonEndDateLabel
		{
			get
			{
				return this._seasonEndDateLabel;
			}
		}

		public ILabel SeasonEndTimeLabel
		{
			get
			{
				return this._seasonEndTimeLabel;
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

		public ILabel InformationLabel
		{
			get
			{
				return this._informationLabel;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ICompetitiveModeView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICompetitiveModeView>(null);
		}

		[SerializeField]
		private UnityUiTitleInfo _title;

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UnityButton _openQueuePeriodsButton;

		[SerializeField]
		private UnityButton _openDivisionsButton;

		[SerializeField]
		private UnityButton _openRewardsButton;

		[SerializeField]
		private UnityButton _openRankingButton;

		[SerializeField]
		private UnityLabel _seasonNameLabel;

		[SerializeField]
		private UnityLabel _seasonStartDateLabel;

		[SerializeField]
		private UnityLabel _seasonStartTimeLabel;

		[SerializeField]
		private UnityLabel _seasonEndDateLabel;

		[SerializeField]
		private UnityLabel _seasonEndTimeLabel;

		[SerializeField]
		private UnityLabel _nextQueuePeriodOpenDateLabel;

		[SerializeField]
		private UnityLabel _nextQueuePeriodOpenTimeLabel;

		[SerializeField]
		private UnityLabel _informationLabel;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[Inject]
		[UsedImplicitly]
		private IViewProvider _viewProvider;
	}
}
