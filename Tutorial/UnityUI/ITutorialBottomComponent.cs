using System;

namespace HeavyMetalMachines.Tutorial.UnityUI
{
	internal interface ITutorialBottomComponent : ITutorialComponent
	{
		string TitleLabelDraft { get; }

		TranslationSheets TitleSheet { get; }

		string DescriptionLabelDraft { get; }

		TranslationSheets DescriptionSheet { get; }
	}
}
