using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class PlayerRaceStartCursorLockController : GameHubObject, IRaceStartCursorLockController, IDisposable
	{
		public PlayerRaceStartCursorLockController()
		{
			this.Initialize();
		}

		private void Initialize()
		{
			GameHubObject.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
			GameHubObject.Hub.GuiScripts.LoadingVersus.OnPreHideLoading += this.OnLockCursorEvent;
			LogoTransition.OnAnimationMiddle += this.OnLockCursorEvent;
			this._cursorLockTimer.UseUnscaledTime = true;
			this._isRaceStartCursorLockEnabled = GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.RaceStartCursorLockEnable, true);
		}

		public void Dispose()
		{
			GameHubObject.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			GameHubObject.Hub.GuiScripts.LoadingVersus.OnPreHideLoading -= this.OnLockCursorEvent;
			LogoTransition.OnAnimationMiddle -= this.OnLockCursorEvent;
		}

		public void FreeCursorLockIfTimeout()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			if (this._cursorLockTimer.PeriodMillis <= 0)
			{
				return;
			}
			if (this._cursorLockTimer.ShouldHalt())
			{
				return;
			}
			this._cursorLockTimer.PeriodMillis = 0;
			this.UnlockCursorByOffset();
		}

		private void OnPhaseChange(BombScoreBoard.State state)
		{
			if (state == BombScoreBoard.State.BombDelivery)
			{
				this.UnlockCursorByOffset();
			}
		}

		private void OnLockCursorEvent()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			if (this.IsInvalidBombGameStateToLockCursor())
			{
				return;
			}
			if (this.IsInvalidGuiStateToLockCursor())
			{
				return;
			}
			this.LockCursorByOffset();
		}

		private bool IsInvalidBombGameStateToLockCursor()
		{
			return !this.IsValidBombGameStateToLockCursor();
		}

		private bool IsValidBombGameStateToLockCursor()
		{
			return GameHubObject.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.Warmup || GameHubObject.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.Shop || GameHubObject.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.PreBomb;
		}

		private bool IsInvalidGuiStateToLockCursor()
		{
			return GameHubObject.Hub.GuiScripts.Esc.IsWindowVisible() || GameHubObject.Hub.GuiScripts.Esc.IsOptionsWindowVisible();
		}

		private void LockCursorByOffset()
		{
			if (!this._isRaceStartCursorLockEnabled)
			{
				return;
			}
			if (GameHubObject.Hub.CursorManager.IsCursorLockedByOffset())
			{
				return;
			}
			GameArenaInfo currentArena = GameHubObject.Hub.ArenaConfig.GetCurrentArena();
			if (currentArena.CursorLockTimeInSeconds <= 0f)
			{
				PlayerRaceStartCursorLockController.Log.Info("Cursor lock is not enabled for current arena.");
				return;
			}
			GameHubObject.Hub.CursorManager.LockCursorByOffset(currentArena.CursorLockOffset.x, currentArena.CursorLockOffset.y);
			float num = currentArena.CursorLockTimeInSeconds * 1000f;
			this._cursorLockTimer.PeriodMillis = (int)num;
			this._cursorLockTimer.Reset();
		}

		private void UnlockCursorByOffset()
		{
			if (!this._isRaceStartCursorLockEnabled)
			{
				return;
			}
			if (!GameHubObject.Hub.CursorManager.IsCursorLockedByOffset())
			{
				return;
			}
			GameHubObject.Hub.CursorManager.UnlockCursorByOffset();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PlayerRaceStartCursorLockController));

		private TimedUpdater _cursorLockTimer;

		private bool _isRaceStartCursorLockEnabled;
	}
}
