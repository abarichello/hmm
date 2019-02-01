using System;
using System.Collections;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassDetailView : MonoBehaviour, IBattlepassDetailView
	{
		protected void Awake()
		{
			this._battlepassDetailComponent = this._battlepassDetailComponentAsset;
			this._mainWindowCanvasGroup.interactable = false;
			this.Setup(this._battlepassDetailComponent.RegisterDetailView(this));
			this.SetVisibility(true);
		}

		protected void OnDestroy()
		{
			base.StopAllCoroutines();
		}

		private void Setup(UnityUiBattlepassDetailView.BattlepassDetailViewData battlepassDetailViewData)
		{
			this._startDayText.text = battlepassDetailViewData.StartDay.ToString("0");
			this._startMonthText.text = string.Format("/{0}", Language.Get(battlepassDetailViewData.StartMonth, TranslationSheets.Battlepass));
			this._endDayText.text = battlepassDetailViewData.EndDay.ToString("0");
			this._endMonthText.text = string.Format("/{0}", Language.Get(battlepassDetailViewData.EndMonth, TranslationSheets.Battlepass));
		}

		public void SetVisibility(bool isVisible)
		{
			base.StartCoroutine(this.SetVisibilityCoroutine(isVisible, false));
		}

		public bool IsVisible()
		{
			return this._isVisible;
		}

		private IEnumerator SetVisibilityCoroutine(bool isVisible, bool showMetalpassWindow = false)
		{
			string animationName;
			if (isVisible)
			{
				this._mainWindowCanvas.enabled = true;
				this._mainWindowCanvasGroup.interactable = true;
				this._isVisible = true;
				animationName = "BattlePassWelcomeelInAnimation";
			}
			else
			{
				this._mainWindowCanvasGroup.interactable = false;
				animationName = "BattlepassGenericOutAnimation";
			}
			this._mainWindowAnimation.Play(animationName);
			yield return new WaitForSeconds(this._mainWindowAnimation.GetClip(animationName).length + Time.deltaTime);
			if (!isVisible)
			{
				this._battlepassDetailComponent.HideDetailWindow(showMetalpassWindow);
				this._mainWindowCanvas.enabled = false;
				this._isVisible = false;
			}
			yield break;
		}

		[UnityUiComponentCall]
		public void OnOkButtonClick()
		{
			base.StartCoroutine(this.SetVisibilityCoroutine(false, false));
		}

		[UnityUiComponentCall]
		public void OnMetalpassButtonClick()
		{
			base.StartCoroutine(this.SetVisibilityCoroutine(false, true));
		}

		[Header("[Infra]")]
		[SerializeField]
		private BattlepassDetailComponent _battlepassDetailComponentAsset;

		[Header("[Main UI Components]")]
		[SerializeField]
		private Canvas _mainWindowCanvas;

		[SerializeField]
		private CanvasGroup _mainWindowCanvasGroup;

		[SerializeField]
		private Animation _mainWindowAnimation;

		[Header("[UI Components]")]
		[SerializeField]
		private Text _startDayText;

		[SerializeField]
		private Text _startMonthText;

		[SerializeField]
		private Text _endDayText;

		[SerializeField]
		private Text _endMonthText;

		[Header("[Test Only]")]
		[SerializeField]
		private UnityUiBattlepassDetailView.BattlepassDetailViewData _battlepassDetailViewDataTest;

		private IBattlepassDetailComponent _battlepassDetailComponent;

		private bool _isVisible;

		private int _currentLevel;

		[Serializable]
		public struct BattlepassDetailViewData
		{
			public int StartDay;

			public string StartMonth;

			public int EndDay;

			public string EndMonth;
		}
	}
}
