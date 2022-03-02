using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public interface IMainMenuView
	{
		IAnimation ShowLobbyAnimation { get; }

		IAnimation HideLobbyAnimation { get; }

		IButton PlayButton { get; }

		IButton TrainingButton { get; }

		IButton StoreButton { get; }

		IButton BattlepassButton { get; }

		IButton TournamentButton { get; }

		IButton RankingButton { get; }

		IButton InventoryButton { get; }

		IButton ProfileButton { get; }

		IButton SpectatorButton { get; }

		IButton TeamsButton { get; }

		IButton MetalSponsorsButton { get; }

		IButton HelpButton { get; }

		IButton DiscordButton { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IUiNavigationEdgeNotifier UiNavigationEdgeNotifier { get; }

		IUiNavigationAxisSelector UiNavigationAxisSelector { get; }
	}
}
