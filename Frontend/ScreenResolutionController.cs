using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using HeavyMetalMachines.Frontend.Apis;
using HeavyMetalMachines.Platform;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ScreenResolutionController : GameHubBehaviour, IScreenResolutionController
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ScreenResolutionController.OnResolutionChange ListenToResolutionChange;

		public static int VerticalSync
		{
			get
			{
				return PlayerPrefs.GetInt("VSYNC", 0);
			}
		}

		public static int TargetFramesPerSecond
		{
			get
			{
				return PlayerPrefs.GetInt("TARGETFPS", -1);
			}
		}

		public static ScreenResolutionController.QualityLevels QualityLevel
		{
			get
			{
				return (ScreenResolutionController.QualityLevels)PlayerPrefs.GetInt("QUALITYLEVEL", 0);
			}
		}

		private void Awake()
		{
			UICamera.onScreenResize += this.ScreenSizeChanged;
			ScreenResolutionController._flashInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(ScreenResolutionController._flashInfo));
			ScreenResolutionController._flashInfo.uCount = uint.MaxValue;
			ScreenResolutionController._flashInfo.dwTimeout = 0u;
			this.ScreenSizeChanged();
			ScreenResolutionController.QualityLevels qualityLevels = (NativePlugins.GetDedicatedVideoMemorySize() <= 800) ? ScreenResolutionController.QualityLevels.LowTextures : ((SystemInfo.systemMemorySize <= 4096) ? ScreenResolutionController.QualityLevels.LowTextures : ScreenResolutionController.QualityLevels.HighTextures);
			ScreenResolutionController.SetQualityLevel(qualityLevels, true);
			ScreenResolutionController.Log.Info(string.Format("Selected quality level: {0}", (int)qualityLevels));
			QualitySettings.vSyncCount = ScreenResolutionController.VerticalSync;
			Application.targetFrameRate = ScreenResolutionController.TargetFramesPerSecond;
		}

		public List<ScreenResolutionController.Resolution> GetAvailableResolutions()
		{
			return this._availableResolutions;
		}

		public ScreenResolutionController.Resolution GetCurrentResolution()
		{
			return this._currentResolution;
		}

		public List<ScreenResolutionController.Display> GetAvailableDisplays()
		{
			return this._availableDisplays;
		}

		public int GetCurrentDisplay()
		{
			return PlayerPrefs.GetInt("UnitySelectMonitor");
		}

		public ScreenResolutionController.Display GetCurrentDisplayInfo()
		{
			int currentDisplay = this.GetCurrentDisplay();
			if (currentDisplay >= this._availableDisplays.Count)
			{
				ScreenResolutionController.Log.ErrorFormat("Display index doesn't is not available! Index: {0}; Available displays: {1}", new object[]
				{
					currentDisplay,
					this._availableDisplays.Count
				});
				return default(ScreenResolutionController.Display);
			}
			return this._availableDisplays[currentDisplay];
		}

		public bool IsFullscreen()
		{
			return Screen.fullScreen;
		}

		public int GetCurrentVsyncCount()
		{
			return QualitySettings.vSyncCount;
		}

		public int GetCurrentTargetFps()
		{
			return Application.targetFrameRate;
		}

		private void Start()
		{
			ScreenResolutionController._windowHandle = WindowsPlatform.GetCurrentWindowHandle("Heavy Metal Machines");
			if (ScreenResolutionController._windowHandle != IntPtr.Zero)
			{
				int num = WindowsPlatform.GetWindowTextLength(ScreenResolutionController._windowHandle);
				StringBuilder strText = new StringBuilder(++num);
				WindowsPlatform.GetWindowText(ScreenResolutionController._windowHandle, strText, num);
			}
			else
			{
				ScreenResolutionController.Log.ErrorFormat("Client window handle not found.", new object[0]);
			}
		}

		private void OnDestroy()
		{
			UICamera.onScreenResize -= this.ScreenSizeChanged;
		}

		private void ScreenSizeChanged()
		{
			this._lastFullscreen = Screen.fullScreen;
			this._currentResolution.Width = Screen.width;
			this._currentResolution.Height = Screen.height;
			if (this._currentResolution.AspectRatio() > 1.8f)
			{
				for (int k = this._standardResolutions.Count - 1; k > 0; k--)
				{
					if (this._standardResolutions[k].CompareTo(this._currentResolution) <= 0)
					{
						this._currentResolution = this._standardResolutions[k];
						ScreenResolutionController.SetWindowResolution(this._currentResolution);
						break;
					}
				}
			}
			this.DisplaysUpdated();
			int num = 0;
			int i;
			for (i = 0; i < this._availableDisplays.Count; i++)
			{
				num = Math.Max(this._standardResolutions.FindLastIndex((ScreenResolutionController.Resolution x) => x.CompareTo(this._availableDisplays[i].Res) <= 0), num);
			}
			this._availableResolutions.Clear();
			this._availableResolutions.AddRange(this._standardResolutions.GetRange(0, num + 1));
			for (int j = 0; j < this._availableDisplays.Count; j++)
			{
				if (this._availableDisplays[j].Res.AspectRatio() <= 1.8f)
				{
					if (!this._availableResolutions.Contains(this._availableDisplays[j].Res))
					{
						this._availableResolutions.Add(this._availableDisplays[j].Res);
					}
				}
			}
			if (!this._availableResolutions.Contains(this._currentResolution))
			{
				this._availableResolutions.Add(this._currentResolution);
			}
			this._availableResolutions.Sort();
			if (this.ListenToResolutionChange != null)
			{
				this.ListenToResolutionChange();
			}
		}

		private void DisplaysUpdated()
		{
			this._availableDisplays.Clear();
			if (!WindowsPlatform.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, new WindowsPlatform.MonitorEnumProc(this.MonitorEnumProc), IntPtr.Zero))
			{
				ScreenResolutionController.Log.Error("Failed to enumerate Monitors");
				WindowsPlatform.RECT rect;
				if (WindowsPlatform.GetWindowRect(WindowsPlatform.GetDesktopWindow(), out rect))
				{
					ScreenResolutionController.Resolution res;
					res.Width = rect.Right - rect.Left;
					res.Height = rect.Bottom - rect.Top;
					this._availableDisplays.Add(new ScreenResolutionController.Display
					{
						Index = 0,
						Rect = rect,
						Res = res
					});
				}
				else
				{
					ScreenResolutionController.Log.Error("Failed to get desktop rect");
				}
			}
		}

		private bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, ref WindowsPlatform.RECT lprcMonitor, IntPtr dwData)
		{
			ScreenResolutionController.Display item = new ScreenResolutionController.Display
			{
				Index = this._availableDisplays.Count,
				Rect = lprcMonitor,
				Res = new ScreenResolutionController.Resolution(lprcMonitor.Right - lprcMonitor.Left, lprcMonitor.Bottom - lprcMonitor.Top)
			};
			this._availableDisplays.Add(item);
			return true;
		}

		private void Update()
		{
			if (Screen.fullScreen != this._lastFullscreen)
			{
				this.ScreenSizeChanged();
				this._lastFullscreen = Screen.fullScreen;
			}
			if (Screen.fullScreen)
			{
				return;
			}
			if (Input.GetMouseButtonUp(0))
			{
				this._startMovingAround = false;
			}
			else if (!this._startMovingAround)
			{
				if (Input.mousePosition.y > (float)(Screen.height - 100) && Input.GetMouseButtonDown(0) && UICamera.hoveredObject != null && UICamera.hoveredObject.tag == "ScreenRepositiningArea")
				{
					if (Application.isEditor)
					{
						return;
					}
					this._startMovingAround = true;
					WindowsPlatform.GetCursorPos(out this._currentMousePos);
					this._lastMousetPos = this._currentMousePos;
				}
			}
			else
			{
				WindowsPlatform.GetCursorPos(out this._currentMousePos);
				if (this._startMovingAround && (this._lastMousetPos.X != this._currentMousePos.X || this._lastMousetPos.Y != this._currentMousePos.Y))
				{
					WindowsPlatform.RECT rect;
					if (!WindowsPlatform.GetWindowRect(ScreenResolutionController._windowHandle, out rect))
					{
						return;
					}
					int num = this._lastMousetPos.X - this._currentMousePos.X;
					int num2 = this._lastMousetPos.Y - this._currentMousePos.Y;
					this.SetPosition(rect.Left - num, rect.Top - num2, 0, 0);
				}
				this._lastMousetPos = this._currentMousePos;
			}
		}

		public void SetPosition(int x, int y, int resX = 0, int resY = 0)
		{
			WindowsPlatform.SetWindowPos(ScreenResolutionController._windowHandle, (IntPtr)0, x, y, resX, resY, (resX * resY != 0) ? 0 : 1);
		}

		public void Minimize()
		{
			WindowsPlatform.ShowWindow(ScreenResolutionController._windowHandle, WindowsPlatform.ShowWindowCommands.Minimize);
		}

		public void HighlightWindow(bool startTopmostWindow)
		{
			this.StartFlashWindow();
			this.ForceShowWindow();
			if (startTopmostWindow)
			{
				this.StartTopmostWindow();
			}
		}

		private void StartFlashWindow()
		{
			ScreenResolutionController._flashInfo.hwnd = ScreenResolutionController._windowHandle;
			ScreenResolutionController._flashInfo.dwFlags = 3u;
			WindowsPlatform.FlashWindowEx(ref ScreenResolutionController._flashInfo);
		}

		public void StopFlashWindow()
		{
			ScreenResolutionController._flashInfo.hwnd = ScreenResolutionController._windowHandle;
			ScreenResolutionController._flashInfo.dwFlags = 0u;
			WindowsPlatform.FlashWindowEx(ref ScreenResolutionController._flashInfo);
		}

		private void ForceShowWindow()
		{
			IntPtr foregroundWindow = WindowsPlatform.GetForegroundWindow();
			if (foregroundWindow == ScreenResolutionController._windowHandle)
			{
				return;
			}
			WindowsPlatform.ShowWindow(ScreenResolutionController._windowHandle, WindowsPlatform.ShowWindowCommands.Normal);
		}

		private void StartTopmostWindow()
		{
			WindowsPlatform.SetWindowPos(ScreenResolutionController._windowHandle, WindowsPlatform.HWND.TOPMOST, 0, 0, 0, 0, WindowsPlatform.SWP.NOSIZE | WindowsPlatform.SWP.NOMOVE | WindowsPlatform.SWP.SHOWWINDOW);
		}

		public void EndTopmostWindow()
		{
			WindowsPlatform.SetWindowPos(ScreenResolutionController._windowHandle, WindowsPlatform.HWND.NOTOPMOST, 0, 0, 0, 0, WindowsPlatform.SWP.NOSIZE | WindowsPlatform.SWP.NOMOVE | WindowsPlatform.SWP.SHOWWINDOW);
		}

		public static void SetWindowResolution(ScreenResolutionController.Resolution resolution)
		{
			Screen.SetResolution(resolution.Width, resolution.Height, Screen.fullScreen);
		}

		public void SetDisplay(int index)
		{
			PlayerPrefs.SetInt("UnitySelectMonitor", index);
			ScreenResolutionController.Display display = this._availableDisplays[index];
			if (this.IsFullscreen())
			{
				WindowsPlatform.MoveWindow(ScreenResolutionController._windowHandle, display.Rect.Left, display.Rect.Top, display.Res.Width, display.Res.Height, true);
			}
			else
			{
				WindowsPlatform.RECT rect;
				if (!WindowsPlatform.GetWindowRect(ScreenResolutionController._windowHandle, out rect))
				{
					ScreenResolutionController.Log.Error("Failed to get window rect, won't move window");
					return;
				}
				int num = rect.Right - rect.Left;
				int num2 = rect.Bottom - rect.Top;
				int x = (display.Res.Width - num >> 1) + display.Rect.Left;
				int y = (display.Res.Height - num2 >> 1) + display.Rect.Top;
				WindowsPlatform.MoveWindow(ScreenResolutionController._windowHandle, x, y, num, num2, false);
			}
		}

		public static void SetFullscreen(bool value)
		{
			Screen.fullScreen = value;
		}

		public static void SetVsync(int count)
		{
			PlayerPrefs.SetInt("VSYNC", count);
			QualitySettings.vSyncCount = count;
		}

		public static void SetTargetFps(int fps)
		{
			PlayerPrefs.SetInt("TARGETFPS", fps);
			Application.targetFrameRate = fps;
		}

		public static void SetQualityLevel(ScreenResolutionController.QualityLevels level, bool persist = true)
		{
			if (persist)
			{
				PlayerPrefs.SetInt("QUALITYLEVEL", (int)level);
			}
			QualitySettings.SetQualityLevel((int)level, false);
			QualitySettings.vSyncCount = ScreenResolutionController.VerticalSync;
		}

		public Vector2 GetClientWindowOffset()
		{
			WindowsPlatform.RECT rect;
			if (!WindowsPlatform.GetClientRect(ScreenResolutionController._windowHandle, out rect))
			{
				ScreenResolutionController.Log.Warn("Could not get window rect.");
				return Vector2.zero;
			}
			WindowsPlatform.POINT point;
			WindowsPlatform.ClientToScreen(ScreenResolutionController._windowHandle, out point);
			rect.Left = point.X;
			rect.Top = point.Y;
			rect.Right += point.X;
			rect.Bottom += point.Y;
			return new Vector2((float)point.X, (float)point.Y);
		}

		public Vector2 GetClientWindowDimensions()
		{
			float x;
			float y;
			if (Screen.fullScreen)
			{
				ScreenResolutionController.Display currentDisplayInfo = this.GetCurrentDisplayInfo();
				x = (float)currentDisplayInfo.Res.Width;
				y = (float)currentDisplayInfo.Res.Height;
			}
			else
			{
				x = (float)Screen.width;
				y = (float)Screen.height;
			}
			return new Vector2(x, y);
		}

		private const string ClientWindowText = "Heavy Metal Machines";

		private const string DisplayKey = "UnitySelectMonitor";

		private const string VsyncKey = "VSYNC";

		private const string FpsKey = "TARGETFPS";

		private const string QualityKey = "QUALITYLEVEL";

		private static readonly BitLogger Log = new BitLogger(typeof(ScreenResolutionController));

		private static IntPtr _windowHandle;

		private static WindowsPlatform.FLASHWINFO _flashInfo;

		public GameState Login;

		public GameState Game;

		private readonly List<ScreenResolutionController.Resolution> _standardResolutions = new List<ScreenResolutionController.Resolution>
		{
			new ScreenResolutionController.Resolution(800, 600),
			new ScreenResolutionController.Resolution(1024, 768),
			new ScreenResolutionController.Resolution(1280, 720),
			new ScreenResolutionController.Resolution(1280, 1024),
			new ScreenResolutionController.Resolution(1360, 768),
			new ScreenResolutionController.Resolution(1366, 768),
			new ScreenResolutionController.Resolution(1440, 900),
			new ScreenResolutionController.Resolution(1600, 900),
			new ScreenResolutionController.Resolution(1680, 1050),
			new ScreenResolutionController.Resolution(1920, 1080)
		};

		private readonly List<ScreenResolutionController.Resolution> _availableResolutions = new List<ScreenResolutionController.Resolution>();

		private readonly List<ScreenResolutionController.Display> _availableDisplays = new List<ScreenResolutionController.Display>();

		private ScreenResolutionController.Resolution _currentResolution;

		private WindowsPlatform.POINT _currentMousePos;

		private WindowsPlatform.POINT _lastMousetPos;

		private bool _startMovingAround;

		private bool _lastFullscreen;

		public struct Resolution : IComparable<ScreenResolutionController.Resolution>
		{
			public Resolution(int width, int height)
			{
				this.Width = width;
				this.Height = height;
			}

			public int CompareTo(ScreenResolutionController.Resolution other)
			{
				int num = this.Width - other.Width;
				return (num != 0) ? num : (this.Height - other.Height);
			}

			public float AspectRatio()
			{
				return (float)this.Width / (float)this.Height;
			}

			public override string ToString()
			{
				return string.Format("{0}x{1}", this.Width, this.Height);
			}

			public int Width;

			public int Height;
		}

		public struct Display
		{
			public override string ToString()
			{
				return this.Index.ToString();
			}

			public int Index;

			public WindowsPlatform.RECT Rect;

			public ScreenResolutionController.Resolution Res;
		}

		public enum QualityLevels
		{
			LowTextures,
			HighTextures,
			ModelViewer
		}

		public delegate void OnResolutionChange();
	}
}
