using System;
using System.Collections.Generic;
using System.Globalization;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches.DataTransferObjects;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.Utils;
using Hoplon.Input.UiNavigation;
using Hoplon.Serialization;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuProfileMatches : MainMenuProfileWindow
	{
		private IUiNavigationSubGroupHolder UiNavigationSubGroupHolder
		{
			get
			{
				return this._uiNavigationSubGroupHolder;
			}
		}

		public override void OnLoading()
		{
			this._dataLoaded = false;
			UIGrid grid = this.Grid;
			grid.onCustomSort = (Comparison<Transform>)Delegate.Combine(grid.onCustomSort, new Comparison<Transform>(this.SortItemsList));
		}

		private void CreatePoolFromUpdateData()
		{
			if (this._creatingPool)
			{
				return;
			}
			this._creatingPool = true;
			Transform child = this.Grid.GetChild(0);
			Transform defeatReference = this.Grid.GetChild(1);
			child.gameObject.SetActive(false);
			defeatReference.gameObject.SetActive(false);
			GameHubBehaviour.Hub.StartCoroutine(GUIUtils.CreateGridPoolAsync(this.Grid, child, this.GridPoolQuantity - 1, delegate()
			{
				this.OnCreateGridPoolVictory(defeatReference);
			}));
		}

		private void OnCreateGridPoolVictory(Transform defeatReference)
		{
			GameHubBehaviour.Hub.StartCoroutine(GUIUtils.CreateGridPoolAsync(this.Grid, defeatReference, this.GridPoolQuantity - 1, new GUIUtils.GridPoolAsyncCompleteCallback(this.OnCreateGridPoolDefeat)));
		}

		private void OnCreateGridPoolDefeat()
		{
			this.Grid.hideInactive = false;
			List<Transform> childList = this.Grid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				childList[i].gameObject.SetActive(false);
			}
			this.Grid.hideInactive = true;
			this._creatingPool = false;
			this._poolCreated = true;
			this.UpdateData();
		}

		public override void OnUnloading()
		{
			this.Grid.onCustomSort = null;
		}

		public override void UpdateData()
		{
			if (!this._poolCreated)
			{
				this.CenterLineGameObject.SetActive(false);
				this.InfoGroupGameObject.SetActive(true);
				this.LoadingFeedbackGameObject.SetActive(true);
				this.InfoGroupLabel.text = Language.Get("PROFILE_MATCHES_LOADING", TranslationContext.Profile);
				this.CreatePoolFromUpdateData();
				return;
			}
			this.LoadHistoryData();
		}

		public override void SetWindowVisibility(bool visible)
		{
			base.StopCoroutineSafe(this.disableCoroutine);
			if (visible)
			{
				base.gameObject.SetActive(true);
				this.ScreenAlphaAnimation.Play("profileMatchesIn");
				if (this._dataLoaded)
				{
					base.StartCoroutine(GUIUtils.HackScrollReposition(this.ScrollView, this.ScrollBar, this.Grid));
				}
			}
			else if (base.gameObject.activeInHierarchy)
			{
				this.ScreenAlphaAnimation.Play("profileMatchesOut");
				this.disableCoroutine = base.StartCoroutine(GUIUtils.WaitAndDisable(this.ScreenAlphaAnimation.clip.length, base.gameObject));
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
		}

		public override void OnBackToMainMenu()
		{
			this.SetWindowVisibility(false);
		}

		private void LoadHistoryData()
		{
			if (this._dataLoaded)
			{
				return;
			}
			Inventory inventoryByKind = GameHubBehaviour.Hub.User.Inventory.GetInventoryByKind(5);
			if (inventoryByKind == null)
			{
				MainMenuProfileMatches.Log.ErrorFormat("Could not find Inventory for MatchHistory Kind. PlayerId: {0}", new object[]
				{
					GameHubBehaviour.Hub.User.PlayerSF.Id
				});
				return;
			}
			GameHubBehaviour.Hub.ClientApi.inventory.GetNewestItems(this, inventoryByKind.Id, this.GridPoolQuantity, new SwordfishClientApi.ParameterizedCallback<Item[]>(this.ClientApiInventoryOnGetMatchHistorySuccess), new SwordfishClientApi.ErrorCallback(this.ClientApiInventoryOnGetMatchHistoryError));
		}

		private void ClientApiInventoryOnGetMatchHistorySuccess(object state, Item[] items)
		{
			if (items.Length > this.GridPoolQuantity)
			{
				MainMenuProfileMatches.Log.ErrorFormat("ClientApiInventoryOnGetMatchHistorySuccess - Item list length [{0}] > Grid pool [{1}]", new object[]
				{
					items.Length,
					this.GridPoolQuantity
				});
				Array.Resize<Item>(ref items, this.GridPoolQuantity);
			}
			MainMenuProfileMatches.Log.Debug("ClientApiInventoryOnGetMatchHistorySuccess - Count = " + items.Length);
			this._dataLoaded = true;
			this.Grid.hideInactive = false;
			List<Transform> childList = this.Grid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				childList[i].gameObject.SetActive(false);
			}
			this.Grid.hideInactive = true;
			for (int j = 0; j < items.Length; j++)
			{
				Item item = items[j];
				MatchHistoryItemBag matchHistoryItemBag = (MatchHistoryItemBag)((JsonSerializeable<!0>)item.Bag);
				MatchKind gameMode = matchHistoryItemBag.GameMode;
				if (gameMode != 2 && gameMode != 6)
				{
					IGameArenaInfo arenaByIndex = GameHubBehaviour.Hub.ArenaConfig.GetArenaByIndex(matchHistoryItemBag.ArenaIndex);
					IItemType itemType;
					if (arenaByIndex == null)
					{
						MainMenuProfileMatches.Log.WarnFormat("Arena index out of range: {0}", new object[]
						{
							matchHistoryItemBag.ArenaIndex
						});
					}
					else if (!GameHubBehaviour.Hub.InventoryColletion.AllCharactersByCharacterId.TryGetValue(matchHistoryItemBag.CharacterId, out itemType))
					{
						Debug.Assert(false, string.Format("Character from MatchHistoryItemBag not found! Id:[{0}]", matchHistoryItemBag.CharacterId), Debug.TargetTeam.All);
					}
					else
					{
						MainMenuProfileMatches.Log.DebugFormat("Match record {0}: {1}", new object[]
						{
							j,
							matchHistoryItemBag
						});
						for (int k = 0; k < childList.Count; k++)
						{
							MainMenuProfileMatchesSlot component = childList[k].GetComponent<MainMenuProfileMatchesSlot>();
							MatchData.MatchResult matchResult = (MatchData.MatchResult)matchHistoryItemBag.MatchResult;
							if (!component.gameObject.activeSelf && component.GetResultType() == matchResult)
							{
								string carSkinSpriteName = HudUtils.GetCarSkinSpriteName(GameHubBehaviour.Hub.InventoryColletion, itemType.Id, new Guid(matchHistoryItemBag.SkinId));
								CultureInfo systemCulture = CultureUtils.GetSystemCulture();
								DateTime matchDateTime = DateTime.Parse(string.Format("{0} GMT", matchHistoryItemBag.Date));
								RewardsInfo.Medals bestPerformanceMedal = (RewardsInfo.Medals)matchHistoryItemBag.BestPerformanceMedal;
								string key = "PROFILE_MATCHES_RESULT_VICTORY";
								Color resultColor = this.ResultLabelVictoryColor;
								if (matchHistoryItemBag.Abandoned)
								{
									key = "PROFILE_MATCHES_RESULT_ABANDON";
									resultColor = this.ResultLabelAbandonColor;
								}
								else if (matchResult == MatchData.MatchResult.Defeat)
								{
									key = "PROFILE_MATCHES_RESULT_DEFEAT";
									resultColor = this.ResultLabelDefeatColor;
								}
								string key2;
								string gameModeIconSpriteName;
								switch (matchHistoryItemBag.GameMode)
								{
								case 0:
								case 7:
									key2 = "PROFILE_PVP";
									gameModeIconSpriteName = "pvp_icon 1";
									break;
								case 1:
									key2 = "PROFILE_PVE";
									gameModeIconSpriteName = "pve_icon";
									break;
								case 2:
								case 6:
									goto IL_307;
								case 3:
									key2 = "PROFILE_RANKED";
									gameModeIconSpriteName = "pve_icon";
									break;
								case 4:
									key2 = "PROFILE_CUSTOM";
									gameModeIconSpriteName = "custom_icon";
									break;
								case 5:
									key2 = "PROFILE_TOURNAMENT";
									gameModeIconSpriteName = "pve_icon";
									break;
								default:
									goto IL_307;
								}
								IL_33E:
								CharacterItemTypeComponent component2 = itemType.GetComponent<CharacterItemTypeComponent>();
								MainMenuProfileMatchesSlot.MatchSlotInfo info = new MainMenuProfileMatchesSlot.MatchSlotInfo
								{
									CharacterIconSpriteName = carSkinSpriteName,
									CharacterNameText = component2.GetCharacterLocalizedName(),
									HasPerformance = (bestPerformanceMedal != RewardsInfo.Medals.None),
									PerformanceIconSpriteName = this.GetPerformanceInfo(bestPerformanceMedal),
									GameModeIconSpriteName = gameModeIconSpriteName,
									GameModeName = Language.Get(key2, TranslationContext.Profile),
									DateText = matchDateTime.ToString(systemCulture.DateTimeFormat.ShortDatePattern),
									TimeText = matchDateTime.ToString("HH:mm"),
									MatchDateTime = matchDateTime,
									ResultText = Language.Get(key, TranslationContext.Profile),
									ResultColor = resultColor,
									ArenaName = Language.Get(arenaByIndex.DraftName, TranslationContext.MainMenuGui)
								};
								component.SetInfo(info);
								component.gameObject.SetActive(true);
								this.CenterLineGameObject.SetActive(true);
								break;
								IL_307:
								key2 = "PROFILE_PVE";
								gameModeIconSpriteName = "pve_icon";
								MainMenuProfileMatches.Log.WarnFormat("Unknown bag.GameMode: {0}", new object[]
								{
									matchHistoryItemBag.GameMode
								});
								goto IL_33E;
							}
						}
					}
				}
			}
			bool activeSelf = this.CenterLineGameObject.activeSelf;
			this.InfoGroupGameObject.SetActive(!activeSelf);
			this.LoadingFeedbackGameObject.SetActive(false);
			if (!activeSelf)
			{
				this.InfoGroupLabel.text = Language.Get("PROFILE_MATCHES_FIRST_INFO", TranslationContext.Profile);
			}
			this.Grid.Reposition();
			if (base.gameObject.activeSelf)
			{
				base.StartCoroutine(GUIUtils.HackScrollReposition(this.ScrollView, this.ScrollBar, this.Grid));
			}
		}

		private string GetPerformanceInfo(RewardsInfo.Medals medal)
		{
			for (int i = 0; i < this.PerformanceInfos.Length; i++)
			{
				MainMenuProfileMatches.PerformanceInfo performanceInfo = this.PerformanceInfos[i];
				if (performanceInfo.MedalType == medal)
				{
					return performanceInfo.SpriteName;
				}
			}
			Debug.Assert(false, "PROFILE MATCHES - performance info not found. (GetPerformanceInfo) - Medal: " + medal, Debug.TargetTeam.All);
			return null;
		}

		private int SortItemsList(Transform t1, Transform t2)
		{
			return t1.GetComponent<MainMenuProfileMatchesSlot>().Compare(t2.GetComponent<MainMenuProfileMatchesSlot>());
		}

		private void ClientApiInventoryOnGetMatchHistoryError(object state, Exception exception)
		{
			MainMenuProfileMatches.Log.ErrorFormat("ClientApiInventoryOnGetMatchHistoryError - Failed to Get Match History. Exception: {0}", new object[]
			{
				exception
			});
		}

		public UIGrid Grid;

		public int GridPoolQuantity = 20;

		public UIScrollView ScrollView;

		public UIScrollBar ScrollBar;

		public GameObject CenterLineGameObject;

		public GameObject InfoGroupGameObject;

		public UILabel InfoGroupLabel;

		public GameObject LoadingFeedbackGameObject;

		[Header("[Result Colors]")]
		public Color ResultLabelVictoryColor;

		public Color ResultLabelDefeatColor;

		public Color ResultLabelAbandonColor;

		[SerializeField]
		protected MainMenuProfileMatches.PerformanceInfo[] PerformanceInfos;

		private static readonly BitLogger Log = new BitLogger(typeof(MainMenuProfileMatches));

		private bool _creatingPool;

		private bool _poolCreated;

		private bool _dataLoaded;

		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;

		private Coroutine disableCoroutine;

		[Serializable]
		protected struct PerformanceInfo
		{
			public RewardsInfo.Medals MedalType;

			public string SpriteName;
		}
	}
}
