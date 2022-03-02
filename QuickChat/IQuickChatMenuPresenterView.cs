using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.RadialMenu.View;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.QuickChat
{
	public interface IQuickChatMenuPresenterView
	{
		IRadialMenuNotifier RadialMenuNotifier { get; }

		IAnimator ContainerAnimator { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IUiNavigationContextInputNotifier UiNavigationContextInputNotifier { get; }

		void ShowHighlight(int index);

		void HideHighlight(int index);

		void HideAllHighlights();
	}
}
