using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class HudGadgetCursorController : GameHubBehaviour
	{
		protected void Start()
		{
			this.SetVisibility(false);
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback += this.OnCurrentPlayerCreated;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnListenToPhaseChange;
			GameHubBehaviour.Hub.CursorManager.ChangeVisibilityCallback += this.CursorManagerOnChangeVisibilityCallback;
			GameHubBehaviour.Hub.CursorManager.CursorTypeChangedCallback += this.CursorManagerOnCursorTypeChangedCallback;
		}

		protected void OnDestroy()
		{
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback -= this.OnCurrentPlayerCreated;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnListenToPhaseChange;
			GameHubBehaviour.Hub.CursorManager.ChangeVisibilityCallback -= this.CursorManagerOnChangeVisibilityCallback;
			GameHubBehaviour.Hub.CursorManager.CursorTypeChangedCallback -= this.CursorManagerOnCursorTypeChangedCallback;
			if (this._combatObject)
			{
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget0).ListenToGadgetReady -= this.OnListenToGadget0Ready;
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget1).ListenToGadgetReady -= this.OnListenToGadget1Ready;
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.BoostGadget).ListenToGadgetReady -= this.OnListenToGadgetBoostReady;
				this._combatObject.ListenToObjectSpawn -= this.CombatObjectOnListenToObjectSpawn;
				this._combatObject.ListenToObjectUnspawn -= this.CombatObjectOnListenToObjectUnspawn;
			}
			this._combatObject = null;
		}

		private void SetVisibility(bool isVisible)
		{
			this._mainCanvasGroup.alpha = ((!isVisible) ? 0f : 1f);
		}

		private bool CanBeVisible(BombScoreBoard.State state)
		{
			bool flag = state == BombScoreBoard.State.Warmup || state == BombScoreBoard.State.Shop || state == BombScoreBoard.State.PreBomb || state == BombScoreBoard.State.BombDelivery || state == BombScoreBoard.State.PreReplay || state == BombScoreBoard.State.Replay;
			return this.IsEnabledInOptions() && (flag || GameHubBehaviour.Hub.Match.LevelIsTutorial());
		}

		private bool CanRenderVfx(BombScoreBoard.State state)
		{
			bool flag = state != BombScoreBoard.State.PreReplay && state != BombScoreBoard.State.Replay && state != BombScoreBoard.State.EndGame;
			return flag && this._playerIsAlive;
		}

		private void BombManagerOnListenToPhaseChange(BombScoreBoard.State state)
		{
			if (!this.CanRenderVfx(state))
			{
				for (int i = 0; i < this.GadgetInfos.Length; i++)
				{
					this.ResetGadgetInfo(this.GadgetInfos[i]);
				}
			}
			this.SetVisibility(this.CanBeVisible(state));
		}

		private void CursorManagerOnCursorTypeChangedCallback(CursorManager.CursorTypes cursorType)
		{
			if (cursorType == CursorManager.CursorTypes.GameCursor)
			{
				GameHubBehaviour.Hub.CursorManager.ShowAndSetCursor(!this.IsEnabledInOptions(), CursorManager.CursorTypes.GameCursor);
			}
		}

		private void CursorManagerOnChangeVisibilityCallback(bool visible)
		{
			this.SetVisibility(!visible && this.CanBeVisible(GameHubBehaviour.Hub.BombManager.CurrentBombGameState));
		}

		private bool IsEnabledInOptions()
		{
			return GameHubBehaviour.Hub.Options.Game.ShowGadgetsCursor;
		}

		public void Update()
		{
			if (!this._initialized || this._mainCanvasGroup.alpha < 0.001f)
			{
				return;
			}
			if (this.CanRenderVfx(GameHubBehaviour.Hub.BombManager.CurrentBombGameState))
			{
				this.RenderGadgetUpdate(this.GadgetInfos[0], GadgetSlot.CustomGadget0, this._combatObject.CustomGadget0);
				this.RenderGadgetUpdate(this.GadgetInfos[1], GadgetSlot.CustomGadget1, this._combatObject.CustomGadget1);
				this.RenderGadgetUpdate(this.GadgetInfos[2], GadgetSlot.BoostGadget, this._combatObject.BoostGadget);
			}
		}

		private void RenderGadgetUpdate(HudGadgetCursorController.GadgetInfo gadgetInfo, GadgetSlot gadgetSlot, GadgetBehaviour gadgetBehaviour)
		{
			if (this.TryRenderCustomGadgetUpdate(gadgetInfo))
			{
				return;
			}
			GadgetData.GadgetStateObject gadgetState = this._combatObject.GadgetStates.GetGadgetState(gadgetSlot);
			if (gadgetState.GadgetState == GadgetState.Cooldown)
			{
				long num = gadgetState.CoolDown - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
				float num2 = (float)num * 0.001f / gadgetBehaviour.Cooldown;
				gadgetInfo.FillImage.fillAmount = 1f - num2;
				if (gadgetInfo.BaseCanvasGroup.alpha > 0.001f)
				{
					gadgetInfo.BaseCanvasGroup.alpha = 0f;
					gadgetInfo.BaseTransparentCanvasGroup.alpha = 1f;
				}
			}
			else if (gadgetInfo.FillImage.fillAmount > 0.001f)
			{
				this.ResetGadgetInfo(gadgetInfo);
			}
		}

		private bool TryRenderCustomGadgetUpdate(HudGadgetCursorController.GadgetInfo gadgetInfo)
		{
			CombatGadget combatGadget;
			if (!this._combatObject.CustomGadgets.TryGetValue(gadgetInfo.Slot, out combatGadget))
			{
				return false;
			}
			float fillAmount = 0f;
			if (combatGadget.HasCooldownParameters())
			{
				int playbackTime = GameHubBehaviour.Hub.Clock.GetPlaybackTime();
				int cooldownEndTime = combatGadget.GetCooldownEndTime();
				bool flag = cooldownEndTime > playbackTime;
				if (flag)
				{
					float num = (float)(cooldownEndTime - playbackTime) * 0.001f;
					float cooldownTotalTime = combatGadget.GetCooldownTotalTime();
					if (cooldownTotalTime > 0f)
					{
						fillAmount = 1f - num / cooldownTotalTime;
					}
					if (gadgetInfo.BaseCanvasGroup.alpha > 0.001f)
					{
						gadgetInfo.BaseCanvasGroup.alpha = 0f;
						gadgetInfo.BaseTransparentCanvasGroup.alpha = 1f;
					}
				}
				else if (gadgetInfo.BaseTransparentCanvasGroup.alpha > 0.001f)
				{
					this.PlayGadgetInfoGlowAnimation(gadgetInfo);
					this.ResetGadgetInfo(gadgetInfo);
				}
			}
			gadgetInfo.FillImage.fillAmount = fillAmount;
			return true;
		}

		private void ResetGadgetInfo(HudGadgetCursorController.GadgetInfo gadgetInfo)
		{
			gadgetInfo.FillImage.fillAmount = 0f;
			if (gadgetInfo.BaseTransparentCanvasGroup.alpha > 0.001f)
			{
				gadgetInfo.BaseCanvasGroup.alpha = 1f;
				gadgetInfo.BaseTransparentCanvasGroup.alpha = 0f;
			}
		}

		private void OnCurrentPlayerCreated(PlayerEvent obj)
		{
			this._combatObject = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<CombatObject>();
			this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget0).ListenToGadgetReady += this.OnListenToGadget0Ready;
			this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget1).ListenToGadgetReady += this.OnListenToGadget1Ready;
			this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.BoostGadget).ListenToGadgetReady += this.OnListenToGadgetBoostReady;
			this._combatObject.ListenToObjectSpawn += this.CombatObjectOnListenToObjectSpawn;
			this._combatObject.ListenToObjectUnspawn += this.CombatObjectOnListenToObjectUnspawn;
			this._playerIsAlive = this._combatObject.IsAlive();
			GameHubBehaviour.Hub.CursorManager.ShowAndSetCursor(!this.IsEnabledInOptions(), CursorManager.CursorTypes.GameCursor);
			this.SetVisibility(this.CanBeVisible(GameHubBehaviour.Hub.BombManager.CurrentBombGameState));
			this._initialized = true;
		}

		private void OnListenToGadget0Ready()
		{
			this.PlayGadgetInfoGlowAnimation(this.GadgetInfos[0]);
		}

		private void OnListenToGadget1Ready()
		{
			this.PlayGadgetInfoGlowAnimation(this.GadgetInfos[1]);
		}

		private void OnListenToGadgetBoostReady()
		{
			this.PlayGadgetInfoGlowAnimation(this.GadgetInfos[2]);
		}

		private void PlayGadgetInfoGlowAnimation(HudGadgetCursorController.GadgetInfo gadgetInfo)
		{
			if (this.CanRenderVfx(GameHubBehaviour.Hub.BombManager.CurrentBombGameState))
			{
				gadgetInfo.GlowAnimation.Play();
			}
		}

		private void CombatObjectOnListenToObjectSpawn(CombatObject combatObject, SpawnEvent msg)
		{
			this._playerIsAlive = true;
		}

		private void CombatObjectOnListenToObjectUnspawn(CombatObject combatObject, UnspawnEvent msg)
		{
			this._playerIsAlive = false;
			for (int i = 0; i < this.GadgetInfos.Length; i++)
			{
				this.ResetGadgetInfo(this.GadgetInfos[i]);
			}
		}

		[SerializeField]
		private CanvasGroup _mainCanvasGroup;

		[SerializeField]
		private HudGadgetCursorController.GadgetInfo[] GadgetInfos;

		private CombatObject _combatObject;

		private bool _initialized;

		private bool _playerIsAlive;

		[Serializable]
		private struct GadgetInfo
		{
			public GadgetSlot Slot;

			public Image FillImage;

			public CanvasGroup BaseCanvasGroup;

			public CanvasGroup BaseTransparentCanvasGroup;

			public Animation GlowAnimation;
		}
	}
}
