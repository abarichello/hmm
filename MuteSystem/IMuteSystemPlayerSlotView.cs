using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation.AxisSelector;

namespace HeavyMetalMachines.MuteSystem
{
	public interface IMuteSystemPlayerSlotView
	{
		ILabel PlayerNameLabel { get; }

		ILabel CharacterNameLabel { get; }

		IDynamicImage CharacterIconDynamicImage { get; }

		IActivatable PsnIdIconActivatable { get; }

		ILabel PsnIdLabel { get; }

		IButton MuteVoiceButton { get; }

		IHoverable MuteVoiceHoverable { get; }

		ISelectable MuteVoiceSelectable { get; }

		IButton UnMuteVoiceButton { get; }

		IHoverable UnMuteVoiceHoverable { get; }

		ISelectable UnMuteVoiceSelectable { get; }

		IButton MuteOtherActionsButton { get; }

		IHoverable MuteOtherActionsHoverable { get; }

		ISelectable MuteOtherActionsSelectable { get; }

		IButton UnMuteOtherActionsButton { get; }

		IHoverable UnMuteOtherActionsHoverable { get; }

		ISelectable UnMuteOtherActionsSelectable { get; }

		IButton BlockButton { get; }

		IHoverable BlockHoverable { get; }

		ISelectable BlockSelectable { get; }

		IButton UnBlockButton { get; }

		IHoverable UnBlockHoverable { get; }

		ISelectable UnBlockSelectable { get; }

		IActivatable BlockInfoActivatable { get; }

		IButton ReportButton { get; }

		IHoverable ReportHoverable { get; }

		ISelectable ReportSelectable { get; }

		void SetCharacterNameLabelAsAlly();

		void SetCharacterNameLabelAsEnemy();

		void SetCharacterNameLabelAsNarrator();

		void TryToSelect(IButton button, IUiNavigationAxisSelectorTransformHandler axisSelectorTransformHandler);

		string GetEmptyCharIconName();
	}
}
