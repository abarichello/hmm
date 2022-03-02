using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Spectator.View
{
	public class SpectatorHelperView : MonoBehaviour, ISpectatorHelperView
	{
		public IButton CloseButton
		{
			get
			{
				return this._closeButton;
			}
		}

		public IActivatable KeyboardInputGroup
		{
			get
			{
				return this._keyboardInputGroup;
			}
		}

		public IActivatable GamepadInputGroup
		{
			get
			{
				return this._gamepadInputGroup;
			}
		}

		public ICanvas Canvas
		{
			get
			{
				return this._canvas;
			}
		}

		private void OnEnable()
		{
			this._presenter.Initialize(this);
			this._hortaOnlyGroup.SetActive(this._config.GetBoolValue(ConfigAccess.HORTA));
		}

		private void OnDisable()
		{
			this._presenter.Dispose();
		}

		public void PlayInAnimation()
		{
			this._viewObject.SetActive(true);
			this._animator.SetBool("active", true);
		}

		public void PlayOutAnimation()
		{
			this._animator.SetBool("active", false);
		}

		public ISpriteImage SetNextMachineSelectionGampepadSprite
		{
			get
			{
				return this._nextMachineSelectionGampepadSprite;
			}
		}

		public ISpriteImage SetPreviousMachineSelectionGampepadSprite
		{
			get
			{
				return this._previousMachineSelectionGampepadSprite;
			}
		}

		public ISpriteImage SetBombSelectionGampepadSprite
		{
			get
			{
				return this._bombSelectionGampepadSprite;
			}
		}

		public ISpriteImage SetZoomInGampepadSprite
		{
			get
			{
				return this._zoomInGampepadSprite;
			}
		}

		public ISpriteImage SetZoomOutGampepadSprite
		{
			get
			{
				return this._zoomOutGampepadSprite;
			}
		}

		public ISpriteImage SetToogleOrbitalCameraGampepadSprite
		{
			get
			{
				return this._toogleOrbitalCameraGampepadSprite;
			}
		}

		public ISpriteImage SetToogleFreeCameraGampepadSprite
		{
			get
			{
				return this._toogleFreeCameraGampepadSprite;
			}
		}

		public ISpriteImage SetTeamBlueFixedCameraGampepadSprite
		{
			get
			{
				return this._teamBlueFixedCameraGampepadSprite;
			}
		}

		public ISpriteImage SetTeamRedFixedCameraGampepadSprite
		{
			get
			{
				return this._teamRedFixedCameraGampepadSprite;
			}
		}

		public ISpriteImage SetCenterArenaFixedCameraGampepadSprite
		{
			get
			{
				return this._centerArenaFixedCameraGampepadSprite;
			}
		}

		public ISpriteImage SetFreeCameraMovementGampepadSprite
		{
			get
			{
				return this._freeCameraMovementGampepadSprite;
			}
		}

		public ISpriteImage SetOrbitalCameraMovementGampepadSprite
		{
			get
			{
				return this._orbitalCameraMovementGampepadSprite;
			}
		}

		[InjectOnClient]
		private ISpectatorHelperPresenter _presenter;

		[SerializeField]
		private UnityButton _closeButton;

		[SerializeField]
		private UnityCanvas _canvas;

		[SerializeField]
		private GameObjectActivatable _viewObject;

		[SerializeField]
		private GameObjectActivatable _hortaOnlyGroup;

		[SerializeField]
		private GameObjectActivatable _keyboardInputGroup;

		[SerializeField]
		private GameObjectActivatable _gamepadInputGroup;

		[SerializeField]
		private UnityImage _nextMachineSelectionGampepadSprite;

		[SerializeField]
		private UnityImage _previousMachineSelectionGampepadSprite;

		[SerializeField]
		private UnityImage _bombSelectionGampepadSprite;

		[SerializeField]
		private UnityImage _zoomInGampepadSprite;

		[SerializeField]
		private UnityImage _zoomOutGampepadSprite;

		[SerializeField]
		private UnityImage _toogleOrbitalCameraGampepadSprite;

		[SerializeField]
		private UnityImage _toogleFreeCameraGampepadSprite;

		[SerializeField]
		private UnityImage _teamBlueFixedCameraGampepadSprite;

		[SerializeField]
		private UnityImage _teamRedFixedCameraGampepadSprite;

		[SerializeField]
		private UnityImage _centerArenaFixedCameraGampepadSprite;

		[SerializeField]
		private UnityImage _freeCameraMovementGampepadSprite;

		[SerializeField]
		private UnityImage _orbitalCameraMovementGampepadSprite;

		[SerializeField]
		private Animator _animator;

		[Inject]
		private IConfigLoader _config;
	}
}
