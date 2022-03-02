using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Components.API;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using ClientAPI.Objects;
using FMod;
using HeavyMetalMachines.Audio.Music;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options.Presenting;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Hoplon.Input;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using ModelViewer;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class PickModeGUI : StateGuiController, ConfirmPickCallback.IConfirmPickCallbackListener
	{
		public List<int> LockedInCompetitiveCharacterIds { get; set; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnPickModeEnd;

		private UiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private IUiNavigationRebuilder UiNavigationAxisSelectorRebuilder
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		private IUiNavigationAxisSelectorTransformHandler UiNavigationAxisSelectorTransformHandler
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public void InitializeModelViewer()
		{
			PickModeGUI.Log.Info("[TestModelViewer][InitializeModelViewer]");
			if (this._carModelViewer.IsSceneLoaded())
			{
				string id = GameHubBehaviour.Hub.ClientApi.hubClient.Id;
				string msg = string.Format("SteamId={0}", id);
				GameHubBehaviour.Hub.Swordfish.Log.BILogClientMatchMsg(86, msg, true);
			}
			this._carModelViewer.Reset();
		}

		private void Awake()
		{
			PickModeGUI.Log.Info("[TestModelViewer][Awake]");
			this.InitializeModelViewer();
			if (GameHubBehaviour.Hub)
			{
				string arenaDraftName = GameHubBehaviour.Hub.ArenaConfig.GetArenaDraftName(GameHubBehaviour.Hub.Match.ArenaIndex);
				this.ArenaTitleLabel.text = Language.Get(arenaDraftName, TranslationContext.MainMenuGui);
				this.BackgroundSprite.SpriteName = GameHubBehaviour.Hub.ArenaConfig.GetPickImageName(GameHubBehaviour.Hub.Match.ArenaIndex);
				this._pickMode = (GameHubBehaviour.Hub.State.Current as PickModeSetup);
				for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
				{
					PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[i];
					if (playerData.Team == GameHubBehaviour.Hub.Players.CurrentPlayerData.Team)
					{
						this._alliedCount++;
						if (!playerData.IsBot)
						{
							this._alliedPlayersCount++;
						}
					}
					else
					{
						this._enemyCount++;
					}
				}
				if (GameHubBehaviour.Hub.Match.Kind == 6)
				{
					this.SetupTrainingHint();
					this._timerLabelEnabled = false;
				}
				else
				{
					this._timerLabelEnabled = true;
					this.DisableTrainingHint();
				}
			}
			this.SkinMenuNavigator.Config(GameHubBehaviour.Hub);
			this.SkinMenuNavigator.ListenToSkinSelectionChanged += this.SkinSelectionChanged;
			this._charactersConfigDic = new Dictionary<int, CharacterConfig>();
			this._playerSelection = new Dictionary<int, PlayerSelectionConfig>();
			this.ConfigTopPanelPlayerSelection();
			this.PickStateFeedback.OnPickCharacterStateStarted();
			this.PlayAnimations(this.OnPickCharacterStarted);
			this.SetupTeamInfo();
			if (GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.DriverHelper.SetWindowVisibility(false);
			}
			if (SpectatorController.IsSpectating)
			{
				base.StartCoroutine(this.WaitBeforeShowAnnouncerSelector());
			}
			this.BackgroundSprite.gameObject.SetActive(true);
			GameHubBehaviour.Hub.GuiScripts.Loading.OnHidingAnimationCompleted += this.Loading_OnHidingAnimationCompleted;
			this._pickConfirmedByServer = false;
			this._uiRootScale = this.BackgroundSprite.root.transform.localScale.x;
			this._screenAspectRatio = (float)Screen.width / (float)Screen.height;
			this._halfTooltipWidth = this.PickdModeStatusInfo.GetTooltipWidth() * 0.5f * this._uiRootScale;
			this.PickdModeStatusInfo.gameObject.SetActive(true);
			this.PickdModeStatusInfo.Initialize();
			BaseModelViewer carModelViewer = this._carModelViewer;
			carModelViewer.OnModelLoadedCallback = (Action)Delegate.Combine(carModelViewer.OnModelLoadedCallback, new Action(this.ModelViewLoaded));
			ObservableExtensions.Subscribe<Unit>(this._updateCurrentMatchPlayersCompetitiveState.Update());
			this.UiNavigationGroupHolder.AddGroup();
			ObservableExtensions.Subscribe<Unit>(this.UiNavigationGroupHolder.ObserveInputCancelDown(), delegate(Unit _)
			{
				this._optionsPresenter.Show();
			});
			this.PlayIntroPickScreenAudio();
		}

		private void TryEnableTimerLabel()
		{
			bool active = !SpectatorController.IsSpectating;
			this.TimerLabel.gameObject.SetActive(active);
		}

		private void DisableTrainingHint()
		{
			this.TryEnableTimerLabel();
			this.InfinityTimerSprite.gameObject.SetActive(false);
			this._trainingTitle.gameObject.SetActive(false);
			this._trainingDescription.gameObject.SetActive(false);
			this._trainingIcon.gameObject.SetActive(false);
		}

		private void SetupTrainingHint()
		{
			this.TimerLabel.gameObject.SetActive(false);
			this.InfinityTimerSprite.gameObject.SetActive(true);
			this._trainingTitle.gameObject.SetActive(true);
			this._trainingDescription.gameObject.SetActive(true);
			this._trainingIcon.gameObject.SetActive(true);
			IGameModeItemTypeComponent currentArenaMode = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArenaMode();
			this._trainingTitle.text = Language.Get(currentArenaMode.GameModeNameDraft, TranslationContext.TrainingMode);
			this._trainingDescription.text = Language.Get(currentArenaMode.GameModeDescriptionDraft, TranslationContext.TrainingMode);
			this._trainingIcon.SpriteName = currentArenaMode.IconName;
		}

		private void EnableTimerCounterAndDisableInfinityTimer()
		{
			this.TryEnableTimerLabel();
			this.InfinityTimerSprite.gameObject.SetActive(false);
		}

		public bool IsPickConfirmedByServer()
		{
			return this._pickConfirmedByServer;
		}

		private void Update()
		{
			if (this._timerLabelEnabled)
			{
				this.UpdateTimer();
			}
			else if (this._pickMode.CountdownStarted)
			{
				this.DisableSkinChange();
				this.StartCountdownAnnouncerAudio();
			}
		}

		private void StartCountdownAnnouncerAudio()
		{
			if (!this._countdownAudioPlayed)
			{
				this._countdownAudioPlayed = true;
				GameHubBehaviour.Hub.AnnouncerAudio.Play(3);
			}
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.GuiScripts.Loading.OnHidingAnimationCompleted -= this.Loading_OnHidingAnimationCompleted;
			this.BackgroundSprite.ClearSprite();
			this.SkinMenuNavigator.ListenToSkinSelectionChanged -= this.SkinSelectionChanged;
			BaseModelViewer carModelViewer = this._carModelViewer;
			carModelViewer.OnModelLoadedCallback = (Action)Delegate.Remove(carModelViewer.OnModelLoadedCallback, new Action(this.ModelViewLoaded));
			this.UiNavigationGroupHolder.RemoveGroup();
		}

		private void SetupRandomCharacterConfig()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			GameObject gameObject = this.InstantiateCharacterGridIcon(this.CarriersGridList, true);
			CharacterConfig component = gameObject.GetComponent<CharacterConfig>();
			component.CharItemType = null;
			this.MapCharacterToDictionary(-1, component);
		}

		private void SetupCharacterConfig(UIGrid chosenGrid, IItemType charItemType)
		{
			CharacterItemTypeComponent component = charItemType.GetComponent<CharacterItemTypeComponent>();
			bool isRotationActive = true;
			bool characterIsOwned = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(charItemType.Id) || SpectatorController.IsSpectating;
			GameObject gameObject = this.InstantiateCharacterGridIcon(chosenGrid, false);
			CharacterConfig component2 = gameObject.GetComponent<CharacterConfig>();
			component2.name = string.Format("[{0}]{1}", component.Difficulty, component.AssetPrefix);
			component2.CharItemType = charItemType;
			int characterId = component.CharacterId;
			component2.IconRef.SpriteName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, charItemType.Id, HudUtils.PlayerIconSize.Size64);
			component2.UIeventTrigger.onClick[0].parameters[0].value = characterId;
			bool characterIsInRotation = GameHubBehaviour.Hub.Characters.IsCharacterUnderRotationForPlayer(characterId, GameHubBehaviour.Hub.User.Bag);
			bool canBePicked = component.CanBePicked;
			if (this.LockedInCompetitiveCharacterIds == null)
			{
				this.LockedInCompetitiveCharacterIds = new List<int>(0);
			}
			bool lockedInCompetitive = this.LockedInCompetitiveCharacterIds.Contains(component.CharacterId);
			this.SetInitialCharacterSelection(component2, characterIsOwned, isRotationActive, characterIsInRotation, canBePicked, lockedInCompetitive);
			this.MapCharacterToDictionary(characterId, component2);
		}

		private void MapCharacterToDictionary(int characterId, CharacterConfig characterConfig)
		{
			this._charactersConfigDic.Add(characterId, characterConfig);
		}

		public void PopulateCharacterGrid()
		{
			IItemType[] allAvailableCharactersItemTypes = GameHubBehaviour.Hub.InventoryColletion.GetAllAvailableCharactersItemTypes();
			this._closedCharacterIds = new List<int>();
			if (!SpectatorController.IsSpectating)
			{
				this.SetupRandomCharacterConfig();
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (IItemType itemType in allAvailableCharactersItemTypes)
			{
				CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
				DriverRoleKind role = component.Role;
				UIGrid chosenGrid;
				if (role != 1)
				{
					if (role != 2)
					{
						if (component.Role != null)
						{
							PickModeGUI.Log.WarnFormat("Role {0} was not found. Using support role as default for {1} character. You should fix it fool.", new object[]
							{
								component.Role,
								component.GetCharacterLocalizedName()
							});
						}
						chosenGrid = this.SupportsGridList;
						num3++;
					}
					else
					{
						chosenGrid = this.TacklersGridList;
						num2++;
					}
				}
				else
				{
					chosenGrid = this.CarriersGridList;
					num++;
				}
				this.SetupCharacterConfig(chosenGrid, itemType);
			}
			this.CarriersGridList.Reposition();
			this.TacklersGridList.Reposition();
			this.SupportsGridList.Reposition();
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.NextFrame(0), delegate(Unit _)
			{
				this.UiNavigationAxisSelectorRebuilder.RebuildAndSelect();
			}));
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Character == null)
			{
				this.TryToSelectCharacter(this.FirstCharacterSelectedCharacterId);
			}
			else
			{
				this.TryToSelectCharacter(GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterId);
			}
		}

		private GameObject InstantiateCharacterGridIcon(UIGrid chosenGrid, bool isRandom = false)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.CharacterIconGridPrefab, Vector3.zero, Quaternion.identity);
			gameObject.SetActive(!isRandom);
			gameObject.transform.parent = chosenGrid.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			return gameObject;
		}

		private void ConfigTopPanelPlayerSelection()
		{
			if (GameHubBehaviour.Hub == null)
			{
				UnityEngine.Debug.Log("hub is null");
			}
			else if (GameHubBehaviour.Hub.Players == null)
			{
				UnityEngine.Debug.Log("hub.players is null");
			}
			else if (GameHubBehaviour.Hub.Players.CurrentPlayerData == null)
			{
				UnityEngine.Debug.Log("hub.players.current is null");
			}
			TeamKind team = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team;
			for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[i];
				Color playerColor;
				playerColor..ctor(0.1f, 0.54f, 0.98f, 1f);
				playerColor = GUIColorsInfo.GetPlayerColor(playerData.PlayerId, playerData.Team);
				Transform parentTransform = (playerData.Team != team) ? this.RedPlayerSelectionGrid.transform : this.BluePlayerSelectionGrid.transform;
				GameObject gameObject = this._container.InstantiatePrefab(this.CharacterIconPrefab, Vector3.zero, Quaternion.identity, parentTransform);
				gameObject.SetActive(true);
				gameObject.gameObject.name = ((GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerId != playerData.PlayerId) ? (playerData.PlayerCarId + 20).ToString("0000") : "0000");
				PlayerSelectionConfig component = gameObject.GetComponent<PlayerSelectionConfig>();
				component.SetupVoiceChatStatusChangerGUIButton(playerData);
				component.PlayerAddress = (int)playerData.PlayerAddress;
				component.SetPlayerName(playerData, playerColor, this._teams);
				component.UpdatePsnInfo(playerData);
				component.RepositionTeamTagPlayerNameAndPsnInfo();
				component.CharacterIcon.SpriteName = this.DefaultIconName;
				component.CharacterIcon.color = new Color(1f, 1f, 1f, 0.5f);
				PortraitDecoratorGui.UpdatePortraitSprite(playerData.Customizations, component.FounderBorderSprite, PortraitDecoratorGui.PortraitSpriteType.Circle);
				this._playerSelection[(int)playerData.PlayerAddress] = component;
				component.gameObject.transform.localScale = Vector3.one;
				component.gameObject.SetActive(true);
			}
			this.RedPlayerSelectionGrid.gameObject.SetActive(SpectatorController.IsSpectating);
			this.RedPlayerSelectionGrid.Reposition();
			this.BluePlayerSelectionGrid.Reposition();
		}

		private void SetTopPanelPlayerSelection(int playerAddress, CharacterConfig chosenCharacter)
		{
			if (chosenCharacter.CharItemType == null)
			{
				this._playerSelection[playerAddress].CharacterIcon.transform.rotation = Quaternion.identity;
				this._playerSelection[playerAddress].CharacterIcon.gameObject.SetActive(false);
				this._playerSelection[playerAddress].CharItemType = null;
				return;
			}
			this._playerSelection[playerAddress].CharacterIcon.gameObject.SetActive(true);
			PlayerData anyByAddress = GameHubBehaviour.Hub.Players.GetAnyByAddress((byte)this._playerSelection[playerAddress].PlayerAddress);
			if (anyByAddress.Team != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team)
			{
				this._playerSelection[playerAddress].CharacterIcon.transform.rotation = this.TopMenuEnemyIconRotation;
			}
			this._playerSelection[playerAddress].CharacterIcon.SpriteName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, chosenCharacter.CharItemType.Id, HudUtils.PlayerIconSize.Size64);
			this._playerSelection[playerAddress].CharItemType = chosenCharacter.CharItemType;
		}

		private void ConfirmTopPanelPlayerPick(int playerAddress, CharacterConfig chosenCharacter)
		{
			this.SetTopPanelPlayerSelection(playerAddress, chosenCharacter);
			this._playerSelection[playerAddress].CharacterIcon.color = Color.white;
		}

		public void OnCharacterClick(int characterId)
		{
			bool flag = this._characterSelected != null && this.GetCharacterIdFromCharacterConfig(this._characterSelected) == characterId;
			if (flag)
			{
				return;
			}
			this.TryToSelectCharacter(characterId);
		}

		public void SetRandomCharacterAsSelectedObject()
		{
			GUIUtils.ControllerSetSelectedObject(this._charactersConfigDic[-1].gameObject);
		}

		public void TryToSelectCharacter(int characterId)
		{
			PickModeGUI.Log.InfoFormatStackTrace("[TestModelViewer][TryToSelectCharacter] {0}", new object[]
			{
				characterId
			});
			if (SpectatorController.IsSpectating)
			{
				this.RefreshSelectedCharacterUI(characterId);
				return;
			}
			this._pickMode.SelectCharacter(characterId);
			this.RefreshSelectedCharacterUI(characterId);
		}

		public void OnServerConfirmCharacterSelection(int playerAdress, int characterId)
		{
			if (characterId == -1)
			{
				PickModeGUI.Log.DebugFormat("OnServerConfirmCharacterSelection received characterId = -1. Check CharacterService.SelectCharacter() and see if it was selected an unavailable char on server. Player Address {0}", new object[]
				{
					playerAdress
				});
				return;
			}
			CharacterConfig chosenCharacter;
			if (!this._charactersConfigDic.TryGetValue(characterId, out chosenCharacter))
			{
				PickModeGUI.Log.WarnFormat("OnServerConfirmCharacterSelection not in dic. Player Address {0} and Character ID {1}", new object[]
				{
					playerAdress,
					characterId
				});
				return;
			}
			this.SetTopPanelPlayerSelection(playerAdress, chosenCharacter);
		}

		private void RefreshSelectedCharacterUI(int characterId)
		{
			IItemType itemType = GameHubBehaviour.Hub.InventoryColletion.AllCharactersByCharacterId[characterId];
			CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
			PickModeGUI.Log.InfoFormat("[TestModelViewer][RefreshSelectedCharacterUI] Character={0}", new object[]
			{
				component.Character
			});
			CharacterConfig characterConfig = this._charactersConfigDic[characterId];
			this._selectedCharNameLabel.text = component.GetCharacterLocalizedName();
			this._selectedCharRoleLabel.text = component.GetRoleLocalized();
			this._selectedCharSprite.SpriteName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, itemType.Id, HudUtils.PlayerIconSize.Size64);
			this._selectedCharRecomendationSprite.gameObject.SetActive(component.Difficulty <= CharacterDifficulty.DifficultyLevel2);
			this.SetConfirmCharacterButtonState(characterConfig.IsEnabled);
			this.SetCharacterSelectedState(this._characterSelected, false);
			this.SetCharacterSelectedState(characterConfig, true);
			this.PlayCharacterMusic(this._characterSelected);
			this.LoadinModelViewer(this._characterSelected.CharItemType);
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.Setup(this._characterSelected.CharItemType, GameHubBehaviour.Hub.State.Current);
		}

		private void LoadinModelViewer(IItemType characterHierarchy)
		{
			this._modelViewerTexture.enabled = false;
			this._loadingFeedbackAnimation.Show();
			ShopItemTypeComponent component = characterHierarchy.GetComponent<ShopItemTypeComponent>();
			PickModeGUI.Log.InfoFormatStackTrace("[TestModelViewer][LoadinModelViewer] Character={0}", new object[]
			{
				component.ArtAssetName
			});
			this._carModelViewer.ModelName = component.ArtAssetName;
			this._carModelViewer.gameObject.SetActive(true);
		}

		private void ModelViewLoaded()
		{
			PickModeGUI.Log.InfoStackTrace("[TestModelViewer][ModelViewLoaded]");
			this._loadingFeedbackAnimation.Hide();
			this._modelViewerTexture.enabled = true;
		}

		private void SetCharacterSelectedState(CharacterConfig characterConfig, bool targetState)
		{
			if (characterConfig == null)
			{
				return;
			}
			characterConfig.IsSelected = targetState;
			characterConfig.SelectionAnimationGameObject.SetActive(targetState);
			if (!targetState)
			{
				return;
			}
			this._characterSelected = characterConfig;
			this.UiNavigationAxisSelectorTransformHandler.TryForceSelection(this._characterSelected.transform);
		}

		private void PlayCharacterMusic(CharacterConfig selectedCharacter)
		{
			int characterMusicId = 0;
			if (selectedCharacter.CharItemType != null)
			{
				AudioItemTypeComponent component = selectedCharacter.CharItemType.GetComponent<AudioItemTypeComponent>();
				if (component != null)
				{
					characterMusicId = component.CharacterMusicId;
				}
			}
			MusicManager.PlayCharacterMusic(characterMusicId);
		}

		public void TryToConfirmPick()
		{
			int characterIdFromCharacterConfig = this.GetCharacterIdFromCharacterConfig(this._characterSelected);
			this._pickMode.ConfirmPick(characterIdFromCharacterConfig);
		}

		public void OnServerConfirmMyPick(int characterId, Guid lastSkin)
		{
			if (this._pickConfirmedByServer)
			{
				PickModeGUI.Log.WarnFormat("Server confirmed character pick more than one time. CharacterID={0}, LastSkin={1}", new object[]
				{
					characterId,
					lastSkin
				});
				return;
			}
			this._pickConfirmedByServer = true;
			this.RefreshSelectedCharacterUI(characterId);
			PickModeGUI.Log.DebugFormat("Server confirmed character pick. CharacterID={0}, LastSkin={1}", new object[]
			{
				characterId,
				lastSkin
			});
			FMODAudioManager.PlayOneShotAt(this.ConfirmCharacterPickAudio, Vector3.zero, 0);
			int characterIdFromCharacterConfig = this.GetCharacterIdFromCharacterConfig(this._characterSelected);
			if (characterIdFromCharacterConfig != characterId)
			{
				CharacterConfig characterSelected;
				if (this._charactersConfigDic.TryGetValue(characterId, out characterSelected))
				{
					this._characterSelected = characterSelected;
				}
				else
				{
					PickModeGUI.Log.ErrorFormat("OnServerConfirmMyPick error for Character ID {1}", new object[]
					{
						characterId
					});
				}
			}
			this.ConfirmTopPanelPlayerPick((int)GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerAddress, this._characterSelected);
			this.PlayConfirmationPickAudio();
			this._alliedClosedPicksCount++;
			this.CheckSpotlightActivation();
			this.SetConfirmCharacterButtonState(false);
			this.CharacterSelectionGameObject.gameObject.SetActive(false);
			this.DisableCharacterSelection(this._characterSelected);
			if (this.PickScreenHints != null)
			{
				this.PickScreenHints.UpdateTips(this._closedCharacterIds);
			}
			this._announcerTextSelector.ChangeVisibility(true);
			this.SkinSelectionGameObject.SetActive(true);
			this.SkinMenuNavigator.OpenSkinWindow(GameHubBehaviour.Hub.InventoryColletion, this._characterSelected.CharItemType.Id);
			this.SkinMenuNavigator.ShowSelectSkin(lastSkin.ToString());
			this.SetConfirmSkinButtonState(false);
			this.PickStateFeedback.OnPickSkinStateStarted();
			this.PlayAnimations(this.OnPickSkinStarted);
			this.TryToHideTooltip();
			ItemTypeScriptableObject driver = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[this._characterSelected.CharItemType.Id];
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.Setup(driver, GameHubBehaviour.Hub.State.Current);
			this.OnCharacterIconHoverOut();
			this.PlayCharacterMusic(this._characterSelected);
		}

		private void SetInitialCharacterSelection(CharacterConfig charConfig, bool characterIsOwned, bool isRotationActive, bool characterIsInRotation, bool characterCanBePicked, bool lockedInCompetitive)
		{
			charConfig.IconRef.color = Color.white;
			charConfig.Button.defaultColor = Color.white;
			charConfig.Button.hover = Color.white;
			bool flag = characterCanBePicked && (!isRotationActive || characterIsInRotation || characterIsOwned) && !lockedInCompetitive;
			charConfig.GetComponent<Collider>().enabled = characterCanBePicked;
			charConfig.Button.SetState((!flag) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, true);
			charConfig.IsEnabled = flag;
			charConfig.IconRef.alpha = ((!flag) ? 0.5f : 1f);
			UIButton[] components = charConfig.GetComponents<UIButton>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].hover.a = ((!flag) ? 0.5f : 1f);
			}
			charConfig.CharacterBorder.gameObject.SetActive(true);
			CharacterItemTypeComponent component = charConfig.CharItemType.GetComponent<CharacterItemTypeComponent>();
			charConfig.RecommendedIcon.gameObject.SetActive(component.Difficulty <= CharacterDifficulty.DifficultyLevel2);
			charConfig.CarrierRing.gameObject.SetActive(flag && component.Role == 1);
			charConfig.TacklerRing.gameObject.SetActive(flag && component.Role == 2);
			charConfig.SupportRing.gameObject.SetActive(flag && component.Role == 0);
			charConfig.RotationGroupGameObject.SetActive(isRotationActive && characterIsInRotation);
			charConfig.LockedInCompetitiveImage.IsActive = lockedInCompetitive;
		}

		private void DisableCharacterSelection(CharacterConfig characterConfig)
		{
			characterConfig.Button.SetState(UIButtonColor.State.Disabled, true);
			characterConfig.IconRef.color = new Color(1f, 1f, 1f, 0.5f);
			UIButton[] components = characterConfig.GetComponents<UIButton>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].defaultColor = new Color(1f, 1f, 1f, 0.5f);
				components[i].hover = new Color(1f, 1f, 1f, 0.5f);
			}
			characterConfig.IsEnabled = false;
			this._closedCharacterIds.Add(this.GetCharacterIdFromCharacterConfig(characterConfig));
		}

		private void EnableCharacterSelection(CharacterConfig characterConfig)
		{
			characterConfig.IconRef.color = Color.white;
			characterConfig.Button.defaultColor = Color.white;
			characterConfig.Button.hover = Color.white;
			characterConfig.IsEnabled = true;
		}

		public void OnServerConfirmSomePlayerPick(int playerAddress, int teamKind, int characterId)
		{
			CharacterConfig characterConfig = this._charactersConfigDic[characterId];
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == (TeamKind)teamKind)
			{
				this._alliedClosedPicksCount++;
				this.DisableCharacterSelection(characterConfig);
				int characterIdFromCharacterConfig = this.GetCharacterIdFromCharacterConfig(this._characterSelected);
				if (characterIdFromCharacterConfig == characterId)
				{
					this.SetConfirmCharacterButtonState(false);
				}
			}
			else
			{
				this._enemyClosedPicksCount++;
			}
			this.CheckSpotlightActivation();
			this.ConfirmTopPanelPlayerPick(playerAddress, characterConfig);
			FMODAudioManager.PlayOneShotAt(GameHubBehaviour.Hub.AudioSettings.PickScreenCharacterEnterSFX, this._gameCameraEngine.CameraTransform.position, 0);
			if (this.PickScreenHints != null)
			{
				this.PickScreenHints.UpdateTips(this._closedCharacterIds);
			}
		}

		private void CheckSpotlightActivation()
		{
			this.AlliedSpotlight.SetActive(this._alliedClosedPicksCount >= this._alliedCount);
			this.EnemySpotlight.SetActive(this._enemyClosedPicksCount >= this._enemyCount);
		}

		public void TryToConfirmSkin()
		{
			Guid id = this._characterSelected.CharItemType.Id;
			Guid chosenSkinGuid = this.SkinMenuNavigator.GetChosenSkinGuid();
			this._pickMode.ConfirmSkin(id, chosenSkinGuid);
		}

		public void MoveToGridSelection()
		{
			this.PickStateFeedback.OnPickGridStateStarted();
			this.PlayAnimations(this.OnPickGridStarted);
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			FMODAudioManager.PlayOneShotAt(this.ConfirmSkinPickAudio, Vector3.zero, 0);
			this.DisableSkinChange();
			this.SkinSelectionGameObject.SetActive(false);
			this.GridPickGameObject.SetActive(true);
			this.SetConfirmGridButtonState(false);
		}

		private void DisableSkinChange()
		{
			this.SkinMenuNavigator.DisableSkinState();
			this.TryToHideTooltip();
			if (this.OnPickModeEnd != null)
			{
				this.OnPickModeEnd();
			}
		}

		public void OnServerConfirmSkin(ConfirmSkinCallback evt)
		{
			this.TryToHideTooltip();
		}

		private void TryToHideTooltip()
		{
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.TooltipController.HideWindow();
			}
		}

		public void SelectCurrentSkin()
		{
			GUIUtils.ControllerSetSelectedObject(this.SkinMenuNavigator.GetCurrentSkinGameObject());
		}

		public void MoveToChosenSkin(int skinIndex)
		{
			this.SkinMenuNavigator.MoveToChosenSkin(skinIndex);
		}

		public void MoveSkinNavigatorLeft()
		{
			this.SkinMenuNavigator.MoveLeft();
		}

		public void MoveSkinNavigatorRight()
		{
			this.SkinMenuNavigator.MoveRight();
		}

		private void SkinSelectionChanged(SkinConfig obj)
		{
			bool flag = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(obj.Item.Id);
			bool isDefault = obj.IsDefault;
			bool boolValue = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false);
			bool flag2 = flag || isDefault || boolValue;
			if (flag2)
			{
				this.TryToConfirmSkin();
			}
		}

		public void TryToConfirmGridSelection(int gridIndex)
		{
			this._pickMode.SelectGrid(gridIndex);
		}

		public void OnConfirmGridSelection(byte playerAddress, int gridIndex)
		{
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerAddress == playerAddress)
			{
				this._mySelectedGrid = gridIndex;
				this.SetConfirmGridButtonState(true);
			}
			PlayerData anyByAddress = GameHubBehaviour.Hub.Players.GetAnyByAddress(playerAddress);
			if (anyByAddress.Team != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team)
			{
				return;
			}
			HMMUI2DDynamicSprite hmmui2DDynamicSprite = this.PlayersIcons[anyByAddress.TeamSlot];
			string assetPrefix = this._playerSelection[(int)playerAddress].CharItemType.GetComponent<CharacterItemTypeComponent>().AssetPrefix;
			hmmui2DDynamicSprite.SpriteName = assetPrefix + "_icon_char_64";
			bool confirmGridButtonState = !this._closedGridIndex.Contains(gridIndex);
			this.SetConfirmGridButtonState(confirmGridButtonState);
			UIGrid uigrid = this.UIGridListForPlayerGridSelection[gridIndex];
			hmmui2DDynamicSprite.transform.parent.parent = uigrid.transform;
			hmmui2DDynamicSprite.transform.parent.transform.localPosition = Vector3.zero;
			uigrid.Reposition();
		}

		private void OnGridSelectionChangedWithController(int gridIndex)
		{
			if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
			{
				return;
			}
			this.TryToConfirmGridSelection(gridIndex);
		}

		private void SetControllerHoverToMySelectedGrid()
		{
			if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
			{
				return;
			}
			this.SelectTargetGameObject(this.UIGridListForPlayerGridSelection[this._mySelectedGrid].transform.parent.gameObject);
		}

		public void TrytoConfirmGridPick()
		{
			this._pickMode.PickGrid();
		}

		public void OnConfirmGridPick(byte playerAddress, int gridIndex)
		{
			PickModeGUI.Log.InfoFormat("OnConfirmGridPick. playerAddress={0} gridIndex={1}", new object[]
			{
				playerAddress,
				gridIndex
			});
			PlayerData anyByAddress = GameHubBehaviour.Hub.Players.GetAnyByAddress(playerAddress);
			if (SpectatorController.IsSpectating)
			{
				this._playerSelection[(int)playerAddress].SetGridPositionForSpectator(gridIndex);
				return;
			}
			if (anyByAddress.Team != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team)
			{
				return;
			}
			this._closedGridIndex.Add(gridIndex);
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerAddress == playerAddress)
			{
				this.HandleCurrentPlayerPickConfirmation(gridIndex);
				return;
			}
			HMMUI2DDynamicSprite hmmui2DDynamicSprite = this.PlayersIcons[anyByAddress.TeamSlot];
			UIGrid uigrid = this.UIGridListForPlayerGridSelection[gridIndex];
			if (this._mySelectedGrid == gridIndex)
			{
				this.SetConfirmGridButtonState(false);
			}
			UIButton component = uigrid.transform.parent.GetComponent<UIButton>();
			component.isEnabled = false;
			this.OnConfirmGirdIcons[gridIndex].gameObject.SetActive(true);
			string text = NGUIText.EscapeSymbols(anyByAddress.Name);
			if (!anyByAddress.IsBot)
			{
				text = this._getDisplayableNickName.GetFormattedNickNameWithPlayerTag(anyByAddress.PlayerId, NGUIText.EscapeSymbols(anyByAddress.Name), new long?(anyByAddress.PlayerTag));
			}
			this.OnConfirmGirdPlayerNames[gridIndex].text = text;
			this.PositionIndexIdentifier[gridIndex].SetActive(true);
			hmmui2DDynamicSprite.transform.parent.parent.gameObject.SetActive(false);
			uigrid.Reposition();
		}

		private void HandleCurrentPlayerPickConfirmation(int gridIndex)
		{
			this.SetConfirmGridButtonState(false);
			PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
			HMMUI2DDynamicSprite hmmui2DDynamicSprite = this.PlayersIcons[currentPlayerData.TeamSlot];
			UIGrid uigrid = this.UIGridListForPlayerGridSelection[gridIndex];
			for (int i = 0; i < this.UIGridListForPlayerGridSelection.Length; i++)
			{
				UIGrid uigrid2 = this.UIGridListForPlayerGridSelection[i];
				UIButton component = uigrid2.transform.parent.GetComponent<UIButton>();
				component.isEnabled = false;
			}
			this.WaitForOthersGameObject.SetActive(true);
			this.PickStateFeedback.OnWaitOtherPlayersStateStarted();
			this.PlayAnimations(this.OnWaitOtherPlayersStarted);
			this.OnConfirmGirdIcons[gridIndex].gameObject.SetActive(true);
			string text = NGUIText.EscapeSymbols(currentPlayerData.Name);
			if (!currentPlayerData.IsBot)
			{
				text = this._getDisplayableNickName.GetFormattedNickNameWithPlayerTag(currentPlayerData.PlayerId, NGUIText.EscapeSymbols(currentPlayerData.Name), new long?(currentPlayerData.PlayerTag));
			}
			this.OnConfirmGirdPlayerNames[gridIndex].text = text;
			this.PositionIndexIdentifier[gridIndex].SetActive(true);
			hmmui2DDynamicSprite.transform.parent.parent.gameObject.SetActive(false);
			uigrid.Reposition();
		}

		public void OnConfirmPickCallback(ConfirmPickCallback evt)
		{
			UnityEngine.Debug.LogWarning("[PickModelGUI] Reach OnConfirmPickCallback()");
		}

		private IEnumerator WaitBeforeShowAnnouncerSelector()
		{
			yield return UnityUtils.WaitForOneSecond;
			this._announcerTextSelector.ChangeVisibility(true);
			yield break;
		}

		private void UpdateTimer()
		{
			float num = Mathf.Max(this._pickMode.GetTimer(), 0f);
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)num);
			string text = TimeUtils.FormatTime(timeSpan);
			this.TimerLabel.text = text;
			this.SpectatorTimerLabel.text = text;
			if (timeSpan.Seconds > 5)
			{
				return;
			}
			if (this._pickMode.CountdownStarted)
			{
				this.DisableSkinChange();
				this.StartCountdownAnnouncerAudio();
			}
			if (timeSpan.Minutes != 0 || timeSpan.Seconds + this._countdownToAutoPickAudioPlayed != 5)
			{
				return;
			}
			this._countdownToAutoPickAudioPlayed++;
			FMODAudioManager.PlayOneShotAt(this.CountdownToAutoPickupAudio, Vector3.zero, 0);
		}

		public void OnCountdownStarted()
		{
			this._timerLabelEnabled = true;
			this.EnableTimerCounterAndDisableInfinityTimer();
			this.TimerLabel.color = this.CountdownTimerColor;
			this.SpectatorTimerLabel.color = this.CountdownTimerColor;
			this.PickStateFeedback.OnWaitOtherPlayersStateFinished();
			this.WaitForOthersGameObject.SetActive(false);
			this._announcerTextSelector.ChangeVisibility(false);
		}

		private void PlayConfirmationPickAudio()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			AudioItemTypeComponent component = this._characterSelected.CharItemType.GetComponent<AudioItemTypeComponent>();
			if (component == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Options.Game.CounselorActive && component.VoiceOver.CounselorOnLoading.VoiceLine != null)
			{
				GameHubBehaviour.Hub.AnnouncerAudio.PlayAudio(component.VoiceOver.CounselorOnLoading.VoiceLine, true);
			}
			else if (component.VoiceOver.PickScreen_Confirmation.VoiceLine)
			{
				FMODAudioManager.PlayOneShotAt(component.VoiceOver.PickScreen_Confirmation.VoiceLine, Vector3.zero, 0);
			}
		}

		public void PlayEndPickScreenAudio()
		{
			bool flag = false;
			if (GameHubBehaviour.Hub.Options.Game.CounselorActive && !SpectatorController.IsSpectating && GameHubBehaviour.Hub.Match.Kind != 6)
			{
				int[] allowedLoadingAdvicesCharactersId = GameHubBehaviour.Hub.CounselorConfig.AllowedLoadingAdvicesCharactersId;
				for (int i = 0; i < allowedLoadingAdvicesCharactersId.Length; i++)
				{
					if (this.GetCharacterIdFromCharacterConfig(this._characterSelected) == allowedLoadingAdvicesCharactersId[i])
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				GameHubBehaviour.Hub.AnnouncerAudio.PlayAudio(GameHubBehaviour.Hub.CounselorConfig.LoadingCounselorAudioAsset, true);
			}
			else
			{
				GameHubBehaviour.Hub.AnnouncerAudio.Play(4);
			}
		}

		public void PlayIntroPickScreenAudio()
		{
			AudioEventAsset audioEventAsset = null;
			if (GameHubBehaviour.Hub.Options.Game.CounselorActive && !SpectatorController.IsSpectating && GameHubBehaviour.Hub.Match.Kind != 6)
			{
				audioEventAsset = ((this._inputGetActiveDevicePoller.GetActiveDevice() != 3) ? GameHubBehaviour.Hub.CounselorConfig.IntroCounselorAudioAsset : GameHubBehaviour.Hub.CounselorConfig.JoystickIntroCounselorAudioAsset);
			}
			if (audioEventAsset != null)
			{
				GameHubBehaviour.Hub.AnnouncerAudio.PlayAudio(audioEventAsset, true);
			}
			else
			{
				GameHubBehaviour.Hub.AnnouncerAudio.Play(2);
			}
		}

		public void SelectTargetGameObject(GameObject targetGameObject)
		{
			GUIUtils.ControllerSetSelectedObject(targetGameObject);
		}

		public void ShowFeedbackWindow(string draftFeedback, params object[] args)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.GetFormatted(draftFeedback, TranslationContext.PickMode, args),
				OkButtonText = Language.Get("Ok", TranslationContext.GUI),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void PlayAnimations(List<HMMUIPlayTween> tweenList)
		{
			if (tweenList == null)
			{
				return;
			}
			for (int i = 0; i < tweenList.Count; i++)
			{
				HMMUIPlayTween hmmuiplayTween = tweenList[i];
				hmmuiplayTween.enabled = true;
				hmmuiplayTween.Play();
			}
		}

		private void SetConfirmGridButtonState(bool isEnabled)
		{
			PickModeGUI.SetButtonState(this.ConfirmGridButton, this.ConfirmGridButtonAnimation, isEnabled);
		}

		private void SetConfirmSkinButtonState(bool isEnabled)
		{
			this.ConfirmSkinButton.transform.parent.gameObject.SetActive(isEnabled);
			PickModeGUI.SetButtonState(this.ConfirmSkinButton, this.ConfirmSkinButtonAnimation, isEnabled);
		}

		private void SetConfirmCharacterButtonState(bool isEnabled)
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			PickModeGUI.SetButtonState(this.ConfirmButton, this.ConfirmButtonAnimation, isEnabled);
		}

		private static void SetButtonState(UIButton targetButton, Animation buttonAnimation, bool isEnabled)
		{
			if (buttonAnimation)
			{
				buttonAnimation.gameObject.SetActive(isEnabled);
			}
			targetButton.isEnabled = isEnabled;
			if (isEnabled)
			{
				buttonAnimation.Play();
			}
		}

		private void Loading_OnHidingAnimationCompleted()
		{
			if (this.PickScreenHints != null)
			{
				if (SpectatorController.IsSpectating)
				{
					Object.Destroy(this.PickScreenHints.gameObject);
				}
				else
				{
					this.PickScreenHints.UpdateTips(this._closedCharacterIds);
				}
			}
		}

		public void OnHelpButtonClick()
		{
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.SetWindowVisibility(true);
		}

		private void SetupTeamInfo()
		{
			this.AllyTeamGameObject.SetActive(false);
			this.EnemyTeamGameObject.SetActive(false);
			this.SetGroupTeamInfo(TeamKind.Blue);
			this.SetGroupTeamInfo(TeamKind.Red);
		}

		private void SetGroupTeamInfo(TeamKind teamKind)
		{
			Team groupTeam = this._teams.GetGroupTeam(teamKind);
			if (groupTeam == null)
			{
				return;
			}
			string teamTagGlobalRestriction = this._teamNameRestriction.GetTeamTagGlobalRestriction(groupTeam.Tag);
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == teamKind)
			{
				this.AllyTeamIconSprite.SpriteName = groupTeam.ImageUrl;
				this.AllyTeamNameLabel.text = NGUIText.EscapeSymbols(string.Format("[{0}]", teamTagGlobalRestriction));
				this.AllyTeamGameObject.SetActive(true);
			}
			else if (SpectatorController.IsSpectating)
			{
				this.EnemyTeamIconSprite.SpriteName = groupTeam.ImageUrl;
				this.EnemyTeamNameLabel.text = NGUIText.EscapeSymbols(string.Format("[{0}]", teamTagGlobalRestriction));
				this.EnemyTeamGameObject.SetActive(true);
			}
		}

		public void OnCharacterIconHoverOver(CharacterConfig characterConfig)
		{
			this.PickdModeStatusInfo.Setup(characterConfig.CharItemType);
			this.PickdModeStatusInfo.SetVisibility(true);
		}

		public void OnCharacterIconHoverOut()
		{
			this.PickdModeStatusInfo.SetVisibility(false);
		}

		private int GetCharacterIdFromCharacterConfig(CharacterConfig charConfig)
		{
			if (charConfig.CharItemType == null)
			{
				return -1;
			}
			CharacterItemTypeComponent component = charConfig.CharItemType.GetComponent<CharacterItemTypeComponent>();
			return (!(component == null)) ? component.CharacterId : -1;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PickModeGUI));

		[Inject]
		private DiContainer _container;

		[Inject]
		private IMatchTeams _teams;

		[Inject]
		private IUpdateCurrentMatchPlayersCompetitiveState _updateCurrentMatchPlayersCompetitiveState;

		[Inject]
		private IGameCameraEngine _gameCameraEngine;

		[Inject]
		private IOptionsPresenter _optionsPresenter;

		[Inject]
		private IGetDisplayableNickName _getDisplayableNickName;

		[Inject]
		private ITeamNameRestriction _teamNameRestriction;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		private PickModeSetup _pickMode;

		private bool _timerLabelEnabled;

		[Header("Timer")]
		public UILabel TimerLabel;

		public UI2DSprite InfinityTimerSprite;

		public UILabel SpectatorTimerLabel;

		public Color CountdownTimerColor;

		private bool _countdownAudioPlayed;

		private int _countdownToAutoPickAudioPlayed;

		[Header("Audio")]
		public AudioEventAsset ConfirmCharacterPickAudio;

		public AudioEventAsset ConfirmSkinPickAudio;

		public AudioEventAsset CountdownToAutoPickupAudio;

		[Header("Pick State Feedback")]
		public PickStateFeedback PickStateFeedback;

		[Header("Animations")]
		public List<HMMUIPlayTween> OnPickCharacterStarted;

		public List<HMMUIPlayTween> OnPickSkinStarted;

		public List<HMMUIPlayTween> OnPickGridStarted;

		public List<HMMUIPlayTween> OnWaitOtherPlayersStarted;

		[Header("Spotlight")]
		public GameObject AlliedSpotlight;

		public GameObject EnemySpotlight;

		[Header("Character Selection")]
		public int FirstCharacterSelectedCharacterId = 13;

		public string DefaultIconName;

		public string RandomCarName;

		public GameObject CharacterIconGridPrefab;

		public UIGrid CarriersGridList;

		public UIGrid TacklersGridList;

		public UIGrid SupportsGridList;

		[SerializeField]
		private BaseModelViewer _carModelViewer;

		[SerializeField]
		private UITexture _modelViewerTexture;

		public GameObject CharacterSelectionGameObject;

		public UIButton ConfirmButton;

		public Animation ConfirmButtonAnimation;

		public List<int> _closedCharacterIds;

		public PickModeStatusInfo PickdModeStatusInfo;

		[SerializeField]
		private NguiLoadingView _loadingFeedbackAnimation;

		[SerializeField]
		private UILabel _selectedCharNameLabel;

		[SerializeField]
		private UILabel _selectedCharRoleLabel;

		[SerializeField]
		private HMMUI2DDynamicSprite _selectedCharSprite;

		[SerializeField]
		private UI2DSprite _selectedCharRecomendationSprite;

		[Header("Top Menu Player Selection")]
		[Tooltip("Will use colors from GUIColorsInfo")]
		public GameObject CharacterIconPrefab;

		public UIGrid BluePlayerSelectionGrid;

		public UIGrid RedPlayerSelectionGrid;

		private Dictionary<int, PlayerSelectionConfig> _playerSelection;

		private Dictionary<int, CharacterConfig> _charactersConfigDic;

		private CharacterConfig _characterSelected;

		public Quaternion TopMenuEnemyIconRotation;

		[Header("Skin Selection")]
		public GameObject SkinSelectionGameObject;

		public SkinNavigator SkinMenuNavigator;

		public UIButton ConfirmSkinButton;

		public Animation ConfirmSkinButtonAnimation;

		[Header("Wait for Others")]
		public GameObject WaitForOthersGameObject;

		[Header("[Team]")]
		public GameObject AllyTeamGameObject;

		public HMMUI2DDynamicSprite AllyTeamIconSprite;

		public UILabel AllyTeamNameLabel;

		public GameObject EnemyTeamGameObject;

		public HMMUI2DDynamicSprite EnemyTeamIconSprite;

		public UILabel EnemyTeamNameLabel;

		[Header("Arena")]
		public HMMUI2DDynamicSprite BackgroundSprite;

		public UILabel ArenaTitleLabel;

		[Header("[Hints]")]
		[SerializeField]
		private PickScreenHints PickScreenHints;

		[SerializeField]
		private AnnouncerTextSelector _announcerTextSelector;

		[SerializeField]
		private UILabel _trainingTitle;

		[SerializeField]
		private UILabel _trainingDescription;

		[SerializeField]
		private HMMUI2DDynamicSprite _trainingIcon;

		private int _alliedClosedPicksCount;

		private int _alliedCount;

		private int _alliedPlayersCount;

		private int _enemyClosedPicksCount;

		private int _enemyCount;

		private bool _pickConfirmedByServer;

		private float _uiRootScale;

		private float _halfTooltipWidth;

		private float _screenAspectRatio;

		[Header("[Hints]")]
		public UIGrid[] UIGridListForPlayerGridSelection;

		public HMMUI2DDynamicSprite[] PlayersIcons;

		public HMMUI2DDynamicSprite[] OnConfirmGirdIcons;

		public UILabel[] OnConfirmGirdPlayerNames;

		public GameObject[] PositionIndexIdentifier;

		public UIButton ConfirmGridButton;

		public Animation ConfirmGridButtonAnimation;

		public GameObject GridPickGameObject;

		private List<int> _closedGridIndex = new List<int>(4);

		private int _mySelectedGrid = -1;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;
	}
}
