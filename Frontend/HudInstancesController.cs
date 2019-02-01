using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using FMod;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudInstancesController : HudWindow
	{
		private GameGui GameGui
		{
			get
			{
				return this._gameGui;
			}
		}

		public static bool IsInShopState()
		{
			return GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.Shop;
		}

		public void Start()
		{
			this._alreadyShowInCurrentRound = false;
			this._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>();
			this.Shop.Init();
			this.MatchHighlights.Init();
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnListenToPhaseChange;
			GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.OnVisibilityChange += this.OtherCompetingWindowOnVisibilityChange;
			this.WindowGameObject.SetActive(false);
			if (!GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this.TryToShowInstances();
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			this.GarageController = null;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnListenToPhaseChange;
			GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.OnVisibilityChange -= this.OtherCompetingWindowOnVisibilityChange;
			this._gameGui = null;
		}

		private bool OtherCompetingWindowNotVisible()
		{
			return !GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.IsWindowVisible();
		}

		private void BombManagerOnListenToPhaseChange(BombScoreBoard.State state)
		{
			if (!GameHubBehaviour.Hub.BombManager.Rules.InstancesEnabled)
			{
				return;
			}
			if (state != BombScoreBoard.State.Shop)
			{
				base.SetWindowVisibility(false);
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.Round > 0)
			{
				base.StartCoroutine(this.TryToShowInstancesDelayed(this.OnPhaseChangeDelayAfterBombDeliveryInSec));
			}
			else
			{
				this.TryToShowInstances();
			}
		}

		private IEnumerator TryToShowInstancesDelayed(float timeInSec)
		{
			yield return new WaitForSeconds(timeInSec);
			this.TryToShowInstances();
			yield break;
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			base.ChangeWindowVisibility(visible);
			if (!visible)
			{
				this.TransitionMessage.Hide();
				if (HudInstancesController.IsInShopState())
				{
					GUIUtils.PlayAnimation(this.BgAnimation, true, 1f, string.Empty);
					this.Shop.Hide();
					this.MatchHighlights.Hide();
					this.Timer.Hide();
				}
				else
				{
					this.Shop.HideAnimating();
					this.MatchHighlights.HideAnimating();
					this.Timer.HideAnimating();
					if (this._confirmationAudio != null)
					{
						FMODAudioManager.PlayOneShotAt(this._confirmationAudio, Vector3.zero, 0);
					}
					base.StartCoroutine(this.HideAnimating());
				}
			}
		}

		private IEnumerator HideAnimating()
		{
			yield return new WaitForSeconds(this.DelayBeforeBgOutInSec);
			if (this.WindowGameObject.activeInHierarchy)
			{
				GUIUtils.PlayAnimation(this.BgAnimation, true, 1f, string.Empty);
			}
			yield break;
		}

		public override void AnimationOnWindowExit()
		{
			base.AnimationOnWindowExit();
			if (!this.IsVisible)
			{
				this.Shop.Hide();
				this.Timer.Hide();
				this.MatchHighlights.Hide();
				this.TransitionMessage.Hide();
				if (!HudInstancesController.IsInShopState())
				{
					this._alreadyShowInCurrentRound = false;
				}
			}
		}

		private void OtherCompetingWindowOnVisibilityChange(bool visible)
		{
			if (HudInstancesController.IsInShopState())
			{
				if (!visible)
				{
					this.TryToShowInstances();
				}
				else
				{
					base.SetWindowVisibility(false);
				}
			}
		}

		public override bool CanOpen()
		{
			return base.CanOpen() && this.OtherCompetingWindowNotVisible();
		}

		private void TryToShowInstances()
		{
			bool flag = GameHubBehaviour.Hub.Match.LevelIsTutorial();
			if (((!HudInstancesController.IsInShopState() || !this.CanOpen()) && !flag) || !GameHubBehaviour.Hub.BombManager.Rules.InstancesEnabled)
			{
				return;
			}
			this.Shop.SetGarageController(this.GarageController);
			bool isSpectating = SpectatorController.IsSpectating;
			BombScoreBoard scoreBoard = GameHubBehaviour.Hub.BombManager.ScoreBoard;
			bool flag2 = scoreBoard.Round == 0;
			if ((scoreBoard.MatchOver && !flag) || (flag2 && isSpectating))
			{
				return;
			}
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.SetWindowVisibility(false);
			base.SetWindowVisibility(true);
			GUIUtils.PlayAnimation(this.BgAnimation, false, 1f, string.Empty);
			if (flag || flag2)
			{
				base.StartCoroutine(this.ShowShopDelayed(HudInstancesTimer.RoundType.First, flag));
				return;
			}
			bool flag3 = scoreBoard.Rounds[scoreBoard.Round - 1].DeliverTeam == GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			if (isSpectating || flag3)
			{
				this.Timer.Show(HudInstancesTimer.RoundType.Win);
				this.MatchHighlights.Show();
				if (!this._alreadyShowInCurrentRound)
				{
					this._alreadyShowInCurrentRound = true;
					FMODAudioManager.PlayOneShotAt(this._highlightEnterAudio, Vector3.zero, 0);
				}
				return;
			}
			base.StartCoroutine(this.ShowShopDelayed(HudInstancesTimer.RoundType.Lose, false));
		}

		private IEnumerator ShowShopDelayed(HudInstancesTimer.RoundType roundType, bool isTutorial)
		{
			if (this._alreadyShowInCurrentRound)
			{
				this.Shop.Show(isTutorial);
				if (!isTutorial)
				{
					this.Timer.Show(roundType);
				}
				yield break;
			}
			this._alreadyShowInCurrentRound = true;
			this.TransitionMessage.Show();
			FMODAudioManager.PlayOneShotAt(this._transitionMessageAudio, Vector3.zero, 0);
			yield return new WaitForSeconds(this.DelayToOpenShopAfterTransitionInSec);
			if (this.IsVisible)
			{
				this.Shop.Show(isTutorial);
				if (!isTutorial)
				{
					this.Timer.Show(roundType);
				}
			}
			yield break;
		}

		public void TutorialShowInstances(bool show)
		{
			if (!show)
			{
				base.SetWindowVisibility(false);
				return;
			}
			this.TryToShowInstances();
		}

		public float OnPhaseChangeDelayAfterBombDeliveryInSec = 2f;

		public float DelayToOpenShopAfterTransitionInSec = 1f;

		public float DelayBeforeBgOutInSec = 0.5f;

		public HudInstancesShop Shop;

		public HudInstancesMatchHighlights MatchHighlights;

		public HudInstancesTimer Timer;

		public HudInstancesTransitionMessage TransitionMessage;

		public Animation BgAnimation;

		[NonSerialized]
		public GarageController GarageController;

		[Header("[Audio]")]
		[SerializeField]
		private FMODAsset _confirmationAudio;

		[SerializeField]
		private FMODAsset _highlightEnterAudio;

		[SerializeField]
		private FMODAsset _transitionMessageAudio;

		private GameGui _gameGui;

		private bool _alreadyShowInCurrentRound;
	}
}
