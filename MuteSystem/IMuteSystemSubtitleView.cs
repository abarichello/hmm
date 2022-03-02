using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.MuteSystem
{
	public interface IMuteSystemSubtitleView
	{
		IActivatable MainActivatable { get; }

		ISpriteImage IconSpriteImage { get; }

		ILabel TitleLabel { get; }

		ILabel SubTitleLabel { get; }

		void GetSubtitleDrafts(MuteSystemSubtitleType subtitleType, out string titleDraft, out string subTitleDraft);

		ISprite GetIconSprite(MuteSystemSubtitleType subtitleType);
	}
}
