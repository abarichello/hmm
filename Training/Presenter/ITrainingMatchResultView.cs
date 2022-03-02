using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.Training.Presenter
{
	public interface ITrainingMatchResultView
	{
		ICanvas Canvas { get; }

		IDynamicImage BackgroundImage { get; }

		IAnimation ShowAnimationVictory { get; }

		IAnimation ShowAnimationDefeat { get; }

		IAnimation HideAnimation { get; }

		IButton GoBackToMainMenuButton { get; }

		ILabel ArenaName { get; }

		IDynamicImage IconArena { get; }

		IDynamicImage IconGlow { get; }

		IDynamicImage AssertIcon { get; }

		IDynamicImage AssertIconGlow { get; }

		ILabel MatchConcludeLabel { get; }

		Color WinColor { get; }

		Color LoseColor { get; }

		string MatchWinDraft { get; }

		string MatchLoseDraft { get; }

		string AssertIconMatchWonName { get; }

		string AssertIconMatchLoseName { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
