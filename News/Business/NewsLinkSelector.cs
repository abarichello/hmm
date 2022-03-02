using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Battlepass;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Inventory.Business;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.OpenUrl;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Presenting.Navigation;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Utils;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.News.Business
{
	public class NewsLinkSelector : INewsLinkSelector
	{
		public NewsLinkSelector(ILogger<NewsLinkSelector> logger, IMainMenuPresenterTree mainMenuPresenterTree, IGetItemFromCollection getItemFromCollection, IShopGui shopGui, IBattlepassComponent battlepassComponent, IClientBILogger clientBiLogger, IClientShopBILogger clientShopBiLogger, UserInfo userInfo, IGetUGCRestrictionIsEnabled getUgcRestrictionIsEnabled, IOpenUrlUgcRestricted openUrlUgcRestricted)
		{
			this._logger = logger;
			this._mainMenuPresenterTree = mainMenuPresenterTree;
			this._getItemFromCollection = getItemFromCollection;
			this._shopGui = shopGui;
			this._battlepassComponent = battlepassComponent;
			this._clientBiLogger = clientBiLogger;
			this._clientShopBiLogger = clientShopBiLogger;
			this._userInfo = userInfo;
			this._getUgcRestrictionIsEnabled = getUgcRestrictionIsEnabled;
			this._openUrlUgcRestricted = openUrlUgcRestricted;
			this._newsHmmInvoker = new NewsHmmInvoker();
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Battlepass, new Action<Guid>(this.SelectBattlepass));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Skins, new Action<Guid>(this.SelectSkins));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Boosters, new Action<Guid>(this.SelectBoosters));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Effects, new Action<Guid>(this.SelectEffects));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Sprays, new Action<Guid>(this.SelectSprays));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Cash, new Action<Guid>(this.SelectCash));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Emotes, new Action<Guid>(this.SelectEmotes));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Tournament, new Action<Guid>(this.SelectTournament));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Training, new Action<Guid>(this.SelectTraining));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Competitive, new Action<Guid>(this.SelectCompetitive));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Drivers, new Action<Guid>(this.SelectDrivers));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Play, new Action<Guid>(this.SelectPlay));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Teams, new Action<Guid>(this.SelectTeams));
			this._newsHmmInvoker.Add(NewsHmmSelectionType.Teammates, new Action<Guid>(this.SelectTeammates));
		}

		public void Execute(string link, NewsCardBiPosition biPosition)
		{
			this._biPosition = biPosition;
			if (string.IsNullOrEmpty(link))
			{
				return;
			}
			NewsHmmSelectionType selectionType;
			Guid itemTypeId;
			bool flag = this.IsHmmItemTypeSelection(link, out selectionType, out itemTypeId);
			if (flag)
			{
				this._newsHmmInvoker.Run(selectionType, itemTypeId);
			}
			else
			{
				OpenUrlUtils.OpenUrl(link);
			}
			this.SendBiLog(link, flag);
		}

		private bool IsHmmItemTypeSelection(string link, out NewsHmmSelectionType selectionType, out Guid itemTypeId)
		{
			selectionType = NewsHmmSelectionType.Cash;
			itemTypeId = Guid.Empty;
			string text = link.ToLower();
			if (!text.StartsWith("hmm://"))
			{
				return false;
			}
			text = text.Remove(0, "hmm://".Length);
			string[] array = text.Split(new char[]
			{
				'/'
			});
			if (array.Length == 0)
			{
				this._logger.ErrorFormat("Hmm ItemType format error. Link:{0}", new object[]
				{
					link
				});
				return true;
			}
			bool result;
			try
			{
				selectionType = (NewsHmmSelectionType)Enum.Parse(typeof(NewsHmmSelectionType), array[0], true);
				if (array.Length == 1)
				{
					result = true;
				}
				else if (string.IsNullOrEmpty(array[1]))
				{
					result = true;
				}
				else
				{
					itemTypeId = new Guid(array[1]);
					result = true;
				}
			}
			catch (Exception ex)
			{
				this._logger.WarnFormat("Hmm ItemType selection exception for link [{0}]. Moving to fallback. Exception: {1}", new object[]
				{
					text,
					ex
				});
				result = true;
			}
			return result;
		}

		private void SelectBattlepass(Guid itemTypeId)
		{
			this.NavigateToNode(this._mainMenuPresenterTree.BattlepassNode, delegate
			{
				this._battlepassComponent.ShowMetalpassPremiumShopWindow();
			});
		}

		private void SelectSkins(Guid itemTypeId)
		{
			this._clientShopBiLogger.Log(this.GetClientShopBiOrigin(), 1);
			this.NavigateToNode(this._mainMenuPresenterTree.StoreSkinsNode, delegate
			{
				this.CallIfValidItemTypeId(itemTypeId, delegate(IItemType itemType)
				{
					this._shopGui.ShowSkinDetails(itemType);
				}, new Guid[]
				{
					InventoryMapper.SkinsCategoryGuid
				});
			});
		}

		private void SelectBoosters(Guid itemTypeId)
		{
			this._clientShopBiLogger.Log(this.GetClientShopBiOrigin(), 5);
			this.NavigateToNode(this._mainMenuPresenterTree.StoreBoostersNode, null);
		}

		private void SelectEffects(Guid itemTypeId)
		{
			this._clientShopBiLogger.Log(this.GetClientShopBiOrigin(), 2);
			this.NavigateToNode(this._mainMenuPresenterTree.StoreEffectsNode, delegate
			{
				this.CallIfValidItemTypeId(itemTypeId, delegate(IItemType itemType)
				{
					this._shopGui.ShowItemTypeDetails(itemType);
				}, new Guid[]
				{
					InventoryMapper.VfxKillCategoryGuid,
					InventoryMapper.VfxRespawnCategoryGuid,
					InventoryMapper.VfxScoreCategoryGuid,
					InventoryMapper.VfxTakeOffCategoryGuid
				});
			});
		}

		private void SelectSprays(Guid itemTypeId)
		{
			this._clientShopBiLogger.Log(this.GetClientShopBiOrigin(), 4);
			this.NavigateToNode(this._mainMenuPresenterTree.StoreSpraysNode, delegate
			{
				this.CallIfValidItemTypeId(itemTypeId, delegate(IItemType itemType)
				{
					this._shopGui.ShowItemTypeDetails(itemType);
				}, new Guid[]
				{
					InventoryMapper.SprayCategoryGuid
				});
			});
		}

		private void SelectCash(Guid itemTypeId)
		{
			this._clientShopBiLogger.Log(this.GetClientShopBiOrigin(), 6);
			this.NavigateToNode(this._mainMenuPresenterTree.StoreCashNode, null);
		}

		private void SelectEmotes(Guid itemTypeId)
		{
			this._clientShopBiLogger.Log(this.GetClientShopBiOrigin(), 3);
			this.NavigateToNode(this._mainMenuPresenterTree.StoreEmotesNode, delegate
			{
				this.CallIfValidItemTypeId(itemTypeId, delegate(IItemType itemType)
				{
					this._shopGui.ShowItemTypeDetails(itemType);
				}, new Guid[]
				{
					InventoryMapper.EmoteCategoryGuid
				});
			});
		}

		private void SelectTournament(Guid itemTypeId)
		{
			this.NavigateToNode(this._mainMenuPresenterTree.TournamentListNode, null);
		}

		private void SelectTraining(Guid itemTypeId)
		{
			this.NavigateToNode(this._mainMenuPresenterTree.MainMenuTrainingSelectionNode, null);
		}

		private void SelectCompetitive(Guid itemTypeId)
		{
			this.NavigateToNode(this._mainMenuPresenterTree.MainMenuCompetitiveModeNode, null);
		}

		private void SelectDrivers(Guid itemTypeId)
		{
			this._clientShopBiLogger.Log(this.GetClientShopBiOrigin(), 0);
			this.NavigateToNode(this._mainMenuPresenterTree.StoreDriversNode, null);
		}

		private void SelectPlay(Guid itemTypeId)
		{
			this.NavigateToNode(this._mainMenuPresenterTree.PlayModesNode, null);
		}

		private void SelectTeams(Guid itemTypeId)
		{
			ObservableExtensions.Subscribe<bool>(this._openUrlUgcRestricted.OpenUrlAfterRestrictionCheck(7));
		}

		private void SelectTeammates(Guid itemTypeId)
		{
			ObservableExtensions.Subscribe<bool>(this._openUrlUgcRestricted.OpenUrlAfterRestrictionCheck(8));
		}

		private void NavigateToNode(IPresenterNode presenterNode, Action onNavigationDone = null)
		{
			ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(presenterNode), delegate(Unit _)
			{
				if (onNavigationDone != null)
				{
					onNavigationDone();
				}
			});
		}

		private void CallIfValidItemTypeId(Guid itemTypeId, Action<IItemType> onValidItemType, params Guid[] categoryGuid)
		{
			if (itemTypeId == Guid.Empty)
			{
				return;
			}
			IItemType itemType = this._getItemFromCollection.Get(itemTypeId);
			if (this.AssertItemIsValidStoreItemDetail(itemType, categoryGuid))
			{
				onValidItemType(itemType);
			}
		}

		private bool AssertItemIsValidStoreItemDetail(IItemType itemToCheck, params Guid[] categoryGuid)
		{
			for (int i = 0; i < categoryGuid.Length; i++)
			{
				if (itemToCheck.ItemCategoryId == categoryGuid[i])
				{
					ShopItemTypeComponent component = itemToCheck.GetComponent<ShopItemTypeComponent>();
					return component != null;
				}
			}
			this._logger.WarnFormat("Invalid category id {0} for item {1}", new object[]
			{
				categoryGuid,
				itemToCheck.Name
			});
			return false;
		}

		private void SendBiLog(string link, bool isLinkItemTypeId)
		{
			string text = (!isLinkItemTypeId) ? "External" : "Internal";
			string text2 = string.Format("UniversalId:{0} NewsLink:{1} LanguageCode:{2} Position:{3} UrlType:{4}", new object[]
			{
				this._userInfo.UniversalId,
				link,
				Language.CurrentLanguage,
				this._biPosition,
				text
			});
			this._clientBiLogger.BILogClientMsg(94, text2, false);
		}

		private ClientShopBiOrigin GetClientShopBiOrigin()
		{
			NewsCardBiPosition biPosition = this._biPosition;
			if (biPosition == NewsCardBiPosition.LowerCenterCard)
			{
				return 1;
			}
			if (biPosition != NewsCardBiPosition.LowerLeftCard)
			{
				return 3;
			}
			return 2;
		}

		private const string LinkItemTypeIdPrefix = "hmm://";

		private readonly ILogger<NewsLinkSelector> _logger;

		private readonly IMainMenuPresenterTree _mainMenuPresenterTree;

		private readonly IGetItemFromCollection _getItemFromCollection;

		private readonly IShopGui _shopGui;

		private readonly IBattlepassComponent _battlepassComponent;

		private readonly IClientBILogger _clientBiLogger;

		private readonly IClientShopBILogger _clientShopBiLogger;

		private readonly UserInfo _userInfo;

		private readonly IGetUGCRestrictionIsEnabled _getUgcRestrictionIsEnabled;

		private readonly IOpenUrlUgcRestricted _openUrlUgcRestricted;

		private readonly IUGCRestrictionDialogPresenter _ugcRestrictionDialogPresenter;

		private readonly NewsHmmInvoker _newsHmmInvoker;

		private NewsCardBiPosition _biPosition;
	}
}
