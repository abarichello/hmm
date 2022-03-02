using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public class UnityCompetitiveQueuePeriodsView : MonoBehaviour, ICompetitiveQueuePeriodsView
	{
		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
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

		public IEnumerable<ICompetitiveQueueDayView> QueueDays
		{
			get
			{
				return this._dayViews;
			}
		}

		public IDropdown<string> RegionDropdown
		{
			get
			{
				return this._regionDropdown;
			}
		}

		public IAnimation ValuesChangeInAnimation
		{
			get
			{
				return this._valuesChangeInAnimation;
			}
		}

		public IAnimation ValuesChangeOutAnimation
		{
			get
			{
				return this._valuesChangeOutAnimation;
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
			this._viewProvider.Bind<ICompetitiveQueuePeriodsView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICompetitiveQueuePeriodsView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityCompetitiveQueueDayView[] _dayViews;

		[SerializeField]
		private StringUnityDropdown _regionDropdown;

		[SerializeField]
		private UnityAnimation _valuesChangeInAnimation;

		[SerializeField]
		private UnityAnimation _valuesChangeOutAnimation;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
