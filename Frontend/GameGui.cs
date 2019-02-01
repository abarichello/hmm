using System;
using System.Collections;
using System.Diagnostics;
using HeavyMetalMachines.Battlepass;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class GameGui : StateGuiController
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action ListenToGameGuiCreation;

		private void OnEnable()
		{
			GameHubBehaviour.Hub.CursorManager.ShowAndSetCursor(true, CursorManager.CursorTypes.GameCursor);
			this.HudChatController.gameObject.SetActive(!GameHubBehaviour.Hub.Match.LevelIsTutorial());
			if (GameGui.ListenToGameGuiCreation != null)
			{
				GameGui.ListenToGameGuiCreation();
				GameGui.ListenToGameGuiCreation = null;
			}
			this.HudTabController.OnVisibilityChange += this.OnOtherVisibilityChange;
			SpectatorModalGUI.OnModalVisibilityChanged += this.OnOtherVisibilityChange;
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.OnVisibilityChange += this.OnOtherVisibilityChange;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToPreObjectSpawn += this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning += this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.ShowElementsAfterRespawn;
		}

		private void OnDisable()
		{
			this.HudTabController.OnVisibilityChange -= this.OnOtherVisibilityChange;
			SpectatorModalGUI.OnModalVisibilityChanged -= this.OnOtherVisibilityChange;
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.OnVisibilityChange -= this.OnOtherVisibilityChange;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToPreObjectSpawn -= this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning -= this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.ShowElementsAfterRespawn;
		}

		private void Start()
		{
			GameGui.StaticUIGadgetConstructor = this.UIGadgetConstructor;
		}

		private void OnDestroy()
		{
			GameGui.ListenToGameGuiCreation = null;
		}

		private void HideSomeElementsToRespawn(PlayerEvent data)
		{
			if (data.TargetId != GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId || GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreBoard.State.BombDelivery || GameHubBehaviour.Hub.Players.CurrentPlayerData.IsBotControlled)
			{
				return;
			}
			GameGui.HudElement hudElements = (GameGui.HudElement)2147483479;
			this.SetHudVisibility(hudElements, false);
		}

		private void ShowElementsAfterRespawn(PlayerEvent data)
		{
			if (data.TargetId != GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId || GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreBoard.State.BombDelivery || GameHubBehaviour.Hub.Players.CurrentPlayerData.IsBotControlled)
			{
				return;
			}
			this.SetHudVisibility(GameGui.HudElement.All, true);
		}

		public void ClearBackToMain()
		{
			((Game)GameHubBehaviour.Hub.State.Current).ClearBackToMain();
		}

		private void Minimize()
		{
			GameHubBehaviour.Hub.GuiScripts.ScreenResolution.Minimize();
		}

		public void OnFriendInviteSent()
		{
			this.OkWindowFeedback("InviteSent", "MainMenuGui", new object[0]);
		}

		public void OkWindowFeedback(string key, string tab, params object[] param)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = string.Format(Language.Get(key, tab), param),
				OkButtonText = Language.Get("Ok", "GUI"),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void OnOtherVisibilityChange(bool otherVisibility)
		{
			this.ShowGameHud(!otherVisibility);
		}

		public void ShowGameHud(bool visibility)
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				return;
			}
			bool flag = GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.BombDelivery || GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.PreBomb;
			bool visibility2 = visibility && flag && !GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.IsWindowVisible() && !this.HudTabController.IsWindowVisible() && !GameHubBehaviour.Hub.GuiScripts.DriverHelper.IsWindowVisible();
			this.SetHudVisibility(GameGui.HudElement.All, visibility2);
		}

		public void SetHudVisibility(GameGui.HudElement hudElements, bool visibility)
		{
			if (hudElements.HasFlag(GameGui.HudElement.Gadget))
			{
				this.UIGadgetConstructor.SetWindowVisibility(visibility);
			}
			if (hudElements.HasFlag(GameGui.HudElement.MiniMap) && this.HudMinimapUiController != null)
			{
				this.HudMinimapUiController.SetVisibility(visibility, false);
			}
			if (hudElements.HasFlag(GameGui.HudElement.Players))
			{
				this.HudPlayersController.SetWindowVisibility(visibility);
			}
			if (hudElements.HasFlag(GameGui.HudElement.TopScore))
			{
				this.HudScoreController.SetWindowVisibility(visibility);
				this.OvertimeTextController.SetVisibility(visibility);
			}
			if (hudElements.HasFlag(GameGui.HudElement.Respawn))
			{
				this.RespawnController.SetVisibility(visibility);
			}
		}

		public void ShowEndGameBackground(Action callback)
		{
			base.StartCoroutine(this.ShowEndGameBackgroundCoroutine(callback));
		}

		private IEnumerator ShowEndGameBackgroundCoroutine(Action callback)
		{
			this._endGameBackground.gameObject.SetActive(true);
			TweenAlpha.Begin(this._endGameBackground.gameObject, 0.5f, 1f);
			yield return new WaitForSeconds(0.5f);
			if (callback != null)
			{
				callback();
			}
			yield break;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GameGui));

		public Transform HudIconsTransform;

		public HudChatController HudChatController;

		public HudLifebarController HudLifebarController;

		public HudTabController HudTabController;

		public CombatTextManager CombatTextManager;

		public HudMegafeedbacksController HudMegafeedbacksController;

		public BombTipWindow bombTipWindow;

		public BattlepassComponent BattlepassComponent;

		public BattlepassProgressScriptableObject BattlepassProgressScriptableObject;

		[NonSerialized]
		public UIProgressionController EndGame;

		[NonSerialized]
		public HudWinnerController HudWinnerController;

		[Header("Game Hud Elements To Be Show OnStartup")]
		[SerializeField]
		public HudPlayersController HudPlayersController;

		[SerializeField]
		public UIGadgetConstructor UIGadgetConstructor;

		[SerializeField]
		public HudMinimapUiController HudMinimapUiController;

		[SerializeField]
		public HudScoreController HudScoreController;

		[SerializeField]
		public OvertimeTextController OvertimeTextController;

		[SerializeField]
		public HudRespawnController RespawnController;

		public GameObject Hud;

		[Header("End Game")]
		[SerializeField]
		private UI2DSprite _endGameBackground;

		public static UIGadgetConstructor StaticUIGadgetConstructor;

		[Flags]
		public enum HudElement
		{
			None = 0,
			Portrait = 1,
			Gadget = 2,
			MiniMap = 4,
			TopScore = 8,
			ShopButton = 16,
			Players = 32,
			Radar = 64,
			Respawn = 128,
			All = 2147483647
		}
	}
}
