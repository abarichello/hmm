using System;
using HeavyMetalMachines.Options;
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
				string format = Language.Get(this._descriptionLabelDraft, this._descriptionSheet);
				string descriptionLabelTranslated = TutorialControlActionMsgFormatter.Format(format, this._actions);
				bottomPanelComponent.Show(Language.Get(this._titleLabelDraft, this._titleSheet), descriptionLabelTranslated);
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
		private string _descriptionLabelDraft;

		[SerializeField]
		private TranslationSheets _descriptionSheet;

		[SerializeField]
		private ControlAction[] _actions;
	}
}
