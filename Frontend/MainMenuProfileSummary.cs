using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI.Objects;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Client.Matches;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.Utils;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using Hoplon.Serialization;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuProfileSummary : MainMenuProfileWindow
	{
		private IUiNavigationSubGroupHolder UiNavigationSubGroupHolder
		{
			get
			{
				return this._uiNavigationSubGroupHolder;
			}
		}

		private IUiNavigationRebuilder UiNavigationAxisSelectorRebuilder
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public override void OnLoading()
		{
			for (int i = 0; i < this.MasteryGuiComponentList.Length; i++)
			{
				MainMenuProfileSummary.MasteryGuiComponents masteryGuiComponents = this.MasteryGuiComponentList[i];
				masteryGuiComponents.EventTrigger.onHoverOver.Add(new EventDelegate(new EventDelegate.Callback(this.MasteryOnHoverOver)));
				masteryGuiComponents.EventTrigger.onHoverOut.Add(new EventDelegate(new EventDelegate.Callback(this.MasteryOnHoverOut)));
				masteryGuiComponents.BarSprite.width = this.MasteryMinSpriteBarSize;
				masteryGuiComponents.BarSprite.alpha = 0f;
				masteryGuiComponents.BarBorderSprite.alpha = 0f;
				Vector3 localPosition = masteryGuiComponents.BarSprite.transform.localPosition;
				localPosition.x = (float)(-(float)this.MasteryMinSpriteBarSize);
				masteryGuiComponents.BarSprite.transform.localPosition = localPosition;
			}
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<int>(Observable.Merge<int>(new IObservable<int>[]
			{
				this.UpdateBattlepassLevel(),
				this.UpdatePlayerLevel()
			})), this);
		}

		private IObservable<int> UpdateBattlepassLevel()
		{
			return Observable.Do<int>(this._observeBattlepassProgress.ObserveLevelChanged(), delegate(int level)
			{
				this.StatisticsGui.PlayerLevelLabel.text = (level + 1).ToString("0");
			});
		}

		private IObservable<int> UpdatePlayerLevel()
		{
			return Observable.Do<int>(this._observeLocalPlayerLevelChanged.GetAndObserve, delegate(int level)
			{
				this.StatisticsGui.PlayerLevelTotalLabel.text = (level + 1).ToString("0");
			});
		}

		private void MasteryOnHoverOver()
		{
			UIEventTrigger current = UIEventTrigger.current;
			for (int i = 0; i < this.MasteryGuiComponentList.Length; i++)
			{
				MainMenuProfileSummary.MasteryGuiComponents masteryGuiComponents = this.MasteryGuiComponentList[i];
				if (current == masteryGuiComponents.EventTrigger)
				{
					TooltipInfo tooltipInfo = new TooltipInfo(TooltipInfo.TooltipType.Normal, TooltipInfo.DescriptionSummaryType.None, PreferredDirection.Left, masteryGuiComponents.IconSprite.sprite2D, string.Empty, Language.Get(masteryGuiComponents.NameTranslationDraft, masteryGuiComponents.TranslationSheet), string.Empty, Language.Get(masteryGuiComponents.DescriptionTranslationDraft, masteryGuiComponents.TranslationSheet), string.Empty, string.Empty, string.Empty, masteryGuiComponents.TooltipPosition.position, string.Empty);
					if (!GameHubBehaviour.Hub.GuiScripts.TooltipController.IsVisible())
					{
						GameHubBehaviour.Hub.GuiScripts.TooltipController.ToggleOpenWindow(tooltipInfo);
					}
					break;
				}
			}
		}

		private void MasteryOnHoverOut()
		{
			GameHubBehaviour.Hub.GuiScripts.TooltipController.HideWindow();
		}

		public override void OnUnloading()
		{
			for (int i = 0; i < this.MasteryGuiComponentList.Length; i++)
			{
				MainMenuProfileSummary.MasteryGuiComponents masteryGuiComponents = this.MasteryGuiComponentList[i];
				masteryGuiComponents.EventTrigger.onHoverOver.Clear();
				masteryGuiComponents.EventTrigger.onHoverOut.Clear();
			}
			this.DisableMasteryTooltips();
			GameHubBehaviour.Hub.GuiScripts.TooltipController.HideWindow();
		}

		private void CreatePoolFromUpdateData()
		{
			if (this._creatingPool)
			{
				return;
			}
			this._creatingPool = true;
			GameHubBehaviour.Hub.StartCoroutine(GUIUtils.CreateGridPoolAsync(GameHubBehaviour.Hub, this.SumaryPlayersGrid, this.PlayerGridQuantity, delegate()
			{
				int num = this.StatisticsInfoList.Length;
				int num2 = num / 2;
				int quantity = num2 + num % 2;
				int rightGridCount = num2;
				GameHubBehaviour.Hub.StartCoroutine(GUIUtils.CreateGridPoolAsync(GameHubBehaviour.Hub, this.StatisticsGui.LeftGrid, quantity, delegate()
				{
					GameHubBehaviour.Hub.StartCoroutine(GUIUtils.CreateGridPoolAsync(this.StatisticsGui.RightGrid, this.StatisticsGui.LeftGrid.GetChild(0), rightGridCount, delegate()
					{
						this._creatingPool = false;
						this._poolCreated = true;
						this.UpdateData();
						this.UiNavigationAxisSelectorRebuilder.RebuildAndSelect();
					}));
				}));
			}));
		}

		public override void UpdateData()
		{
			if (!this._poolCreated)
			{
				this.CreatePoolFromUpdateData();
				return;
			}
			this._playerNameLabel.text = this._localPlayer.Player.Nickname;
			this.UpdateInfoCache();
			this.UpdateMasteryData();
			List<ItemTypeScriptableObject> mostPlayedCharacters = this.GetMostPlayedCharacters();
			this.SumaryPlayersGrid.hideInactive = false;
			List<Transform> childList = this.SumaryPlayersGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				childList[i].gameObject.SetActive(false);
			}
			for (int j = 0; j < mostPlayedCharacters.Count; j++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = mostPlayedCharacters[j];
				CharacterItemTypeComponent component = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
				MainMenuProfileCharacterCard component2 = childList[j].GetComponent<MainMenuProfileCharacterCard>();
				component2.SetCharacterName(component.GetCharacterLocalizedName());
				component2.SetCharacterSprite(component.AssetPrefix + "_icon_char_128");
				component2.SetButtonEventListener(component.CharacterId);
				component2.SetCharacterLevelProgressBarVisibility(true);
				CharacterBag characterBag;
				if (HudUtils.TryToGetCharacterBag(GameHubBehaviour.Hub, itemTypeScriptableObject.Id, out characterBag))
				{
					int levelForXP = GameHubBehaviour.Hub.SharedConfigs.CharacterProgression.GetLevelForXP(characterBag.Xp);
					component2.SetInfo((float)levelForXP, HudUtils.GetNormalizedLevelInfo(GameHubBehaviour.Hub.SharedConfigs.CharacterProgression, levelForXP, characterBag.Xp), component.Role);
				}
				else
				{
					Debug.Log(string.Format("(UpdateData) - CharacterBag not found for char typeId:[{0}]", itemTypeScriptableObject.Id));
					component2.SetInfo(0f, 0f, component.Role);
				}
				component2.SetCornerVisibility(false);
				childList[j].gameObject.SetActive(true);
			}
			this.SumaryPlayersGrid.hideInactive = true;
			this.SumaryPlayersGrid.Reposition();
			this.StatisticsGui.VictoriesLabel.text = this._infoCache.WinsCount.ToString("0");
			this.StatisticsGui.DefeatsLabel.text = this._infoCache.DefeatsCount.ToString("0");
			this.SetStatisticsInfo();
		}

		private void UpdateMasteryData()
		{
			float num = (float)Mathf.Max(this._infoCache.CarrierMatchesCount, this._infoCache.SupportMatchesCount);
			num = Mathf.Max(num, (float)this._infoCache.TacklerMatchesCount);
			for (int i = 0; i < this.MasteryGuiComponentList.Length; i++)
			{
				MainMenuProfileSummary.MasteryGuiComponents masteryGuiComponents = this.MasteryGuiComponentList[i];
				float num2 = (float)this._infoCache.CarrierMatchesCount;
				if (masteryGuiComponents.RoleKind == null)
				{
					num2 = (float)this._infoCache.SupportMatchesCount;
				}
				else if (masteryGuiComponents.RoleKind == 2)
				{
					num2 = (float)this._infoCache.TacklerMatchesCount;
				}
				masteryGuiComponents.MatchesLabel.text = num2.ToString();
				Vector3 localPosition = masteryGuiComponents.BarSprite.transform.localPosition;
				if (num2 < 0.001f)
				{
					masteryGuiComponents.BarSprite.width = this.MasteryMinSpriteBarSize;
					masteryGuiComponents.BarSprite.alpha = 0f;
					masteryGuiComponents.BarBorderSprite.alpha = 0f;
					localPosition.x = (float)(-(float)this.MasteryMinSpriteBarSize);
				}
				else
				{
					float num3 = num2 / num;
					float num4 = (float)(this.MasteryMaxSpriteBarSize - this.MasteryMinSpriteBarSize);
					float num5 = num4 * num3;
					masteryGuiComponents.BarSprite.width = this.MasteryMinSpriteBarSize + Mathf.RoundToInt(num5);
					masteryGuiComponents.BarSprite.alpha = 1f;
					masteryGuiComponents.BarBorderSprite.alpha = 1f;
					localPosition.x = 0f;
				}
				masteryGuiComponents.BarSprite.transform.localPosition = localPosition;
			}
		}

		private void SetStatisticsInfo()
		{
			List<Transform> list = this.ClearGrid(this.StatisticsGui.LeftGrid);
			List<Transform> list2 = this.ClearGrid(this.StatisticsGui.RightGrid);
			int num = 0;
			int num2 = 0;
			while (num < this.StatisticsInfoList.Length && num2 < list.Count)
			{
				MainMenuProfileStatisticsSlot component = list[num2].GetComponent<MainMenuProfileStatisticsSlot>();
				component.SetInfo(this.GetStatisticsSlotInfo(this.StatisticsInfoList[num]));
				list[num2].gameObject.SetActive(true);
				num2++;
				num++;
			}
			int num3 = 0;
			while (num < this.StatisticsInfoList.Length && num3 < list2.Count)
			{
				MainMenuProfileStatisticsSlot component2 = list2[num3].GetComponent<MainMenuProfileStatisticsSlot>();
				component2.SetInfo(this.GetStatisticsSlotInfo(this.StatisticsInfoList[num]));
				list2[num3].gameObject.SetActive(true);
				num3++;
				num++;
			}
			this.StatisticsGui.LeftGrid.Reposition();
			this.StatisticsGui.RightGrid.Reposition();
		}

		private MainMenuProfileStatisticsSlot.StatisticsSlotInfo GetStatisticsSlotInfo(MainMenuProfileSummary.StatisticsInfo statisticsInfo)
		{
			MainMenuProfileStatisticsSlot.StatisticsSlotInfo result = default(MainMenuProfileStatisticsSlot.StatisticsSlotInfo);
			result.IconSprite = statisticsInfo.IconSprite;
			result.Title = Language.Get(statisticsInfo.TranslationDraft, statisticsInfo.TranslationSheet);
			switch (statisticsInfo.Type)
			{
			case MainMenuProfileSummary.StatisticsInfoType.Matches:
				result.Total = this._infoCache.MatchesCount;
				result.Average = (float)this._infoCache.MatchesCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.Wins:
				result.Total = this._infoCache.WinsCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.Defeats:
				result.Total = this._infoCache.DefeatsCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.Deaths:
				result.Total = this._infoCache.DeathsCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.SupportMatches:
				result.Total = this._infoCache.SupportMatchesCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.CarrierMatches:
				result.Total = this._infoCache.CarrierMatchesCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.TacklerMatches:
				result.Total = this._infoCache.TacklerMatchesCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.BombStolen:
				result.Total = this._infoCache.BombStolenCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.BombLost:
				result.Total = this._infoCache.BombLostCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.BombDelivered:
				result.Total = this._infoCache.BombDeliveredCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.TotalDamage:
				result.Total = this._infoCache.TotalDamage;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.TotalRepair:
				result.Total = this._infoCache.TotalRepair;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.TravelledDistance:
				result.Total = this._infoCache.TravelledDistance;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.SpeedBoost:
				result.Total = this._infoCache.SpeedBoostCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.ScrapCollected:
				result.Total = this._infoCache.ScrapCollectedCount;
				break;
			case MainMenuProfileSummary.StatisticsInfoType.Kills:
				result.Total = this._infoCache.KillsCount;
				break;
			}
			result.Average = ((statisticsInfo.Type != MainMenuProfileSummary.StatisticsInfoType.Matches) ? result.GetAverage(this._infoCache.MatchesCount) : ((float)this._infoCache.MatchesCount));
			return result;
		}

		private List<Transform> ClearGrid(UIGrid grid)
		{
			bool hideInactive = this.SumaryPlayersGrid.hideInactive;
			this.SumaryPlayersGrid.hideInactive = false;
			List<Transform> childList = grid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				childList[i].gameObject.SetActive(false);
			}
			this.SumaryPlayersGrid.hideInactive = hideInactive;
			return childList;
		}

		private void UpdateInfoCache()
		{
			this._infoCache = default(MainMenuProfileSummary.SummaryInfoCache);
			PlayerMatchCharacterStats[] all = this._localPlayerMatchCharacterStatsProvider.GetAll();
			PlayerMatchStats playerMatchStats = this._calculatePlayerMatchStats.Get(all);
			this._infoCache.BombStolenCount = playerMatchStats.BombStolenCount;
			this._infoCache.BombLostCount = playerMatchStats.BombLostCount;
			this._infoCache.BombDeliveredCount = playerMatchStats.BombDeliveredCount;
			this._infoCache.MatchesCount = playerMatchStats.MatchesCount;
			this._infoCache.WinsCount = playerMatchStats.WinsCount;
			this._infoCache.DefeatsCount = playerMatchStats.DefeatsCount;
			this._infoCache.KillsCount = playerMatchStats.KillsCount;
			this._infoCache.DeathsCount = playerMatchStats.DeathsCount;
			this._infoCache.TotalDamage = playerMatchStats.TotalDamage;
			this._infoCache.TotalRepair = playerMatchStats.TotalRepair;
			this._infoCache.TravelledDistance = playerMatchStats.TravelledDistance;
			this._infoCache.SpeedBoostCount = playerMatchStats.SpeedBoostCount;
			this._infoCache.ScrapCollectedCount = playerMatchStats.ScrapCollectedCount;
			for (int i = 0; i < GameHubBehaviour.Hub.User.Characters.Length; i++)
			{
				Character character = GameHubBehaviour.Hub.User.Characters[i];
				CharacterBag characterBag = (CharacterBag)((JsonSerializeable<!0>)character.Bag);
				if (!GameHubBehaviour.Hub.InventoryColletion.AllItemTypes.ContainsKey(characterBag.CharacterId))
				{
					MainMenuProfileSummary.Log.ErrorFormat("Skipping character {0}, data not found in collection. Is this a unreleased character? The QA database must be reset after testing a new character.", new object[]
					{
						characterBag.CharacterId
					});
				}
				else
				{
					ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[characterBag.CharacterId];
					CharacterItemTypeComponent component = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
					DriverRoleKind role = component.Role;
					if (role != 1)
					{
						if (role != null)
						{
							if (role == 2)
							{
								this._infoCache.TacklerMatchesCount = this._infoCache.TacklerMatchesCount + characterBag.MatchesCount;
							}
						}
						else
						{
							this._infoCache.SupportMatchesCount = this._infoCache.SupportMatchesCount + characterBag.MatchesCount;
						}
					}
					else
					{
						this._infoCache.CarrierMatchesCount = this._infoCache.CarrierMatchesCount + characterBag.MatchesCount;
					}
				}
			}
		}

		public override void SetWindowVisibility(bool visible)
		{
			base.StopCoroutineSafe(this.disableCoroutine);
			if (visible)
			{
				base.gameObject.SetActive(true);
				this.ScreenAlphaAnimation.Play("profileGeneralIn");
				this.EnableMasteryTooltips();
			}
			else if (base.gameObject.activeInHierarchy)
			{
				this.ScreenAlphaAnimation.Play("profileGeneralOut");
				this.disableCoroutine = base.StartCoroutine(GUIUtils.WaitAndDisable(this.ScreenAlphaAnimation.clip.length, base.gameObject));
			}
			if (!visible)
			{
				this.DisableMasteryTooltips();
				GameHubBehaviour.Hub.GuiScripts.TooltipController.HideWindow();
			}
			this.SetUiNavigationFocus(visible);
		}

		private void SetUiNavigationFocus(bool focused)
		{
			if (focused)
			{
				this.UiNavigationSubGroupHolder.SubGroupFocusGet();
			}
			else
			{
				this.UiNavigationSubGroupHolder.SubGroupFocusRelease();
			}
		}

		public override void OnPreBackToMainMenu()
		{
			this.DisableMasteryTooltips();
			GameHubBehaviour.Hub.GuiScripts.TooltipController.HideWindow();
		}

		public override void OnBackToMainMenu()
		{
			this.SetWindowVisibility(false);
		}

		private List<ItemTypeScriptableObject> GetMostPlayedCharacters()
		{
			List<CharacterBag> list = new List<CharacterBag>(GameHubBehaviour.Hub.User.Characters.Length);
			List<ItemTypeScriptableObject> list2 = new List<ItemTypeScriptableObject>(this.PlayerGridQuantity);
			for (int i = 0; i < GameHubBehaviour.Hub.User.Characters.Length; i++)
			{
				CharacterBag item = (CharacterBag)((JsonSerializeable<!0>)GameHubBehaviour.Hub.User.Characters[i].Bag);
				list.Add(item);
			}
			List<CharacterBag> list3 = list;
			if (MainMenuProfileSummary.<>f__mg$cache0 == null)
			{
				MainMenuProfileSummary.<>f__mg$cache0 = new Comparison<CharacterBag>(MainMenuProfileSummary.SortCharacterBagByMatchCount);
			}
			list3.Sort(MainMenuProfileSummary.<>f__mg$cache0);
			for (int j = 0; j < list.Count; j++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject;
				if (GameHubBehaviour.Hub.InventoryColletion.AllItemTypes.TryGetValue(list[j].CharacterId, out itemTypeScriptableObject))
				{
					if (!(itemTypeScriptableObject == null))
					{
						if (list2.Contains(itemTypeScriptableObject))
						{
							CharacterItemTypeComponent component = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
							MainMenuProfileSummary.Log.WarnFormat("GetMostPlayedCharacters. Duplicated char info: CharacterId[{0}].", new object[]
							{
								component.CharacterId
							});
						}
						else
						{
							list2.Add(itemTypeScriptableObject);
							if (list2.Count >= this.PlayerGridQuantity)
							{
								break;
							}
						}
					}
				}
			}
			return list2;
		}

		private static int SortCharacterBagByMatchCount(CharacterBag a, CharacterBag b)
		{
			return -a.MatchesCount.CompareTo(b.MatchesCount);
		}

		public void OnCharacterClick(int id)
		{
			MainMenuProfileController.ProfileWindow profileWindow = this.MainMenuProfileController.ShowWindow(MainMenuProfileController.ProfileWindowType.Machines);
			profileWindow.LeftButton.Set(true, true);
			MainMenuProfileMachines mainMenuProfileMachines = (MainMenuProfileMachines)profileWindow.MainMenuProfileWindow;
			mainMenuProfileMachines.OnCharacterClickFromProfileSummary(id);
		}

		private void EnableMasteryTooltips()
		{
			this.SetMasteryIconsCollidersEnableState(true);
		}

		private void DisableMasteryTooltips()
		{
			this.SetMasteryIconsCollidersEnableState(false);
		}

		private void SetMasteryIconsCollidersEnableState(bool isEnabled)
		{
			for (int i = 0; i < this.MasteryGuiComponentList.Length; i++)
			{
				this.MasteryGuiComponentList[i].IconSprite.GetComponent<BoxCollider>().enabled = isEnabled;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MainMenuProfileSummary));

		[SerializeField]
		private UILabel _playerNameLabel;

		public int PlayerGridQuantity = 3;

		public UIGrid SumaryPlayersGrid;

		[Header("[Mastery]")]
		public int MasteryMinSpriteBarSize = 66;

		public int MasteryMaxSpriteBarSize = 400;

		public MainMenuProfileSummary.MasteryGuiComponents[] MasteryGuiComponentList;

		[Inject]
		private ILocalPlayerStorage _localPlayer;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		[Inject]
		private ILocalPlayerMatchCharacterStatsProvider _localPlayerMatchCharacterStatsProvider;

		[Inject]
		private ICalculatePlayerMatchStats _calculatePlayerMatchStats;

		[Inject]
		private IObserveBattlepassProgress _observeBattlepassProgress;

		[Inject]
		private IObserveLocalPlayerLevelChanged _observeLocalPlayerLevelChanged;

		private bool _creatingPool;

		private bool _poolCreated;

		[SerializeField]
		protected MainMenuProfileSummary.StatisticsGuiComponents StatisticsGui;

		private MainMenuProfileSummary.SummaryInfoCache _infoCache;

		[SerializeField]
		protected MainMenuProfileSummary.StatisticsInfo[] StatisticsInfoList;

		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		private Coroutine disableCoroutine;

		[CompilerGenerated]
		private static Comparison<CharacterBag> <>f__mg$cache0;

		[Serializable]
		public struct MasteryGuiComponents
		{
			public DriverRoleKind RoleKind;

			public UI2DSprite IconSprite;

			public UI2DSprite BarSprite;

			public UI2DSprite BarBorderSprite;

			public UILabel MatchesLabel;

			public UIEventTrigger EventTrigger;

			public Transform TooltipPosition;

			public TranslationSheets TranslationSheet;

			public string NameTranslationDraft;

			public string DescriptionTranslationDraft;
		}

		[Serializable]
		protected struct StatisticsGuiComponents
		{
			public UIGrid LeftGrid;

			public UIGrid RightGrid;

			public UIScrollView ScrollView;

			public UIScrollBar ScrollBar;

			public UILabel VictoriesLabel;

			public UILabel DefeatsLabel;

			public UILabel PlayerLevelLabel;

			public UILabel PlayerLevelTotalLabel;
		}

		private struct SummaryInfoCache
		{
			public int MatchesCount;

			public int WinsCount;

			public int DefeatsCount;

			public int KillsCount;

			public int DeathsCount;

			public int SupportMatchesCount;

			public int CarrierMatchesCount;

			public int TacklerMatchesCount;

			public int BombStolenCount;

			public int BombLostCount;

			public int BombDeliveredCount;

			public int TotalDamage;

			public int TotalRepair;

			public int TravelledDistance;

			public int SpeedBoostCount;

			public int ScrapCollectedCount;
		}

		protected enum StatisticsInfoType
		{
			Matches,
			Wins,
			Defeats,
			Deaths,
			SupportMatches,
			CarrierMatches,
			TacklerMatches,
			BombStolen,
			BombLost,
			BombDelivered,
			TotalDamage,
			TotalRepair,
			TravelledDistance,
			SpeedBoost,
			ScrapCollected,
			Kills
		}

		[Serializable]
		protected struct StatisticsInfo
		{
			public Sprite IconSprite;

			public MainMenuProfileSummary.StatisticsInfoType Type;

			public TranslationSheets TranslationSheet;

			public string TranslationDraft;
		}
	}
}
