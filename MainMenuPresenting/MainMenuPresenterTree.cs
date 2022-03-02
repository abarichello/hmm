using System;
using System.Collections.Generic;
using HeavyMetalMachines.Battlepass.View;
using HeavyMetalMachines.CompetitiveMode.View;
using HeavyMetalMachines.CompetitiveMode.View.Divisions;
using HeavyMetalMachines.CompetitiveMode.View.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.View.Prizes;
using HeavyMetalMachines.CompetitiveMode.View.Ranking;
using HeavyMetalMachines.CustomMatch;
using HeavyMetalMachines.Inventory.Presenter;
using HeavyMetalMachines.Inventory.Tab.Presenter;
using HeavyMetalMachines.MainMenuPresenting.Presenter;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Navigation;
using HeavyMetalMachines.Social.Profile.Presenting;
using HeavyMetalMachines.Store.Presenter;
using HeavyMetalMachines.Store.Tabs.Presenter;
using HeavyMetalMachines.Storyteller.Presenting;
using HeavyMetalMachines.Tournaments.Presenting.Detailing;
using HeavyMetalMachines.Tournaments.Presenting.Listing;
using HeavyMetalMachines.Training.Presenter;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class MainMenuPresenterTree : IMainMenuPresenterTree
	{
		public MainMenuPresenterTree(DiContainer diContainer, IPresenterTree presenterTree)
		{
			this._diContainer = diContainer;
			this._presenterTree = presenterTree;
		}

		public void Dispose()
		{
			this._presenterTree.Dispose();
		}

		public IPresenterTree PresenterTree
		{
			get
			{
				return this._presenterTree;
			}
		}

		public IPresenterNode MainMenuNode { get; private set; }

		public IPresenterNode PlayModesNode { get; private set; }

		public IPresenterNode StoreDriversNode { get; private set; }

		public IPresenterNode StoreCashNode { get; private set; }

		public IPresenterNode StoreBoostersNode { get; private set; }

		public IPresenterNode StoreEffectsNode { get; private set; }

		public IPresenterNode StoreEmotesNode { get; private set; }

		public IPresenterNode StoreSkinsNode { get; private set; }

		public IPresenterNode StoreSpraysNode { get; private set; }

		public IPresenterNode PreCustomMatchNode { get; private set; }

		public IPresenterNode CustomMatchNode { get; private set; }

		public IPresenterNode TournamentNode { get; private set; }

		public IPresenterNode TournamentListNode { get; private set; }

		public IPresenterNode InventoryPortraitNode { get; private set; }

		public IPresenterNode InventoryScoreNode { get; private set; }

		public IPresenterNode InventoryAvatarsNode { get; private set; }

		public IPresenterNode InventoryKillsNode { get; private set; }

		public IPresenterNode InventoryLoresNode { get; private set; }

		public IPresenterNode InventoryRespawnsNode { get; private set; }

		public IPresenterNode InventorySkinsNode { get; private set; }

		public IPresenterNode InventorySpraysNode { get; private set; }

		public IPresenterNode InventoryTakeoffsNode { get; private set; }

		public IPresenterNode InventoryEmotesNode { get; private set; }

		public IPresenterNode CompetitiveModeNode { get; private set; }

		public IPresenterNode BattlepassNode { get; private set; }

		public IPresenterNode ProfileNode { get; private set; }

		public IPresenterNode ProfileInventoryPortraitNode { get; private set; }

		public IPresenterNode ProfileInventoryScoreNode { get; private set; }

		public IPresenterNode ProfileInventoryAvatarsNode { get; private set; }

		public IPresenterNode ProfileInventoryKillsNode { get; private set; }

		public IPresenterNode ProfileInventoryLoresNode { get; private set; }

		public IPresenterNode ProfileInventoryRespawnsNode { get; private set; }

		public IPresenterNode ProfileInventorySkinsNode { get; private set; }

		public IPresenterNode ProfileInventorySpraysNode { get; private set; }

		public IPresenterNode ProfileInventoryTakeoffsNode { get; private set; }

		public IPresenterNode ProfileInventoryEmotesNode { get; private set; }

		public IPresenterNode SpectatorNode { get; private set; }

		public IPresenterNode MainMenuCompetitiveModeNode { get; private set; }

		public IPresenterNode MainMenuTrainingSelectionNode { get; private set; }

		public IPresenterNode TrainingSelectionNode { get; private set; }

		public IPresenterNode MainMenuPreCustomMatchNode { get; private set; }

		private void InitializeNodes()
		{
			this.MainMenuCompetitiveModeNode = this.InitializeCompetitiveBranch("Competitive");
			this.MainMenuTrainingSelectionNode = this.CreateNode<ITrainingSelectionScreenPresenter>("TrainingSelection", new IPresenterNode[0]);
			this.TournamentNode = this.CreateNode<ITournamentPresenter>("Tournament", new IPresenterNode[0]);
			this.TournamentListNode = this.CreateNode<ITournamentListPresenter>("TournamentList", new IPresenterNode[]
			{
				this.TournamentNode
			});
			this.BattlepassNode = this.CreateNode<IBattlepassPresenter>("Battlepass", new IPresenterNode[0]);
			this.PlayModesNode = this.InitializePlayModesNode();
			this._storeNode = this.InitializeStoreNodes();
			this._inventoryNode = this.InitializeInventoryNodes("Inventory");
			this.ProfileNode = this.InitializeProfileNodes();
			this.SpectatorNode = this.CreateNode<IStorytellerPresenter>("Storyteller", new IPresenterNode[0]);
			this.MainMenuNode = this.CreateNode<IMainMenuPresenter>("StartMenu", new IPresenterNode[0]);
			this.CustomMatchNode = this.CreateNode<ICustomMatchPresenter>("CustomMatch", new IPresenterNode[0]);
			this.MainMenuPreCustomMatchNode = this.CreateNode<IPreCustomMatchPresenter>("PreCustomMatch", new IPresenterNode[0]);
			this._startNode = new PresenterNode("Start", new IPresenterNode[]
			{
				this.MainMenuNode,
				this.PlayModesNode,
				this._storeNode,
				this.BattlepassNode,
				this.TournamentListNode,
				this._inventoryNode,
				this.ProfileNode,
				this.SpectatorNode,
				this.MainMenuCompetitiveModeNode,
				this.MainMenuTrainingSelectionNode,
				this.CustomMatchNode,
				this.MainMenuPreCustomMatchNode
			}, () => this._diContainer.Resolve<IStartPresenter>());
		}

		private IPresenterNode InitializePlayModesNode()
		{
			this.PreCustomMatchNode = this.CreateNode<IPreCustomMatchPresenter>("PlayModes/PreCustomMatch", new IPresenterNode[0]);
			this.TrainingSelectionNode = this.CreateNode<ITrainingSelectionScreenPresenter>("PlayModes/TrainingSelection", new IPresenterNode[0]);
			this.CompetitiveModeNode = this.InitializeCompetitiveBranch("PlayModes/Competitive");
			return this.CreateNode<IPlayModesPresenter>("PlayModes", new IPresenterNode[]
			{
				this.CompetitiveModeNode,
				this.PreCustomMatchNode,
				this.TrainingSelectionNode
			});
		}

		private IPresenterNode InitializeProfileNodes()
		{
			string text = "Profile/Inventory";
			this.ProfileInventoryPortraitNode = this.CreateNode<IPortraitInventoryTabPresenter>(text + "/Portraits", new IPresenterNode[0]);
			this.ProfileInventoryScoreNode = this.CreateNode<IScoreInventoryTabPresenter>(text + "/Scores", new IPresenterNode[0]);
			this.ProfileInventoryAvatarsNode = this.CreateNode<IAvatarsInventoryTabPresenter>(text + "/Avatars", new IPresenterNode[0]);
			this.ProfileInventoryKillsNode = this.CreateNode<IKillsInventoryTabPresenter>(text + "/Kills", new IPresenterNode[0]);
			this.ProfileInventoryLoresNode = this.CreateNode<ILoreInventoryTabPresenter>(text + "/Lores", new IPresenterNode[0]);
			this.ProfileInventoryRespawnsNode = this.CreateNode<IRespawnsInventoryTabPresenter>(text + "/Respawns", new IPresenterNode[0]);
			this.ProfileInventorySkinsNode = this.CreateNode<ISkinsInventoryTabPresenter>(text + "/Skins", new IPresenterNode[0]);
			this.ProfileInventorySpraysNode = this.CreateNode<ISpraysInventoryTabPresenter>(text + "/Sprays", new IPresenterNode[0]);
			this.ProfileInventoryTakeoffsNode = this.CreateNode<ITakeoffInventoryTabPresenter>(text + "/Takeoffs", new IPresenterNode[0]);
			this.ProfileInventoryEmotesNode = this.CreateNode<IEmotesInventoryTabPresenter>(text + "/Emotes", new IPresenterNode[0]);
			this._profileInventoryNode = this.CreateNode<IInventoryPresenter>(text, new IPresenterNode[]
			{
				this.ProfileInventoryPortraitNode,
				this.ProfileInventoryScoreNode,
				this.ProfileInventoryAvatarsNode,
				this.ProfileInventoryKillsNode,
				this.ProfileInventoryLoresNode,
				this.ProfileInventoryRespawnsNode,
				this.ProfileInventorySkinsNode,
				this.ProfileInventorySpraysNode,
				this.ProfileInventoryTakeoffsNode,
				this.ProfileInventoryEmotesNode
			});
			return this.CreateNode<IProfilePresenter>("Profile", new IPresenterNode[]
			{
				this._profileInventoryNode
			});
		}

		private IPresenterNode InitializeInventoryNodes(string prefix)
		{
			this.InventoryPortraitNode = this.CreateNode<IPortraitInventoryTabPresenter>(prefix + "/Portraits", new IPresenterNode[0]);
			this.InventoryScoreNode = this.CreateNode<IScoreInventoryTabPresenter>(prefix + "/Scores", new IPresenterNode[0]);
			this.InventoryAvatarsNode = this.CreateNode<IAvatarsInventoryTabPresenter>(prefix + "/Avatars", new IPresenterNode[0]);
			this.InventoryKillsNode = this.CreateNode<IKillsInventoryTabPresenter>(prefix + "/Kills", new IPresenterNode[0]);
			this.InventoryLoresNode = this.CreateNode<ILoreInventoryTabPresenter>(prefix + "/Lores", new IPresenterNode[0]);
			this.InventoryRespawnsNode = this.CreateNode<IRespawnsInventoryTabPresenter>(prefix + "/Respawns", new IPresenterNode[0]);
			this.InventorySkinsNode = this.CreateNode<ISkinsInventoryTabPresenter>(prefix + "/Skins", new IPresenterNode[0]);
			this.InventorySpraysNode = this.CreateNode<ISpraysInventoryTabPresenter>(prefix + "/Sprays", new IPresenterNode[0]);
			this.InventoryTakeoffsNode = this.CreateNode<ITakeoffInventoryTabPresenter>(prefix + "/Takeoffs", new IPresenterNode[0]);
			this.InventoryEmotesNode = this.CreateNode<IEmotesInventoryTabPresenter>(prefix + "/Emotes", new IPresenterNode[0]);
			return this.CreateNode<IInventoryPresenter>(prefix, new IPresenterNode[]
			{
				this.InventoryPortraitNode,
				this.InventoryScoreNode,
				this.InventoryAvatarsNode,
				this.InventoryKillsNode,
				this.InventoryLoresNode,
				this.InventoryRespawnsNode,
				this.InventorySkinsNode,
				this.InventorySpraysNode,
				this.InventoryTakeoffsNode,
				this.InventoryEmotesNode
			});
		}

		private IPresenterNode InitializeStoreNodes()
		{
			this.StoreDriversNode = this.CreateNode<IDriversStoreTabPresenter>("Store/Drivers", new IPresenterNode[0]);
			this.StoreCashNode = this.CreateNode<ICashStoreTabPresenter>("Store/Cash", new IPresenterNode[0]);
			this.StoreBoostersNode = this.CreateNode<IBoostersStoreTabPresenter>("Store/Boosters", new IPresenterNode[0]);
			this.StoreEffectsNode = this.CreateNode<IEffectsStoreTabPresenter>("Store/Effects", new IPresenterNode[0]);
			this.StoreEmotesNode = this.CreateNode<IEmotesStoreTabPresenter>("Store/Emotes", new IPresenterNode[0]);
			this.StoreSkinsNode = this.CreateNode<ISkinsStoreTabPresenter>("Store/Skins", new IPresenterNode[0]);
			this.StoreSpraysNode = this.CreateNode<ISpraysStoreTabPresenter>("Store/Sprays", new IPresenterNode[0]);
			return this.CreateNode<IStorePresenter>("Store", new IPresenterNode[]
			{
				this.StoreDriversNode,
				this.StoreCashNode,
				this.StoreBoostersNode,
				this.StoreEffectsNode,
				this.StoreEmotesNode,
				this.StoreSkinsNode,
				this.StoreSpraysNode
			});
		}

		private IPresenterNode InitializeCompetitiveBranch(string prefix)
		{
			IPresenterNode presenterNode = this.CreateNode<ICompetitiveDivisionsPresenter>(prefix + "/Divisions", new IPresenterNode[0]);
			IPresenterNode presenterNode2 = this.CreateNode<ICompetitiveRewardsPresenter>(prefix + "/Rewards", new IPresenterNode[0]);
			IPresenterNode presenterNode3 = this.CreateNode<ICompetitiveRankingPresenter>(prefix + "/Ranking", new IPresenterNode[0]);
			IPresenterNode presenterNode4 = this.CreateNode<ICompetitiveQueuePeriodsPresenter>(prefix + "/QueuePeriods", new IPresenterNode[0]);
			CompetitiveModeTreeBranch branch = new CompetitiveModeTreeBranch
			{
				DivisionsNode = presenterNode,
				RankingNode = presenterNode3,
				RewardsNode = presenterNode2,
				QueuePeriodsNode = presenterNode4
			};
			DiContainer container = this.CreateCompetitiveBranchSubContainer(branch);
			return this.CreateNode<ICompetitiveModePresenter>(prefix, container, new IPresenterNode[]
			{
				presenterNode,
				presenterNode2,
				presenterNode3,
				presenterNode4
			});
		}

		private DiContainer CreateCompetitiveBranchSubContainer(CompetitiveModeTreeBranch branch)
		{
			DiContainer diContainer = this._diContainer.CreateSubContainer();
			Type type = this._diContainer.ResolveType<ICompetitiveModePresenter>();
			diContainer.Bind<ICompetitiveModePresenter>().To(new Type[]
			{
				type
			}).AsTransient();
			diContainer.BindInstance<CompetitiveModeTreeBranch>(branch);
			return diContainer;
		}

		private IPresenterNode CreateNode<TPresenter>(string navigationName, params IPresenterNode[] children) where TPresenter : IPresenter
		{
			return new PresenterNode(navigationName, children, () => this._diContainer.Resolve<TPresenter>());
		}

		private IPresenterNode CreateNode<TPresenter>(string navigationName, DiContainer container, params IPresenterNode[] children) where TPresenter : IPresenter
		{
			return new PresenterNode(navigationName, children, () => container.Resolve<TPresenter>());
		}

		private Func<IPresenter> GetResolvePresenterFunction<T>() where T : IPresenter
		{
			return () => this._diContainer.Resolve<T>();
		}

		public IObservable<Unit> Initialize()
		{
			this.InitializeNodes();
			PresenterTreeSettings presenterTreeSettings = new PresenterTreeSettings();
			presenterTreeSettings.RootNode = this._startNode;
			presenterTreeSettings.ForcedRedirections = this.GetRedirections();
			return this._presenterTree.Initialize(presenterTreeSettings);
		}

		private Dictionary<IPresenterNode, IPresenterNode> GetRedirections()
		{
			return new Dictionary<IPresenterNode, IPresenterNode>
			{
				{
					this._startNode,
					this.MainMenuNode
				},
				{
					this._storeNode,
					this.StoreDriversNode
				},
				{
					this._inventoryNode,
					this.InventoryPortraitNode
				},
				{
					this._profileInventoryNode,
					this.ProfileInventoryAvatarsNode
				}
			};
		}

		public IObservable<Unit> NavigateToNode(IPresenterNode node)
		{
			return this._presenterTree.NavigateToNode(node);
		}

		public IObservable<Unit> NavigateBackwards()
		{
			return this._presenterTree.NavigateBackwards();
		}

		private readonly DiContainer _diContainer;

		private readonly IPresenterTree _presenterTree;

		private IPresenterNode _startNode;

		private IPresenterNode _storeNode;

		private IPresenterNode _inventoryNode;

		private IPresenterNode _profileInventoryNode;
	}
}
