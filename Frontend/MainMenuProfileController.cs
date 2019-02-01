using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

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

		public void Awake()
		{
			this.MainMenuButtonGameObject.SetActive(true);
			for (int i = 0; i < this.ProfileWindows.Length; i++)
			{
				this.ProfileWindows[i].MainMenuProfileWindow.MainMenuProfileController = this;
				this.ProfileWindows[i].MainMenuProfileWindow.OnLoading();
			}
		}

		public void OnDestroy()
		{
			for (int i = 0; i < this.ProfileWindows.Length; i++)
			{
				this.ProfileWindows[i].MainMenuProfileWindow.MainMenuProfileController = null;
				this.ProfileWindows[i].MainMenuProfileWindow.OnUnloading();
			}
		}

		public void AnimateShow()
		{
			this._screenAnimation.gameObject.SetActive(true);
			this._screenAnimation.Play("ProfileIn");
			this.ShowWindow(this.ProfileWindows[0].ProfileWindowType);
			this.ProfileWindows[0].LeftButton.Set(true, true);
		}

		internal MainMenuProfileController.ProfileWindow ShowWindow(MainMenuProfileController.ProfileWindowType profileWindowType)
		{
			this.UpdateData(profileWindowType);
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
			for (int i = 0; i < this.ProfileWindows.Length; i++)
			{
				if (this.ProfileWindows[i].ProfileWindowType == profileWindowType)
				{
					this.ProfileWindows[i].MainMenuProfileWindow.UpdateData();
					break;
				}
			}
		}

		public void ReturnToMainMenu()
		{
			this._screenAnimation.Play("profileOut");
			base.StartCoroutine(GUIUtils.WaitAndDisable(this._screenAnimation.clip.length, this._screenAnimation.gameObject));
			MainMenuProfileController.MainMenuGui.AnimateReturnToLobby(false, false);
			for (int i = 0; i < this.ProfileWindows.Length; i++)
			{
				this.ProfileWindows[i].MainMenuProfileWindow.OnBackToMainMenu();
			}
		}

		public void OnProfileLeftButtonClick(int profileWindowType)
		{
			this.ShowWindow((MainMenuProfileController.ProfileWindowType)profileWindowType);
		}

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
