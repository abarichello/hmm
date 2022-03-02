using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.UnityUI;
using HeavyMetalMachines.Utils;
using Hoplon.Input.UiNavigation;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Tournaments.Presenting.Informing
{
	public class TournamentInfoPresenterView : MonoBehaviour, ITournamentInfoPresenterView
	{
		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
			}
		}

		public ICanvasGroup MainCanvasGroup
		{
			get
			{
				return this._mainCanvasGroup;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IButton NavLeftButton
		{
			get
			{
				return this._navLeftButton;
			}
		}

		public IButton NavRightButton
		{
			get
			{
				return this._navRightButton;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public ILabel HowItWorksTitleLabel
		{
			get
			{
				return this._howItWorksTitleLabel;
			}
		}

		public ILabel HowItWorksRegionLabel
		{
			get
			{
				return this._howItWorksRegionLabel;
			}
		}

		public ILabel HowItWorksSeasonLabel
		{
			get
			{
				return this._howItWorksSeasonLabel;
			}
		}

		public ILabel RewardTitleLabel
		{
			get
			{
				return this._rewardTitleLabel;
			}
		}

		public ILabel RewardRegionLabel
		{
			get
			{
				return this._rewardRegionLabel;
			}
		}

		public ILabel RewardSeasonLabel
		{
			get
			{
				return this._rewardSeasonLabel;
			}
		}

		public ILabel RewardDescriptionLabel
		{
			get
			{
				return this._rewardDescriptionLabel;
			}
		}

		public IDynamicImage TournamentLogoDynamicImage
		{
			get
			{
				return this._tournamentLogoImage;
			}
		}

		public ILabel TournamentLogoNameLabel
		{
			get
			{
				return this._tournamentLogoNameLabel;
			}
		}

		public IActivatable TournamentLogoNameLabelActivatable
		{
			get
			{
				return this._tournamentLogoNameLabelActivatable;
			}
		}

		public void PlayPageAnimation(bool toLeft)
		{
			GUIUtils.PlayAnimation(this._animation, toLeft, 1f, "TournamentInfoTransitionAnimation");
		}

		public IObservable<Unit> PlayInAnimation()
		{
			return this.PlayWindowAnimation("TournamentInfoInAnimation");
		}

		public IObservable<Unit> PlayOutAnimation()
		{
			return this.PlayWindowAnimation("TournamentInfoOutAnimation");
		}

		public void SetBackButtonLabelText(string text)
		{
			this._genericBackButton.SetText(text);
		}

		private IObservable<Unit> PlayWindowAnimation(string clipName)
		{
			return Observable.Delay<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				Animation animation = this._animation;
				string clipName2 = clipName;
				GUIUtils.PlayAnimation(animation, false, 1f, clipName2);
			}), TimeSpan.FromSeconds((double)this._animation.GetClip(clipName).length));
		}

		private void Start()
		{
			this._viewProvider.Bind<ITournamentInfoPresenterView>(this, null);
			this._genericBackButton.DisableAutomaticTranslation();
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ITournamentInfoPresenterView>(null);
		}

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroup;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UnityButton _navLeftButton;

		[SerializeField]
		private UnityButton _navRightButton;

		[SerializeField]
		private Animation _animation;

		[SerializeField]
		private UnityUiGenericButton _genericBackButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UnityDynamicImage _tournamentLogoImage;

		[SerializeField]
		private UnityLabel _tournamentLogoNameLabel;

		[SerializeField]
		private GameObjectActivatable _tournamentLogoNameLabelActivatable;

		[Header("How it works screen")]
		[SerializeField]
		private UnityLabel _howItWorksTitleLabel;

		[SerializeField]
		private UnityLabel _howItWorksRegionLabel;

		[SerializeField]
		private UnityLabel _howItWorksSeasonLabel;

		[Header("Rewards screen")]
		[SerializeField]
		private UnityLabel _rewardTitleLabel;

		[SerializeField]
		private UnityLabel _rewardRegionLabel;

		[SerializeField]
		private UnityLabel _rewardSeasonLabel;

		[SerializeField]
		private UnityLabel _rewardDescriptionLabel;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
