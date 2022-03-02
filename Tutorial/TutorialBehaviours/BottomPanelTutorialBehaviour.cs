using System;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Tutorial.InGame;
using HeavyMetalMachines.Tutorial.UnityUI;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.TutorialBehaviours
{
	public class BottomPanelTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnClient()
		{
			TutorialBottomComponent bottomPanelComponent = TutorialUIController.Instance.BottomPanelComponent;
			base.StartBehaviourOnClient();
			if (this._show)
			{
				string titleLabelTranslated = Language.Get(this._titleLabelDraft, this._titleSheet);
				TutorialBottomDataDescription[] array = new TutorialBottomDataDescription[this._descriptions.Length];
				for (int i = 0; i < this._descriptions.Length; i++)
				{
					TutorialDataDescription tutorialDataDescription = this._descriptions[i];
					array[i] = new TutorialBottomDataDescription
					{
						DescriptionTranslated = Language.Get(tutorialDataDescription.Draft, tutorialDataDescription.Sheet),
						ControllerInputAction = tutorialDataDescription.ControllerInputAction
					};
				}
				bottomPanelComponent.Show(titleLabelTranslated, array);
			}
			else
			{
				bottomPanelComponent.Hide();
			}
		}

		[SerializeField]
		private bool _show;

		[SerializeField]
		private string _titleLabelDraft;

		[SerializeField]
		private TranslationSheets _titleSheet;

		[SerializeField]
		private TutorialDataDescription[] _descriptions;
	}
}
