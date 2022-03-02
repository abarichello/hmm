using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class PlayerRaceStartCursorLockController : GameHubObject, IDisposable
	{
		public PlayerRaceStartCursorLockController(IConfigLoader config)
		{
			this.Initialize(config);
		}

		private void Initialize(IConfigLoader config)
		{
			GameHubObject.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
			if (GameHubObject.Hub.GuiScripts)
			{
				GameHubObject.Hub.GuiScripts.LoadingVersus.OnPreHideLoading += this.OnLockCursorEvent;
			}
			LogoTransition.OnAnimationMiddle += this.OnLockCursorEvent;
			this._cursorLockTimer.UseUnscaledTime = true;
			this._isRaceStartCursorLockEnabled = config.GetBoolValue(ConfigAccess.RaceStartCursorLockEnable, true);
		}

		public void Dispose()
		{
			GameHubObject.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			if (GameHubObject.Hub.GuiScripts)
			{
				GameHubObject.Hub.GuiScripts.LoadingVersus.OnPreHideLoading -= this.OnLockCursorEvent;
			}
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

		private void OnPhaseChange(BombScoreboardState state)
		{
			if (state == BombScoreboardState.BombDelivery)
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
			return GameHubObject.Hub.BombManager.CurrentBombGameState == BombScoreboardState.Warmup || GameHubObject.Hub.BombManager.CurrentBombGameState == BombScoreboardState.Shop || GameHubObject.Hub.BombManager.CurrentBombGameState == BombScoreboardState.PreBomb;
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
			IGameArenaInfo currentArena = GameHubObject.Hub.ArenaConfig.GetCurrentArena();
			if (currentArena == null || currentArena.CursorLockTimeInSeconds <= 0f)
			{
				PlayerRaceStartCursorLockController.Log.Debug("Cursor lock is not enabled for current arena.");
				return;
			}
			GameHubObject.Hub.CursorManager.LockCursorByOffset(currentArena.CursorLockOffset.x, currentArena.CursorLockOffset.y);
			float num = currentArena.CursorLockTimeInSeconds * 1000f;
			this._cursorLockTimer.PeriodMillis = (int)num;
			this._cursorLockTimer.Reset();
			PlayerRaceStartCursorLockController.Log.DebugFormat("Triggering CURSOR LOCK. Lock time is {0} milliseconds", new object[]
			{
				num
			});
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
			PlayerRaceStartCursorLockController.Log.Debug("Triggering CURSOR UNLOCK.");
			GameHubObject.Hub.CursorManager.UnlockCursorByOffset();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PlayerRaceStartCursorLockController));

		private TimedUpdater _cursorLockTimer;

		private bool _isRaceStartCursorLockEnabled;
	}
}
