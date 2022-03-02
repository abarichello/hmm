using System;
using HeavyMetalMachines.Frontend;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.RadialMenu.View
{
	public interface IRadialMenuView
	{
		ITextureMappingUpdater[] SpritesheetAnimators { get; }

		RadialMenuMouseNotifier RadialMenuMouseNotifier { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IUiNavigationContextInputNotifier UiNavigationContextInputNotifier { get; }

		void WindowAnimationIn();

		void WindowAnimationOut();

		void SelectorGlowIn(int index);

		void SelectorGlowOut(int index);
	}
}
