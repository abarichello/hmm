using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;

namespace HeavyMetalMachines.MuteSystem
{
	public interface IMuteSystemPresenterView
	{
		ICanvas MainCanvas { get; }

		ICanvasGroup MainCanvasGroup { get; }

		IButton ExitButton { get; }

		IAnimator ContainerAnimator { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IUiNavigationAxisSelector UiNavigationAxisSelector { get; }

		IMuteSystemSubtitleView MuteSystemSubtitleView { get; }

		IMuteSystemPlayerSlotView AddPlayerSlot();

		void AddSeparator(int index);

		float GetInAnimationLength();

		float GetOutAnimationLength();

		void TryToSelect(IButton button, IMuteSystemPlayerSlotView playerSlotView);

		void ShowNarratorTitle(int index);
	}
}
