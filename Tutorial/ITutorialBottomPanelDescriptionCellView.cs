using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Tutorial
{
	public interface ITutorialBottomPanelDescriptionCellView
	{
		IActivatable MainActivatable { get; }

		ILabel DescriptionLabel { get; }

		IActivatable InputIconActivatable { get; }

		ISpriteImage InputIconImage { get; }

		IActivatable KeyboardActivatable { get; }

		ILabel KeyboardLabel { get; }
	}
}
