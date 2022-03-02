using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HeavyMetalMachines.Swordfish;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Windows
{
	internal class WindowsGameQuitHandler : IGameQuitHandler, IDisposable
	{
		public WindowsGameQuitHandler(SwordfishServices swordfish, bool IsServer)
		{
			this._gameQuitReasons = Enum.GetNames(typeof(GameQuitReason));
			this._isServer = IsServer;
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.EnableAltF4Hack))
			{
				return;
			}
			this._swordfishServices = swordfish;
			this._window = Platform.Current.GetCurrentWindowHandle(Application.productName);
			this._customWinProcDelegate = new WindowsGameQuitHandler.WindowProcDelegate(this.CustomWinProc);
			this._customWinProc = Marshal.GetFunctionPointerForDelegate(this._customWinProcDelegate);
			this._originalWinProc = this.ChangeWindowProc(this._customWinProc);
		}

		private IntPtr CustomWinProc(IntPtr hWnd, User32.WindowMessage message, IntPtr wParam, IntPtr lParam)
		{
			if (message != User32.WindowMessage.SysKeydown)
			{
				if (message != User32.WindowMessage.Close)
				{
					if (message == User32.WindowMessage.SysCommand)
					{
						if (this.HandleSysCommandMessage(hWnd, message, wParam, lParam))
						{
							return IntPtr.Zero;
						}
					}
				}
				else if (this.HandleCloseMessage(hWnd, message, wParam, lParam))
				{
					return IntPtr.Zero;
				}
			}
			else if (this.HandleSysKeyDownMessage(hWnd, message, wParam, lParam))
			{
				return IntPtr.Zero;
			}
			return User32.CallWindowProc(this._originalWinProc, hWnd, message, wParam, lParam);
		}

		private bool HandleSysKeyDownMessage(IntPtr hWnd, User32.WindowMessage message, IntPtr wParam, IntPtr lParam)
		{
			if (wParam.ToInt32() != 115)
			{
				return false;
			}
			this._pressedAltF4 = true;
			return false;
		}

		private bool HandleSysCommandMessage(IntPtr hWnd, User32.WindowMessage message, IntPtr wParam, IntPtr lParam)
		{
			if (wParam.ToInt32() != 61536)
			{
				return false;
			}
			int xlparam = User32.GetXLParam(lParam);
			int ylparam = User32.GetYLParam(lParam);
			this.ChangeWindowProc(this._originalWinProc);
			User32.PostMessage(hWnd, message, wParam, lParam);
			if (this._pressedAltF4)
			{
				this.Quit(1, string.Empty);
			}
			else if (xlparam != 0 || ylparam != 0)
			{
				this.Quit(21, string.Empty);
			}
			else
			{
				this.Quit(20, string.Empty);
			}
			return true;
		}

		private bool HandleCloseMessage(IntPtr hWnd, User32.WindowMessage message, IntPtr wParam, IntPtr lParam)
		{
			this.ChangeWindowProc(this._originalWinProc);
			User32.PostMessage(hWnd, message, wParam, lParam);
			this.Quit(19, string.Empty);
			return true;
		}

		private IntPtr ChangeWindowProc(IntPtr newProc)
		{
			return User32.SetWindowLongPtr(this._window, User32.WindowLongIndex.WindowProc, newProc);
		}

		private void LogQuitReason(GameQuitReason quitReason)
		{
			WindowsGameQuitHandler.Log.InfoFormat("Quit Reason: {0}", new object[]
			{
				this._gameQuitReasons[quitReason]
			});
			if (this._isServer || this._inEditor)
			{
				return;
			}
			this.SendQuitReasonToBi(quitReason);
			this.WriteQuitReasonToWindowsRegistry(quitReason);
		}

		private void SendQuitReasonToBi(GameQuitReason quitReason)
		{
			if (this._swordfishServices == null || this._swordfishServices.Log == null)
			{
				return;
			}
			bool isFirstLogin = false;
			bool joinedMatchmaking = false;
			if (this._swordfishServices.Connection != null)
			{
				isFirstLogin = this._swordfishServices.Connection.IsFirstLogin;
				joinedMatchmaking = this._swordfishServices.Connection.PlayerEverJoinedQueue;
			}
			string name = Enum.GetName(typeof(GameQuitReason), quitReason);
			string closeData = string.Format("{{\"condition\":\"{0}\"}}", name);
			this._swordfishServices.Log.BILogClientCloseCondition(closeData, isFirstLogin, joinedMatchmaking);
		}

		private void WriteQuitReasonToWindowsRegistry(GameQuitReason quitReason)
		{
			WindowsRegistry.WriteKeyValue("Software\\Hoplon Infotainment\\Heavy Metal Machines", "ExitCondition", quitReason);
		}

		private void ResetWinProc()
		{
			if (this._customWinProc == IntPtr.Zero)
			{
				return;
			}
			this.ChangeWindowProc(this._originalWinProc);
		}

		private void NotifySwordfishIfNeeded()
		{
			bool flag = this._isServer && this._swordfishServices != null && this._swordfishServices.Connection != null;
			if (flag)
			{
				this._swordfishServices.Connection.JobDone();
			}
		}

		public void Quit(GameQuitReason quitReason, string detail = "")
		{
			if (this._hasQuit)
			{
				return;
			}
			this.LogQuitReason(quitReason);
			this.NotifySwordfishIfNeeded();
			int num = 3;
			while (num-- > 0)
			{
				try
				{
					Application.Quit();
					this._hasQuit = true;
					return;
				}
				catch (Exception ex)
				{
					WindowsGameQuitHandler.Log.ErrorFormat("Exception trying to quit. Exception:{0}", new object[]
					{
						ex
					});
				}
			}
			if (!this._inEditor)
			{
				Process.GetCurrentProcess().Kill();
				return;
			}
		}

		public void Dispose()
		{
			this.ResetWinProc();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(WindowsGameQuitHandler));

		private readonly string[] _gameQuitReasons;

		private readonly SwordfishServices _swordfishServices;

		private readonly IntPtr _window;

		private readonly IntPtr _originalWinProc;

		private readonly IntPtr _customWinProc = IntPtr.Zero;

		private readonly WindowsGameQuitHandler.WindowProcDelegate _customWinProcDelegate;

		private readonly bool _inEditor;

		private readonly bool _isServer;

		private bool _hasQuit;

		private bool _pressedAltF4;

		private delegate IntPtr WindowProcDelegate(IntPtr hWnd, User32.WindowMessage message, IntPtr wParam, IntPtr lParam);
	}
}
