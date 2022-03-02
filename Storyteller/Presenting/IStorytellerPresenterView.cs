using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.UnityUI;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using UniRx;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public interface IStorytellerPresenterView
	{
		ICanvas MainCanvas { get; }

		ICanvasGroup MainCanvasGroup { get; }

		IButton BackButton { get; }

		IButton RefreshButton { get; }

		IButton SearchButton { get; }

		IDropdown<string> RegionsDropdown { get; }

		IDropdown<string> QueuesDropdown { get; }

		IInputField SearchField { get; }

		UnityUiTitleInfo Title { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IUiNavigationSubGroupHolder UiNavigationTopSubGroupHolder { get; }

		IUiNavigationSubGroupHolder UiNavigationMatchListSubGroupHolder { get; }

		IActivatable NoResultsGroup { get; }

		ILabel NoResultsLabel { get; }

		IUiNavigationAxisSelector UiNavigationTopGroupAxisSelector { get; }

		IUiNavigationAxisSelector UiNavigationMatchListAxisSelector { get; }

		IStorytellerGameserverScroller GameServersScroller { get; }

		IStorytellerGameserverConnection GameServerConnection { get; }

		void UiNavigationPrioritySelection();

		IStorytellerMatchInfo CreateMatchInfo();

		void PlaySearchFailedByMinCharsAnim();

		void ShowLoadingMatches();

		void HideLoadingMatches();

		void EnableSearchInteractions();

		void DisableSearchInteractions();

		IObservable<Unit> PlayIntroAnim();

		IObservable<Unit> PlayOutroAnim();

		void ShowWaitingWindow();

		void HideWaitingWindow();

		IObservable<string> OnVirtualKeyboardClose();
	}
}
