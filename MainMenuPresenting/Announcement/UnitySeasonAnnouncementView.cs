using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.MainMenuPresenting.Announcement
{
	public class UnitySeasonAnnouncementView : MonoBehaviour, ISeasonAnnouncementView
	{
		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
			}
		}

		public IButton CloseButton
		{
			get
			{
				return this._closeButton;
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

		public IDynamicImage BackgroundImage
		{
			get
			{
				return this._backgroundImage;
			}
		}

		public string BattlepassBackgroundImageName
		{
			get
			{
				return this._battlepassBackgroundImageName;
			}
			set
			{
				this._battlepassBackgroundImageName = value;
			}
		}

		public string CompetitiveModeBackgroundImageName
		{
			get
			{
				return this._competitiveModeBackgroundImageName;
			}
			set
			{
				this._competitiveModeBackgroundImageName = value;
			}
		}

		public string MergedBackgroundImageName
		{
			get
			{
				return this._mergedBackgroundImageName;
			}
			set
			{
				this._mergedBackgroundImageName = value;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<ISeasonAnnouncementView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ISeasonAnnouncementView>(null);
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityButton _closeButton;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityDynamicRawImage _backgroundImage;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private string _battlepassBackgroundImageName;

		[SerializeField]
		private string _competitiveModeBackgroundImageName;

		[SerializeField]
		private string _mergedBackgroundImageName;
	}
}
