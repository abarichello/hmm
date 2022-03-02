using System;
using HeavyMetalMachines.Profile;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuProfileController : StateGuiController
	{
		private static MainMenuGui MainMenuGui
		{
			get
			{
				return GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
			}
		}

		private bool IsProfileRefactorEnabled
		{
			get
			{
				return this._isFeatureToggled.Check(Features.ProfileRefactor);
			}
		}

		public void Awake()
		{
			if (this.IsProfileRefactorEnabled)
			{
				return;
			}
			this.MainMenuButtonGameObject.SetActive(true);
			for (int i = 0; i < this.ProfileWindows.Length; i++)
			{
				this.ProfileWindows[i].MainMenuProfileWindow.MainMenuProfileController = this;
				this.ProfileWindows[i].MainMenuProfileWindow.OnLoading();
			}
		}

		public void OnDestroy()
		{
			if (this.IsProfileRefactorEnabled)
			{
				return;
			}
			for (int i = 0; i < this.ProfileWindows.Length; i++)
			{
				this.ProfileWindows[i].MainMenuProfileWindow.MainMenuProfileController = null;
				this.ProfileWindows[i].MainMenuProfileWindow.OnUnloading();
			}
		}

		public void AnimateShow()
		{
			if (this.IsProfileRefactorEnabled)
			{
				return;
			}
			this._screenAnimation.gameObject.SetActive(true);
			this._screenAnimation.Play("ProfileIn");
			this.LegacyShow();
		}

		public void LegacyShow()
		{
			if (this.IsProfileRefactorEnabled)
			{
				return;
			}
			this.ShowWindow(this.ProfileWindows[0].ProfileWindowType);
			this.ProfileWindows[0].LeftButton.Set(true, true);
		}

		internal MainMenuProfileController.ProfileWindow ShowWindow(MainMenuProfileController.ProfileWindowType profileWindowType)
		{
			this.UpdateData(profileWindowType);
			ObservableExtensions.Subscribe<Unit>(this._playerProfilePresenter.Initialize());
			MainMenuProfileController.ProfileWindow result = this.ProfileWindows[0];
			for (int i = 0; i < this.ProfileWindows.Length; i++)
			{
				MainMenuProfileController.ProfileWindow profileWindow = this.ProfileWindows[i];
				bool flag = profileWindow.ProfileWindowType == profileWindowType;
				if (flag)
				{
					result = profileWindow;
				}
				profileWindow.MainMenuProfileWindow.SetWindowVisibility(flag);
				UIButton[] components = profileWindow.LeftButton.GetComponents<UIButton>();
				for (int j = 0; j < components.Length; j++)
				{
					components[j].SetState(UIButtonColor.State.Normal, true);
					components[j].UpdateColor(true);
				}
			}
			return result;
		}

		private void UpdateData(MainMenuProfileController.ProfileWindowType profileWindowType)
		{
			if (this.IsProfileRefactorEnabled)
			{
				return;
			}
			for (int i = 0; i < this.ProfileWindows.Length; i++)
			{
				if (this.ProfileWindows[i].ProfileWindowType == profileWindowType)
				{
					this.ProfileWindows[i].MainMenuProfileWindow.UpdateData();
					break;
				}
			}
		}

		[Obsolete]
		public void ReturnToMainMenu()
		{
			if (this.IsProfileRefactorEnabled)
			{
				return;
			}
			MainMenuProfileController.Log.WarnStackTrace("Obsolete ReturnToMainMenu");
		}

		public void LegacyPreHide()
		{
			if (this.IsProfileRefactorEnabled)
			{
				return;
			}
			for (int i = 0; i < this.ProfileWindows.Length; i++)
			{
				this.ProfileWindows[i].MainMenuProfileWindow.OnPreBackToMainMenu();
			}
		}

		public void LegacyHide()
		{
			if (this.IsProfileRefactorEnabled)
			{
				return;
			}
			for (int i = 0; i < this.ProfileWindows.Length; i++)
			{
				this.ProfileWindows[i].MainMenuProfileWindow.OnBackToMainMenu();
			}
		}

		public void OnProfileLeftButtonClick(int profileWindowType)
		{
			if (this.IsProfileRefactorEnabled)
			{
				return;
			}
			this.ShowWindow((MainMenuProfileController.ProfileWindowType)profileWindowType);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MainMenuProfileController));

		[Inject]
		private IPlayerProfilePresenter _playerProfilePresenter;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		public GameObject MainMenuButtonGameObject;

		public MainMenuProfileController.ProfileWindow[] ProfileWindows;

		[SerializeField]
		private Animation _screenAnimation;

		[Serializable]
		public struct ProfileWindow
		{
			public MainMenuProfileController.ProfileWindowType ProfileWindowType;

			public MainMenuProfileWindow MainMenuProfileWindow;

			public UIToggle LeftButton;
		}

		public enum ProfileWindowType
		{
			Summary,
			Machines,
			Matches
		}
	}
}
