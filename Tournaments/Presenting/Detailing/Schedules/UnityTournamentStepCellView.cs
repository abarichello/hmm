using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing.Schedules
{
	public class UnityTournamentStepCellView : EnhancedScrollerCellView, ITournamentStepCellView
	{
		public int DataIndex { get; set; }

		public ILabel PositionLabel
		{
			get
			{
				return this._positionLabel;
			}
		}

		public ILabel TitleLabel
		{
			get
			{
				return this._titleLabel;
			}
		}

		public ILabel DateLabel
		{
			get
			{
				return this._dateLabel;
			}
		}

		public ILabel DayOfWeekLabel
		{
			get
			{
				return this._dayOfWeekLabel;
			}
		}

		public ILabel PeriodLabel
		{
			get
			{
				return this._periodLabel;
			}
		}

		public IObservable<int> ObserveRankingButtonClick()
		{
			return this.GetRankingButtonClickSubject();
		}

		public void SetAsOldStep()
		{
			this._bgRawImage.texture = this._oldStepTexture;
			this._selectRankingButton.interactable = true;
			this._disabledHighlightSelectable.interactable = false;
			this._disabledHighlightSelectable.gameObject.SetActive(false);
			this._infoCanvasGroup.alpha = this.ActiveInfoAlpha;
			this._closestStepIdleAnimationGameobject.SetActive(false);
		}

		public void SetAsClosestStep()
		{
			this._bgRawImage.texture = this._closestStepTexture;
			this._selectRankingButton.interactable = true;
			this._disabledHighlightSelectable.interactable = false;
			this._disabledHighlightSelectable.gameObject.SetActive(false);
			this._infoCanvasGroup.alpha = this.ActiveInfoAlpha;
			this._closestStepIdleAnimationGameobject.SetActive(true);
		}

		public void SetAsFutureStep()
		{
			this._bgRawImage.texture = this._futureStepTexture;
			this._selectRankingButton.interactable = false;
			this._disabledHighlightSelectable.interactable = true;
			this._disabledHighlightSelectable.gameObject.SetActive(true);
			this._infoCanvasGroup.alpha = this.InactiveInfoAlpha;
			this._closestStepIdleAnimationGameobject.SetActive(false);
		}

		private void Start()
		{
			ObservableExtensions.Subscribe<Unit>(UnityUIComponentExtensions.OnClickAsObservable(this._selectRankingButton), delegate(Unit _)
			{
				this.GetRankingButtonClickSubject().OnNext(this.DataIndex);
			});
		}

		public float GetSize()
		{
			return this._rectTransform.sizeDelta.y;
		}

		private void OnValidate()
		{
			this._rectTransform = base.GetComponent<RectTransform>();
		}

		private Subject<int> GetRankingButtonClickSubject()
		{
			if (this._rankingButtonClickSubject == null)
			{
				this._rankingButtonClickSubject = new Subject<int>();
			}
			return this._rankingButtonClickSubject;
		}

		[SerializeField]
		[Range(0f, 1f)]
		private float ActiveInfoAlpha = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float InactiveInfoAlpha = 0.5f;

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private UnityLabel _positionLabel;

		[SerializeField]
		private UnityLabel _titleLabel;

		[SerializeField]
		private UnityLabel _dateLabel;

		[SerializeField]
		private UnityLabel _dayOfWeekLabel;

		[SerializeField]
		private UnityLabel _periodLabel;

		[SerializeField]
		private Button _selectRankingButton;

		[SerializeField]
		private Selectable _disabledHighlightSelectable;

		[SerializeField]
		private RawImage _bgRawImage;

		[SerializeField]
		private CanvasGroup _infoCanvasGroup;

		[SerializeField]
		private GameObject _closestStepIdleAnimationGameobject;

		[SerializeField]
		private Texture _oldStepTexture;

		[SerializeField]
		private Texture _closestStepTexture;

		[SerializeField]
		private Texture _futureStepTexture;

		private Subject<int> _rankingButtonClickSubject;
	}
}
