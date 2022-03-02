using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.VFX;
using Hoplon.Input.UiNavigation;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Profile.View
{
	public class LegacyNguiProfileView : MonoBehaviour, ILegacyProfileView
	{
		public IActivatable MainActivatable
		{
			get
			{
				return this._pivotGameObject;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IButton AvatarChangeButton
		{
			get
			{
				return this._avatarChangeButton;
			}
		}

		public IObservable<Unit> Show()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._legacyView.LegacyShow();
			}), this._animationIn.Play());
		}

		public IObservable<Unit> Hide()
		{
			this._legacyView.LegacyPreHide();
			return Observable.Do<Unit>(this._animationOut.Play(), delegate(Unit _)
			{
				this._legacyView.LegacyHide();
			});
		}

		public IActivatable AvatarLoading
		{
			get
			{
				return this._avatarImageLoading;
			}
		}

		public ILabel PlayerTagLabel
		{
			get
			{
				return this._playerTagLabel;
			}
		}

		public IButton CopyPlayerTagButton
		{
			get
			{
				return this._copyPlayerTagButton;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public void SetPortraitAndAvatarImage(string avatarImageName, string portraitImageName)
		{
			this._avatarImage.TextureName = avatarImageName;
			this._portraitImage.SpriteName = portraitImageName;
		}

		private void Awake()
		{
			this._viewProvider.Bind<ILegacyProfileView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ILegacyProfileView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private MainMenuProfileController _legacyView;

		[SerializeField]
		private UnityAnimation _animationIn;

		[SerializeField]
		private UnityAnimation _animationOut;

		[SerializeField]
		private GameObjectActivatable _pivotGameObject;

		[SerializeField]
		private NGuiButton _backButton;

		[SerializeField]
		private NGuiButton _avatarChangeButton;

		[SerializeField]
		private HMMUI2DDynamicTexture _avatarImage;

		[SerializeField]
		private HMMUI2DDynamicSprite _portraitImage;

		[SerializeField]
		private GameObjectActivatable _avatarImageLoading;

		[SerializeField]
		private NGuiLabel _playerTagLabel;

		[SerializeField]
		private NGuiButton _copyPlayerTagButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
