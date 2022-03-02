using System;
using System.Collections;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.VFX;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudDeliveryBombController : GameHubBehaviour, ICleanupListener
	{
		private GameGui GameGui
		{
			get
			{
				GameGui result;
				if ((result = this._gameGui) == null)
				{
					result = (this._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>());
				}
				return result;
			}
		}

		private void OnEnable()
		{
			this.HideWindow();
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnPhaseChange;
			this.waitReplayRoundDelaySeconds = new WaitForSeconds(this.ReplayRoundDelayInSec);
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnPhaseChange;
		}

		public void Cleanup()
		{
			HudDeliveryBombController.Log.Debug("cleanup");
			this._gameGui = null;
		}

		private void HideWindow()
		{
			this.HudDeliveryGuiComponents.WindowGameObject.SetActive(false);
		}

		private void ShowWindow()
		{
			this.HudDeliveryGuiComponents.WindowGameObject.SetActive(true);
		}

		private void BombManagerOnPhaseChange(BombScoreboardState bombScoreBoardState)
		{
			HudDeliveryBombController.Log.DebugFormat("Entering state {0}", new object[]
			{
				bombScoreBoardState
			});
			switch (bombScoreBoardState)
			{
			case BombScoreboardState.Warmup:
				HudDeliveryBombController.Log.Debug("BombManagerOnPhaseChange - Warmup phase.");
				this.ShowPlayerInfo(false);
				this.GameGui.ShowGameHud(false);
				this.HideWindow();
				break;
			case BombScoreboardState.PreBomb:
				HudDeliveryBombController.Log.Debug("BombManagerOnPhaseChange - Prebomb phase.");
				this.ShowPlayerInfo(false);
				this.GameGui.ShowGameHud(true);
				this.HideWindow();
				break;
			case BombScoreboardState.BombDelivery:
				HudDeliveryBombController.Log.Debug("BombManagerOnPhaseChange - Delivery phase.");
				this.ShowPlayerInfo(false);
				this.GameGui.ShowGameHud(true);
				this.HideWindow();
				break;
			case BombScoreboardState.PreReplay:
				HudDeliveryBombController.Log.Debug("BombManagerOnPhaseChange - PreReplay phase.");
				this.ShowPlayerInfo(false);
				this.GameGui.ShowGameHud(false);
				this.HideWindow();
				break;
			case BombScoreboardState.Replay:
				HudDeliveryBombController.Log.Debug("BombManagerOnPhaseChange - Replay phase.");
				this.GameGui.ShowGameHud(false);
				this.ShowPlayerInfo(false);
				base.StartCoroutine(this.StartReplayAsync());
				break;
			case BombScoreboardState.Shop:
				HudDeliveryBombController.Log.Debug("BombManagerOnPhaseChange - Shop phase.");
				this.ShowPlayerInfo(false);
				this.GameGui.ShowGameHud(false);
				this.HideWindow();
				break;
			case BombScoreboardState.EndGame:
				HudDeliveryBombController.Log.Debug("BombManagerOnPhaseChange - EndGame phase.");
				this.ShowPlayerInfo(false);
				this.GameGui.ShowGameHud(false);
				this.HideWindow();
				break;
			default:
				HudDeliveryBombController.Log.ErrorFormat("BombManagerOnPhaseChange - Unknown phase={0}", new object[]
				{
					bombScoreBoardState
				});
				break;
			}
		}

		private IEnumerator StartReplayAsync()
		{
			yield return this.waitReplayRoundDelaySeconds;
			this.ReplayAnimation.gameObject.SetActive(true);
			this.ShowWindow();
			yield break;
		}

		private void ShowPlayerInfo(bool isActive)
		{
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this.Cleanup();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudDeliveryBombController));

		public float ReplayRoundDelayInSec = 2f;

		[SerializeField]
		protected Color AllyColor;

		[SerializeField]
		protected Color EnemyColor;

		[Header("[GUI]")]
		public HudDeliveryBombController.GuiComponents HudDeliveryGuiComponents;

		public Animation ReplayAnimation;

		private GameGui _gameGui;

		private WaitForSeconds waitReplayRoundDelaySeconds;

		private bool _isPreReplayCoroutineRunning;

		[Serializable]
		public struct GuiComponents
		{
			public GameObject WindowGameObject;

			[Header("[Player]")]
			public GameObject PlayerGroupGameObject;

			public UILabel PlayerTitleInfoLabel;

			public UILabel PlayerNameLabel;

			public UILabel PlayerCharacterNameLabel;

			public UI2DSprite PlayerBorderSprite;

			[Header("[Team]")]
			public GameObject TeamGroupGameObject;

			public HMMUI2DDynamicSprite TeamIconSprite;

			public UI2DSprite TeamBorderSprite;

			public Sprite TeamBorderAllySprite;

			public Sprite TeamBorderEnemySprite;

			[Header("[Score]")]
			public GameObject ScoreGroupGameObject;

			public UILabel ScoreAllyLabel;

			public UILabel ScoreEnemyLabel;
		}
	}
}
