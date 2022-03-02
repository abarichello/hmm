using System;
using System.Collections;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Utils;
using Hoplon.Input.UiNavigation;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassInfoView : MonoBehaviour, IBattlepassInfoView
	{
		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IButton PrevPageButton
		{
			get
			{
				return this._prevPageButton;
			}
		}

		public IButton NextPageButton
		{
			get
			{
				return this._nextPageButton;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		private void OnDestroy()
		{
			if (this._previousPageButtonClickDisposable != null)
			{
				this._previousPageButtonClickDisposable.Dispose();
			}
			if (this._nextPageButtonClickDisposable != null)
			{
				this._nextPageButtonClickDisposable.Dispose();
			}
		}

		public void SetVisibility(bool isVisible)
		{
			base.StartCoroutine(this.SetVisibilityCoroutine(isVisible));
		}

		public bool IsVisible()
		{
			return this._isVisible;
		}

		protected void Start()
		{
			this._battlepassInfoComponent.RegisterInfoView(this);
			this._previousPageButtonClickDisposable = ObservableExtensions.Subscribe<Unit>(this._prevPageButton.OnClick(), delegate(Unit _)
			{
				this.OnPreviousPageButtonClick();
			});
			this._nextPageButtonClickDisposable = ObservableExtensions.Subscribe<Unit>(this._nextPageButton.OnClick(), delegate(Unit _)
			{
				this.OnNextPageButtonClick();
			});
			ObservableExtensions.Subscribe<Unit>(this.BackButton.OnClick(), delegate(Unit _)
			{
				this._battlepassInfoComponent.HideInfoWindow();
			});
			this._logoMetalPassGroup.SetImageName("event_logo_season_current_large");
			this._logoRewardGroup.SetImageName("event_logo_season_current_large");
		}

		private IEnumerator SetVisibilityCoroutine(bool isVisible)
		{
			if (isVisible)
			{
				this.RefreshNavigationButtons();
				this._mainWindowCanvas.enabled = true;
				this._mainWindowCanvasGroup.interactable = true;
				this._isVisible = true;
				GUIUtils.PlayAnimation(this._mainWindowAnimation, false, 1f, "BattlePassInfoInAnimation");
				this.UiNavigationGroupHolder.AddHighPriorityGroup();
			}
			else
			{
				this._mainWindowCanvasGroup.interactable = false;
				GUIUtils.PlayAnimation(this._mainWindowAnimation, false, 1f, "BattlePassInfoOutAnimation");
				this.UiNavigationGroupHolder.RemoveHighPriorityGroup();
			}
			yield return new WaitForSeconds(this._mainWindowAnimation.clip.length);
			if (!isVisible)
			{
				this._mainWindowCanvas.enabled = false;
				this._isVisible = false;
				this._battlepassInfoComponent.OnInfoWindowHideAnimationEnded();
			}
			yield break;
		}

		private void ShowPage(int pageIndex)
		{
			if (pageIndex == this._currentPage)
			{
				return;
			}
			GUIUtils.PlayAnimation(this._mainWindowAnimation, pageIndex == 0, 1f, "BattlePassInfoTransitionAnimation");
			this._currentPage = pageIndex;
			this.RefreshNavigationButtons();
		}

		private void RefreshNavigationButtons()
		{
			this.PrevPageButton.IsInteractable = (this._currentPage == 1);
			this.NextPageButton.IsInteractable = (this._currentPage == 0);
		}

		private void OnPreviousPageButtonClick()
		{
			this.ShowPage(0);
		}

		private void OnNextPageButtonClick()
		{
			this.ShowPage(1);
		}

		[UnityUiComponentCall]
		public void OnBackButtonClick()
		{
			this._battlepassInfoComponent.HideInfoWindow();
		}

		[SerializeField]
		private BattlepassInfoComponent _battlepassInfoComponent;

		[SerializeField]
		private Canvas _mainWindowCanvas;

		[SerializeField]
		private CanvasGroup _mainWindowCanvasGroup;

		[SerializeField]
		private Animation _mainWindowAnimation;

		[SerializeField]
		private UnityButton _prevPageButton;

		[SerializeField]
		private UnityButton _nextPageButton;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UnityDynamicImage _logoRewardGroup;

		[SerializeField]
		private UnityDynamicImage _logoMetalPassGroup;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		private const string LogoImageName = "event_logo_season_current_large";

		private bool _isVisible;

		private int _currentPage;

		private IDisposable _previousPageButtonClickDisposable;

		private IDisposable _nextPageButtonClickDisposable;
	}
}
