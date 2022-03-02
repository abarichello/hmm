using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing.Schedules
{
	public class TournamentHomeView : MonoBehaviour, ITournamentHomeView
	{
		public ICanvasGroup MainCanvasGroup
		{
			get
			{
				return this._mainCanvasGroupUnity;
			}
		}

		public ILabel StartDateScheduleLabel
		{
			get
			{
				return this._startDateScheduleLabel;
			}
		}

		public ILabel EndDateScheduleLabel
		{
			get
			{
				return this._endDateScheduleLabel;
			}
		}

		public ILabel TitleLabel
		{
			get
			{
				return this._titleLabel;
			}
		}

		public ILabel TournamentSeasonLabel
		{
			get
			{
				return this._seasonIdLabel;
			}
		}

		public IScroller<TournamentStepCellData> StepsScroller
		{
			get
			{
				return this._stepsScroller;
			}
		}

		public IObservable<int> ObserveStepRankingSelection()
		{
			return this._stepsScroller.ObserveStepRankingSelection();
		}

		public IUiNavigationRebuilder UiNavigationRebuilder
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public void ShowFeedback(string message)
		{
			this._feedbackGameObject.SetActive(true);
			this._feedbackText.text = message;
		}

		public void HideFeedback()
		{
			this._feedbackGameObject.SetActive(false);
		}

		public void FillScheduleTimeProgressBar(float normalizedValue)
		{
			this._scheduleTimeScrollbar.size = normalizedValue;
		}

		private void Start()
		{
			this._viewProvider.Bind<ITournamentHomeView>(this, null);
			this._stepsScroller = new UnityTournamentStepScroller(this._homeStepScroller, this._homeStepCellViewPrefab);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ITournamentHomeView>(null);
		}

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroupUnity;

		[SerializeField]
		private EnhancedScroller _homeStepScroller;

		[SerializeField]
		private UnityTournamentStepCellView _homeStepCellViewPrefab;

		[SerializeField]
		private GameObject _feedbackGameObject;

		[SerializeField]
		private Text _feedbackText;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[SerializeField]
		private Scrollbar _scheduleTimeScrollbar;

		[SerializeField]
		private UnityLabel _startDateScheduleLabel;

		[SerializeField]
		private UnityLabel _endDateScheduleLabel;

		[SerializeField]
		private UnityLabel _titleLabel;

		[SerializeField]
		private UnityLabel _seasonIdLabel;

		private UnityTournamentStepScroller _stepsScroller;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
