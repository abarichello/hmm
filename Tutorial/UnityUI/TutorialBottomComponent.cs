using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Tutorial.UnityUI
{
	[CreateAssetMenu(menuName = "UnityUI/TutorialBottomComponent")]
	public class TutorialBottomComponent : ScriptableObject, ITutorialBottomComponent, ITutorialComponent
	{
		public string TitleLabelDraft
		{
			get
			{
				return this._titleLabelTranslated;
			}
		}

		public TranslationSheets TitleSheet
		{
			get
			{
				return this._titleSheet;
			}
		}

		public string DescriptionLabelDraft
		{
			get
			{
				return this._descriptionLabelTranslated;
			}
		}

		public TranslationSheets DescriptionSheet
		{
			get
			{
				return this._descriptionSheet;
			}
		}

		public void Load()
		{
			SceneManager.LoadSceneAsync("UI_ADD_TutorialBottomPanel", LoadSceneMode.Additive);
		}

		public void Unload()
		{
			SceneManager.UnloadSceneAsync("UI_ADD_TutorialBottomPanel");
		}

		public void RegisterEvents()
		{
		}

		public void UnregisterEvents()
		{
		}

		public void RegisterView(ITutorialPanelView tutorialPanelView)
		{
			this._tutorialPanelView = tutorialPanelView;
		}

		public void Show(string titleLabelTranslated, string descriptionLabelTranslated)
		{
			this._titleLabelTranslated = titleLabelTranslated;
			this._descriptionLabelTranslated = descriptionLabelTranslated;
			this._tutorialPanelView.SetVisibility(true);
		}

		public void Hide()
		{
			this._tutorialPanelView.SetVisibility(false);
		}

		private const string TUTORIAL_BOTTOM_SCENE_NAME = "UI_ADD_TutorialBottomPanel";

		private ITutorialPanelView _tutorialPanelView;

		private string _titleLabelTranslated;

		private TranslationSheets _titleSheet;

		private string _descriptionLabelTranslated;

		private TranslationSheets _descriptionSheet;
	}
}
