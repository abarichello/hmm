using System;
using Pocketverse;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Tutorial.UnityUI
{
	[CreateAssetMenu(menuName = "UnityUI/TutorialHintComponent")]
	public class TutorialHintComponent : GameHubScriptableObject, ITutorialComponent
	{
		public void Load()
		{
			SceneManager.LoadSceneAsync("UI_ADD_HintTutorial", LoadSceneMode.Additive);
		}

		public void Unload()
		{
			SceneManager.UnloadSceneAsync("UI_ADD_HintTutorial");
		}

		public void RegisterEvents()
		{
			GameHubScriptableObject.Hub.GuiScripts.Esc.OnEscMenuVisibleChanged += this.Esc_OnVisibilityChange;
		}

		public void UnregisterEvents()
		{
			GameHubScriptableObject.Hub.GuiScripts.Esc.OnEscMenuVisibleChanged -= this.Esc_OnVisibilityChange;
		}

		public void RegisterView(ITutorialPanelView tutorialHintPanelView)
		{
			this._tutorialHintPanelView = tutorialHintPanelView;
		}

		public void Show()
		{
			this._tutorialHintPanelView.SetVisibility(true);
		}

		private void Esc_OnVisibilityChange(bool visible)
		{
			if (visible)
			{
				this.Hide();
			}
			else
			{
				this.Show();
			}
		}

		public void Hide()
		{
			this._tutorialHintPanelView.SetVisibility(false);
		}

		private const string TUTORIAL_HINT_SCENE_NAME = "UI_ADD_HintTutorial";

		private ITutorialPanelView _tutorialHintPanelView;
	}
}
