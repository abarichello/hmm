using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using JetBrains.Annotations;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class MainMenuView : GameHubBehaviour, IMainMenuView
	{
		public IAnimation ShowLobbyAnimation
		{
			get
			{
				return this._showLobbyAnimation;
			}
		}

		public IAnimation HideLobbyAnimation
		{
			get
			{
				return this._hideLobbyAnimation;
			}
		}

		public IButton PlayButton
		{
			get
			{
				return this._playButton;
			}
		}

		public IButton TrainingButton
		{
			get
			{
				return this._trainingButton;
			}
		}

		public IButton StoreButton
		{
			get
			{
				return this._storeButton;
			}
		}

		public IButton BattlepassButton
		{
			get
			{
				return this._battlepassButton;
			}
		}

		public IButton TournamentButton
		{
			get
			{
				return this._tournamentButton;
			}
		}

		public IButton RankingButton
		{
			get
			{
				return this._rankingButton;
			}
		}

		public IButton InventoryButton
		{
			get
			{
				return this._inventoryButton;
			}
		}

		public IButton ProfileButton
		{
			get
			{
				return this._profileButton;
			}
		}

		public IButton SpectatorButton
		{
			get
			{
				return this._spectatorButton;
			}
		}

		public IButton TeamsButton
		{
			get
			{
				return this._teamsButton;
			}
		}

		public IButton MetalSponsorsButton
		{
			get
			{
				return this._metalSponsorsButton;
			}
		}

		public IButton HelpButton
		{
			get
			{
				return this._helpButton;
			}
		}

		public IButton DiscordButton
		{
			get
			{
				return this._discordButton;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IUiNavigationEdgeNotifier UiNavigationEdgeNotifier
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<IMainMenuView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IMainMenuView>(null);
		}

		[SerializeField]
		private UnityAnimation _showLobbyAnimation;

		[SerializeField]
		private UnityAnimation _hideLobbyAnimation;

		[SerializeField]
		private NGuiButton _playButton;

		[SerializeField]
		private NGuiButton _trainingButton;

		[SerializeField]
		private NGuiButton _storeButton;

		[SerializeField]
		private NGuiButton _battlepassButton;

		[SerializeField]
		private NGuiButton _tournamentButton;

		[SerializeField]
		private NGuiButton _rankingButton;

		[SerializeField]
		private NGuiButton _inventoryButton;

		[SerializeField]
		private NGuiButton _profileButton;

		[SerializeField]
		private NGuiButton _spectatorButton;

		[SerializeField]
		private NGuiButton _teamsButton;

		[SerializeField]
		private NGuiButton _metalSponsorsButton;

		[SerializeField]
		private NGuiButton _helpButton;

		[SerializeField]
		private NGuiButton _discordButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[InjectOnClient]
		[UsedImplicitly]
		private IViewProvider _viewProvider;

		[InjectOnClient]
		private IMainMenuGuiProvider _mainMenuGuiProvider;
	}
}
