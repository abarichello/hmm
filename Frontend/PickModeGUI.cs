using System;
using System.Collections;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using ClientAPI.Objects;
using FMod;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.Audio.Music;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using ModelViewer;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PickModeGUI : StateGuiController, ConfirmPickCallback.IConfirmPickCallbackListener
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub)
			{
				string arenaDraftName = GameHubBehaviour.Hub.ArenaConfig.GetArenaDraftName(GameHubBehaviour.Hub.Match.ArenaIndex);
				this.ArenaTitleLabel.text = Language.Get(arenaDraftName, TranslationSheets.MainMenuGui);
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
			}
			this.SkinMenuNavigator.Config(GameHubBehaviour.Hub);
			this.SkinMenuNavigator.ListenToSkinSelectionChanged += this.SkinSelectionChanged;
			this._charactersConfigDic = new Dictionary<int, CharacterConfig>();
			this._playerSelection = new Dictionary<int, PlayerSelectionConfig>();
			this.ConfigTopPanelPlayerSelection();
			this.PopulateCharacterGrid();
			this.PickStateFeedback.OnPickCharacterStateStarted();
			this.PlayAnimations(this.OnPickCharacterStarted);
			this.SetupTeamInfo();
			this.PlayIntroPickScreenAudio();
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
			this.PickdModeStatusInfo.SetVisibility(false);
			BaseModelViewer carModelViewer = this._carModelViewer;
			carModelViewer.OnModelLoadedCallback = (Action)Delegate.Combine(carModelViewer.OnModelLoadedCallback, new Action(this.ModelViewLoaded));
		}

		private void Update()
		{
			this.UpdateTimer();
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.GuiScripts.Loading.OnHidingAnimationCompleted -= this.Loading_OnHidingAnimationCompleted;
			this.HideTooltip();
			this.CharacterDescription.OnDisable();
			this.BackgroundSprite.ClearSprite();
			this.SkinMenuNavigator.ListenToSkinSelectionChanged -= this.SkinSelectionChanged;
			BaseModelViewer carModelViewer = this._carModelViewer;
			carModelViewer.OnModelLoadedCallback = (Action)Delegate.Remove(carModelViewer.OnModelLoadedCallback, new Action(this.ModelViewLoaded));
		}

		private void SetupRandomCharacterConfig()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			GameObject gameObject = this.InstantiateCharacterGridIcon(this.CarriersGridList, true);
			CharacterConfig component = gameObject.GetComponent<CharacterConfig>();
			component.CharInfo = null;
			component.PickModeGUI = this;
			this.MapCharacterToDictionary(-1, component);
		}

		private void SetupCharacterConfig(UIGrid chosenGrid, HeavyMetalMachines.Character.CharacterInfo info)
		{
			bool isRotationActive = true;
			bool characterIsOwned = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(info.CharacterItemTypeGuid) || SpectatorController.IsSpectating;
			bool characterIsInRotation = GameHubBehaviour.Hub.Characters.IsCharacterUnderRotationForPlayer(info.CharacterId, GameHubBehaviour.Hub.User.Bag);
			bool canBePicked = info.CanBePicked;
			GameObject gameObject = this.InstantiateCharacterGridIcon(chosenGrid, false);
			CharacterConfig component = gameObject.GetComponent<CharacterConfig>();
			component.name = string.Format("[{0}]{1}", info.Dificult, info.Asset);
			component.CharInfo = info;
			component.PickModeGUI = this;
			component.IconRef.SpriteName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, info.CharacterItemTypeGuid, HudUtils.PlayerIconSize.Size64);
			component.UIeventTrigger.onClick[0].parameters[0].value = info.CharacterId;
			this.SetInitialCharacterSelection(component, characterIsOwned, isRotationActive, characterIsInRotation, canBePicked);
			this.MapCharacterToDictionary(info.CharacterId, component);
		}

		private void MapCharacterToDictionary(int characterId, CharacterConfig characterConfig)
		{
			this._charactersConfigDic.Add(characterId, characterConfig);
		}

		private void PopulateCharacterGrid()
		{
			HeavyMetalMachines.Character.CharacterInfo[] allAvailableCharacterInfos = GameHubBehaviour.Hub.InventoryColletion.GetAllAvailableCharacterInfos();
			this._keyNavigations = new List<UIKeyNavigation>();
			this._closedCharacterIds = new List<int>();
			if (!SpectatorController.IsSpectating)
			{
				this.SetupRandomCharacterConfig();
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (HeavyMetalMachines.Character.CharacterInfo characterInfo in allAvailableCharacterInfos)
			{
				HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind role = characterInfo.Role;
				UIGrid chosenGrid;
				if (role != HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Carrier)
				{
					if (role != HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Tackler)
					{
						if (characterInfo.Role != HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Support)
						{
							PickModeGUI.Log.WarnFormat("Role {0} was not found. Using support role as default for {1} character. You should fix it fool.", new object[]
							{
								characterInfo.Role,
								characterInfo.LocalizedName
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
				this.SetupCharacterConfig(chosenGrid, characterInfo);
			}
			this.AddIconsLeft(num, this.CarriersGridList);
			this.AddIconsLeft(num2, this.TacklersGridList);
			this.AddIconsLeft(num3, this.SupportsGridList);
			this.CarriersGridList.Reposition();
			this.TacklersGridList.Reposition();
			this.SupportsGridList.Reposition();
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Character == null)
			{
				this.TryToSelectCharacter(this.FirstCharacterSelectedCharacterId);
			}
			else
			{
				this.TryToSelectCharacter(GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterId);
			}
		}

		private void AddIconsLeft(int iconCount, UIGrid chosenGrid)
		{
			int num = iconCount % chosenGrid.maxPerLine;
			if (num == 0)
			{
				return;
			}
			int num2 = chosenGrid.maxPerLine - num;
			for (int i = 0; i < num2; i++)
			{
				this.AddDisabledIcon(chosenGrid);
			}
		}

		private void AddDisabledIcon(UIGrid chosenGrid)
		{
			GameObject gameObject = this.InstantiateCharacterGridIcon(chosenGrid, false);
			CharacterConfig component = gameObject.GetComponent<CharacterConfig>();
			component.EmptyBorder.gameObject.SetActive(true);
			component.CharInfo = null;
			component.GetComponent<Collider>().enabled = false;
		}

		private GameObject InstantiateCharacterGridIcon(UIGrid chosenGrid, bool isRandom = false)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.CharacterIconGridPrefab, Vector3.zero, Quaternion.identity);
			gameObject.SetActive(!isRandom);
			gameObject.transform.parent = chosenGrid.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			return gameObject;
		}

		private void ConfigTopPanelPlayerSelection()
		{
			TeamKind team = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team;
			for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[i];
				Color playerColor = new Color(0.1f, 0.54f, 0.98f, 1f);
				playerColor = GUIColorsInfo.GetPlayerColor(playerData.PlayerId, playerData.Team);
				Transform parent = (playerData.Team != team) ? this.RedPlayerSelectionGrid.transform : this.BluePlayerSelectionGrid.transform;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.CharacterIconPrefab, Vector3.zero, Quaternion.identity);
				gameObject.SetActive(true);
				gameObject.gameObject.name = ((GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerId != playerData.PlayerId) ? (playerData.PlayerCarId + 20).ToString("0000") : "0000");
				PlayerSelectionConfig component = gameObject.GetComponent<PlayerSelectionConfig>();
				component.SetupVoiceChatStatusChangerGUIButton(playerData);
				component.PlayerAddress = (int)playerData.PlayerAddress;
				component.SetPlayerName(playerData, playerColor);
				component.CharacterIcon.SpriteName = this.DefaultIconName;
				component.CharacterIcon.color = new Color(1f, 1f, 1f, 0.5f);
				PortraitDecoratorGui.UpdatePortraitSprite(playerData.Customizations, component.FounderBorderSprite, PortraitDecoratorGui.PortraitSpriteType.Circle);
				component.CharInfo = null;
				this._playerSelection[(int)playerData.PlayerAddress] = component;
				component.transform.parent = parent;
				component.gameObject.transform.localScale = Vector3.one;
				component.gameObject.SetActive(true);
			}
			this.RedPlayerSelectionGrid.gameObject.SetActive(SpectatorController.IsSpectating);
			this.RedPlayerSelectionGrid.Reposition();
			this.BluePlayerSelectionGrid.Reposition();
		}

		private void SetTopPanelPlayerSelection(int playerAddress, CharacterConfig chosenCharacter)
		{
			if (chosenCharacter.CharInfo == null)
			{
				this._playerSelection[playerAddress].CharacterIcon.transform.rotation = Quaternion.identity;
				this._playerSelection[playerAddress].CharacterIcon.gameObject.SetActive(false);
				this._playerSelection[playerAddress].CharInfo = null;
				return;
			}
			this._playerSelection[playerAddress].CharacterIcon.gameObject.SetActive(true);
			PlayerData anyByAddress = GameHubBehaviour.Hub.Players.GetAnyByAddress((byte)this._playerSelection[playerAddress].PlayerAddress);
			if (anyByAddress.Team != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team)
			{
				this._playerSelection[playerAddress].CharacterIcon.transform.rotation = this.TopMenuEnemyIconRotation;
			}
			this._playerSelection[playerAddress].CharacterIcon.SpriteName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, chosenCharacter.CharInfo.CharacterItemTypeGuid, HudUtils.PlayerIconSize.Size64);
			this._playerSelection[playerAddress].CharInfo = chosenCharacter.CharInfo;
		}

		private void ConfirmTopPanelPlayerPick(int playerAddress, CharacterConfig chosenCharacter)
		{
			this.SetTopPanelPlayerSelection(playerAddress, chosenCharacter);
			this._playerSelection[playerAddress].CharacterIcon.color = Color.white;
		}

		public void OnCharacterClick(int characterId)
		{
			bool flag = this._characterSelected != null && this._characterSelected.CharInfo.CharacterId == characterId;
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
			if (SpectatorController.IsSpectating)
			{
				this.RefreshSelectedCharacterUI(characterId, null);
				return;
			}
			this._pickMode.SelectCharacter(characterId);
			this.RefreshSelectedCharacterUI(characterId, null);
		}

		public void OnServerConfirmCharacterSelection(int playerAdress, int characterId)
		{
			if (characterId == -1)
			{
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

		private void RefreshSelectedCharacterUI(int characterId, CharacterConfig characterConfig = null)
		{
			if (characterConfig == null)
			{
				characterConfig = this._charactersConfigDic[characterId];
			}
			this._selectedCharNameLabel.text = characterConfig.CharInfo.LocalizedName;
			this._selectedCharRoleLabel.text = characterConfig.CharInfo.GetRoleTranslation();
			this._selectedCharSprite.SpriteName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, characterConfig.CharInfo.CharacterItemTypeGuid, HudUtils.PlayerIconSize.Size64);
			this._selectedCharRecomendationSprite.gameObject.SetActive(characterConfig.CharInfo.Dificult <= 2);
			this.SetConfirmCharacterButtonState(characterConfig.IsEnabled);
			this.SetCharacterSelectedState(this._characterSelected, false);
			this.SetCharacterSelectedState(characterConfig, true);
			this.PlayCharacterMusic(this._characterSelected);
			this.LoadinModelViewer(characterConfig);
			ItemTypeScriptableObject driver = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[this._characterSelected.CharInfo.CharacterItemTypeGuid];
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.Setup(driver, GameHubBehaviour.Hub.State.Current);
		}

		private void LoadinModelViewer(CharacterConfig characterConfig)
		{
			this._modelViewerTexture.enabled = false;
			this._loadingFeedbackAnimation.Show();
			this._carModelViewer.ModelName = string.Format("{0}_skin_00_shop", characterConfig.CharInfo.Asset);
			this._carModelViewer.gameObject.SetActive(true);
		}

		private void ModelViewLoaded()
		{
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
			characterConfig.Animation.gameObject.SetActive(targetState);
			if (!targetState)
			{
				return;
			}
			this._characterSelected = characterConfig;
			this.CharacterDescription.SetNewDescription(characterConfig, this, GameHubBehaviour.Hub);
		}

		private void PlayCharacterMusic(CharacterConfig selectedCharacter)
		{
			MusicManager.PlayCharacterMusic(selectedCharacter.CharInfo);
		}

		public void TryToConfirmPick()
		{
			int characterId = (!(this._characterSelected.CharInfo == null)) ? this._characterSelected.CharInfo.CharacterId : -1;
			this._pickMode.ConfirmPick(characterId);
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
			FMODAudioManager.PlayOneShotAt(this.ConfirmCharacterPickAudio, Vector3.zero, 0);
			int num = (!(this._characterSelected.CharInfo == null)) ? this._characterSelected.CharInfo.CharacterId : -1;
			if (num != characterId)
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
			for (int i = 0; i < this._keyNavigations.Count; i++)
			{
				CharacterConfig component = this._keyNavigations[i].GetComponent<CharacterConfig>();
				if (component)
				{
					this.DisableCharacterSelection(component);
				}
			}
			this.SkinSelectionGameObject.SetActive(true);
			this.SkinMenuNavigator.OpenSkinWindow(GameHubBehaviour.Hub.InventoryColletion, this._characterSelected.CharInfo.CharacterItemTypeGuid);
			this.SkinMenuNavigator.ShowSelectSkin(lastSkin.ToString());
			this.SetConfirmSkinButtonState(false);
			this.PickStateFeedback.OnPickSkinStateStarted();
			this.PlayAnimations(this.OnPickSkinStarted);
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.TooltipController.HideWindow();
			}
			ItemTypeScriptableObject driver = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[this._characterSelected.CharInfo.CharacterItemTypeGuid];
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.Setup(driver, GameHubBehaviour.Hub.State.Current);
			this.OnCharacterIconHoverOut();
			this.PlayCharacterMusic(this._characterSelected);
		}

		private void SetInitialCharacterSelection(CharacterConfig charConfig, bool characterIsOwned, bool isRotationActive, bool characterIsInRotation, bool characterCanBePicked)
		{
			charConfig.IconRef.color = Color.white;
			charConfig.Button.defaultColor = Color.white;
			charConfig.Button.hover = Color.white;
			bool flag = characterCanBePicked && (!isRotationActive || characterIsInRotation || characterIsOwned);
			charConfig.GetComponent<Collider>().enabled = characterCanBePicked;
			charConfig.Button.SetState((!flag) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, true);
			charConfig.IsEnabled = flag;
			charConfig.IconRef.alpha = ((!flag) ? 0.5f : 1f);
			charConfig.CharacterBorder.gameObject.SetActive(true);
			charConfig.RecommendedIcon.gameObject.SetActive(charConfig.CharInfo.Dificult <= 2);
			charConfig.CarrierRing.gameObject.SetActive(flag && charConfig.CharInfo.Role == HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Carrier);
			charConfig.TacklerRing.gameObject.SetActive(flag && charConfig.CharInfo.Role == HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Tackler);
			charConfig.SupportRing.gameObject.SetActive(flag && charConfig.CharInfo.Role == HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Support);
			charConfig.RotationGroupGameObject.SetActive(isRotationActive && characterIsInRotation);
		}

		private void DisableCharacterSelection(CharacterConfig characterConfig)
		{
			characterConfig.Button.SetState(UIButtonColor.State.Disabled, true);
			characterConfig.IconRef.color = new Color(1f, 1f, 1f, 0.5f);
			characterConfig.Button.defaultColor = new Color(1f, 1f, 1f, 0.5f);
			characterConfig.Button.hover = new Color(1f, 1f, 1f, 0.5f);
			characterConfig.IsEnabled = false;
			if (characterConfig.CharInfo)
			{
				this._closedCharacterIds.Add(characterConfig.CharInfo.CharacterId);
			}
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
				if (this._characterSelected && this._characterSelected.CharInfo && this._characterSelected.CharInfo.CharacterId == characterId)
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
			FMODAudioManager.PlayOneShotAt(GameHubBehaviour.Hub.AudioSettings.PickScreenCharacterEnterSFX, CarCamera.Singleton.transform.position, 0);
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
			Guid characterItemTypeGuid = this._characterSelected.CharInfo.CharacterItemTypeGuid;
			Guid chosenSkinGuid = this.SkinMenuNavigator.GetChosenSkinGuid();
			this._pickMode.ConfirmSkin(characterItemTypeGuid, chosenSkinGuid);
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
		}

		public void OnServerConfirmSkin(ConfirmSkinCallback evt)
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
			hmmui2DDynamicSprite.SpriteName = this._playerSelection[(int)playerAddress].CharInfo.Asset + "_icon_char_64";
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

		public void OnConfirmGridPick(byte playerAddress, int gridIndex, Guid skinSelected)
		{
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
				this.HandleCurrentPlayerPickConfirmation(gridIndex, skinSelected);
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
			ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[skinSelected];
			SkinPrefabItemTypeComponent component2 = itemTypeScriptableObject.GetComponent<SkinPrefabItemTypeComponent>();
			this.OnConfirmGirdIcons[gridIndex].gameObject.SetActive(true);
			this.OnConfirmGirdIcons[gridIndex].SpriteName = component2.SkinSpriteName;
			this.OnConfirmGirdPlayerNames[gridIndex].text = NGUIText.EscapeSymbols(anyByAddress.Name);
			this.PositionIndexIdentifier[gridIndex].SetActive(true);
			hmmui2DDynamicSprite.transform.parent.parent.gameObject.SetActive(false);
			uigrid.Reposition();
		}

		private void HandleCurrentPlayerPickConfirmation(int gridIndex, Guid skinSelected)
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
			this.OnConfirmGirdIcons[gridIndex].SpriteName = this.SkinMenuNavigator.GetSkinConfig(skinSelected).name;
			this.OnConfirmGirdPlayerNames[gridIndex].text = NGUIText.EscapeSymbols(currentPlayerData.Name);
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
			float timer = this._pickMode.GetTimer();
			this._lastTime = (float)((int)timer);
			int num = Mathf.FloorToInt(this._lastTime / 60f);
			int num2 = Mathf.FloorToInt(this._lastTime - (float)(num * 60));
			if (this._lastTime >= 0f)
			{
				this.TimerLabel.text = string.Format("{0:#0}:{1:00}", num, num2);
			}
			if (num2 <= 5)
			{
				if (this._pickMode.CountdownStarted)
				{
					this.DisableSkinChange();
					if (!this._countdownAudioPlayed)
					{
						this._countdownAudioPlayed = true;
						GameHubBehaviour.Hub.AnnouncerAudio.Play(AnnouncerVoiceOverType.PickCountdown);
						return;
					}
				}
				if (num != 0 || num2 + this._countdownToAutoPickAudioPlayed != 5)
				{
					return;
				}
				this._countdownToAutoPickAudioPlayed++;
				FMODAudioManager.PlayOneShotAt(this.CountdownToAutoPickupAudio, Vector3.zero, 0);
			}
		}

		public void OnCountdownStarted()
		{
			this.TimerLabel.color = this.CountdownTimerColor;
			this.PickStateFeedback.OnWaitOtherPlayersStateFinished();
			this.WaitForOthersGameObject.SetActive(false);
			this._announcerTextSelector.ChangeVisibility(false);
		}

		private void PlayConfirmationPickAudio()
		{
			if (!SpectatorController.IsSpectating)
			{
				if (GameHubBehaviour.Hub.Options.Game.CounselorActive && this._characterSelected.CharInfo.voiceOver.CounselorOnLoading.VoiceLine != null)
				{
					GameHubBehaviour.Hub.AnnouncerAudio.PlayAudio(this._characterSelected.CharInfo.voiceOver.CounselorOnLoading.VoiceLine, true);
				}
				else if (this._characterSelected.CharInfo.voiceOver.PickScreen_Confirmation.VoiceLine)
				{
					FMODAudioManager.PlayOneShotAt(this._characterSelected.CharInfo.voiceOver.PickScreen_Confirmation.VoiceLine, Vector3.zero, 0);
				}
			}
		}

		public void PlayEndPickScreenAudio()
		{
			bool flag = false;
			if (GameHubBehaviour.Hub.Options.Game.CounselorActive && !SpectatorController.IsSpectating)
			{
				int[] allowedLoadingAdvicesCharactersId = GameHubBehaviour.Hub.CounselorConfig.AllowedLoadingAdvicesCharactersId;
				for (int i = 0; i < allowedLoadingAdvicesCharactersId.Length; i++)
				{
					if (this._characterSelected.CharInfo.CharacterId == allowedLoadingAdvicesCharactersId[i])
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
				GameHubBehaviour.Hub.AnnouncerAudio.Play(AnnouncerVoiceOverType.PickEnd);
			}
		}

		public void PlayIntroPickScreenAudio()
		{
			bool flag = GameHubBehaviour.Hub.Options.Game.CounselorActive && !SpectatorController.IsSpectating && GameHubBehaviour.Hub.CounselorConfig.IntroCounselorAudioAsset != null;
			if (flag)
			{
				GameHubBehaviour.Hub.AnnouncerAudio.PlayAudio(GameHubBehaviour.Hub.CounselorConfig.IntroCounselorAudioAsset, true);
			}
			else
			{
				GameHubBehaviour.Hub.AnnouncerAudio.Play(AnnouncerVoiceOverType.PickStart);
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
				QuestionText = string.Format(Language.Get(draftFeedback, TranslationSheets.PickMode), args),
				OkButtonText = Language.Get("Ok", "GUI"),
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

		public void OnButtonHover(GameObject button)
		{
			button.gameObject.SetActive(!button.activeInHierarchy);
		}

		public void ShowRoleTooltip(string translatedText)
		{
			this.CharacterDescription.ShowRoleTooltip(translatedText);
		}

		public void ShowSkillTooltip(GadgetInfo gadget)
		{
			this.CharacterDescription.ShowSkillTooltip(gadget);
		}

		public void HideTooltip()
		{
			if (this.CharacterDescription == null)
			{
				return;
			}
			this.CharacterDescription.HideTooltip();
		}

		public void QuitApplication()
		{
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenCloseGameConfirmWindow(delegate
			{
				try
				{
					if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
					{
						GameHubBehaviour.Hub.Swordfish.Msg.Cleanup();
					}
				}
				catch (Exception ex)
				{
				}
			});
		}

		private void Loading_OnHidingAnimationCompleted()
		{
			if (this.PickScreenHints != null)
			{
				if (SpectatorController.IsSpectating)
				{
					UnityEngine.Object.Destroy(this.PickScreenHints.gameObject);
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
			TeamUtils.GetGroupTeamAsync(GameHubBehaviour.Hub, TeamKind.Blue, delegate(Team team)
			{
				this.SetGroupTeamInfo(TeamKind.Blue, team);
			}, delegate(Exception exception)
			{
				PickModeGUI.Log.Error(string.Format("Error on GetGroupTeamAsync [{0}]. Exception:{1}", TeamKind.Blue, exception));
			});
			TeamUtils.GetGroupTeamAsync(GameHubBehaviour.Hub, TeamKind.Red, delegate(Team team)
			{
				this.SetGroupTeamInfo(TeamKind.Red, team);
			}, delegate(Exception exception)
			{
				PickModeGUI.Log.Error(string.Format("Error on GetGroupTeamAsync [{0}]. Exception:{1}", TeamKind.Red, exception));
			});
		}

		private void SetGroupTeamInfo(TeamKind teamKind, Team team)
		{
			if (team == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == teamKind)
			{
				this.AllyTeamIconSprite.SpriteName = team.ImageUrl;
				this.AllyTeamNameLabel.text = NGUIText.EscapeSymbols(string.Format("[{0}]", team.Tag));
				this.AllyTeamGameObject.SetActive(true);
			}
			else if (SpectatorController.IsSpectating)
			{
				this.EnemyTeamIconSprite.SpriteName = team.ImageUrl;
				this.EnemyTeamNameLabel.text = NGUIText.EscapeSymbols(string.Format("[{0}]", team.Tag));
				this.EnemyTeamGameObject.SetActive(true);
			}
		}

		public void OnCharacterIconHoverOver(CharacterConfig characterConfig)
		{
			this.PickdModeStatusInfo.Setup(characterConfig.CharInfo);
			this.PickdModeStatusInfo.SetVisibility(true);
		}

		public void OnCharacterIconHoverOut()
		{
			this.PickdModeStatusInfo.SetVisibility(false);
		}

		protected static readonly BitLogger Log = new BitLogger(typeof(PickModeGUI));

		private PickModeSetup _pickMode;

		[Header("Timer")]
		public UILabel TimerLabel;

		public Color CountdownTimerColor;

		private float _lastTime;

		private bool _countdownAudioPlayed;

		private int _countdownToAutoPickAudioPlayed;

		[Header("Audio")]
		public FMODAsset ConfirmCharacterPickAudio;

		public FMODAsset ConfirmSkinPickAudio;

		public FMODAsset CountdownToAutoPickupAudio;

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

		public PilotDescriptionConfig CharacterDescription;

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

		private int _alliedClosedPicksCount;

		private int _alliedCount;

		private int _alliedPlayersCount;

		private int _enemyClosedPicksCount;

		private int _enemyCount;

		private bool _pickConfirmedByServer;

		private List<UIKeyNavigation> _keyNavigations;

		private float _uiRootScale;

		private float _halfTooltipWidth;

		private float _screenAspectRatio;

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
	}
}
