using System;

namespace HeavyMetalMachines.Tutorial.UnityUI
{
	internal interface ITutorialBottomComponent : ITutorialComponent
	{
		string TitleLabelDraft { get; }

		TutorialBottomDataDescription[] DataDescriptions { get; }
	}
}
