using System;
using System.Collections;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassInfoView : MonoBehaviour, IBattlepassInfoView
	{
		protected void Start()
		{
			this._battlepassInfoComponent.RegisterInfoView(this);
		}

		public void SetVisibility(bool isVisible)
		{
			base.StartCoroutine(this.SetVisibilityCoroutine(isVisible));
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
			}
			else
			{
				this._mainWindowCanvasGroup.interactable = false;
				GUIUtils.PlayAnimation(this._mainWindowAnimation, false, 1f, "BattlePassInfoOutAnimation");
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

		public bool IsVisible()
		{
			return this._isVisible;
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
			this._prevPageButton.interactable = (this._currentPage == 1);
			this._nextPageButton.interactable = (this._currentPage == 0);
		}

		[UnityUiComponentCall]
		public void OnPreviousPageButtonClick()
		{
			this.ShowPage(0);
		}

		[UnityUiComponentCall]
		public void OnNextPageButtonClick()
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
		private Button _prevPageButton;

		[SerializeField]
		private Button _nextPageButton;

		private bool _isVisible;

		private int _currentPage;
	}
}
