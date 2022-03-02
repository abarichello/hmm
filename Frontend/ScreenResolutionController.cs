using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Frontend.Apis;
using NativePlugins;
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
				int @int = PlayerPrefs.GetInt("TARGETFPS", -1);
				ScreenResolutionController.Log.Info(string.Format("TargetFramesPerSecond: {0}", @int));
				return @int;
			}
		}

		public static ScreenResolutionController.QualityLevels QualityLevel
		{
			get
			{
				int @int = PlayerPrefs.GetInt("QUALITYLEVEL", 0);
				ScreenResolutionController.Log.Info(string.Format("QualityLevel: {0}", @int));
				return (ScreenResolutionController.QualityLevels)@int;
			}
		}

		private void Awake()
		{
			UICamera.onScreenResize += this.ScreenSizeChanged;
			this.ScreenSizeChanged();
			ScreenResolutionController.SelectedQuality = ((UnityInterface.GetDedicatedVideoMemorySize() <= 800) ? ScreenResolutionController.QualityLevels.LowTextures : ((SystemInfo.systemMemorySize <= 4096) ? ScreenResolutionController.QualityLevels.LowTextures : ScreenResolutionController.QualityLevels.HighTextures));
			ScreenResolutionController.Log.Info(string.Format("Selected quality level: {0}", (int)ScreenResolutionController.SelectedQuality));
			ScreenResolutionController.SetQualityLevel(ScreenResolutionController.QualityLevels.PreviewItem, false);
			Application.targetFrameRate = ScreenResolutionController.TargetFramesPerSecond;
		}

		public List<Resolution> GetAvailableResolutions()
		{
			return this._availableResolutions;
		}

		public Resolution GetCurrentResolution()
		{
			return this._currentResolution;
		}

		public List<Display> GetAvailableDisplays()
		{
			return this._availableDisplays;
		}

		public int GetCurrentDisplay()
		{
			return PlayerPrefs.GetInt("UnitySelectMonitor");
		}

		public Display GetCurrentDisplayInfo()
		{
			int currentDisplay = this.GetCurrentDisplay();
			if (currentDisplay >= this._availableDisplays.Count)
			{
				ScreenResolutionController.Log.ErrorFormat("Display index not available! Index: {0}; Available displays: {1}", new object[]
				{
					currentDisplay,
					this._availableDisplays.Count
				});
				return default(Display);
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
			ScreenResolutionController._windowHandle = Platform.Current.GetCurrentWindowHandle("Heavy Metal Machines");
			if (ScreenResolutionController._windowHandle != IntPtr.Zero)
			{
				ScreenResolutionController.Log.DebugFormat("Client window detected. Text:[{0}] Handle:[{1}]", new object[]
				{
					Platform.Current.GetWindowText(ScreenResolutionController._windowHandle),
					ScreenResolutionController._windowHandle
				});
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
			ScreenResolutionController.Log.DebugFormat("ScreenSizeChanged: {0} Fullscreen: {1}", new object[]
			{
				this._currentResolution,
				Screen.fullScreen
			});
			this.DisplaysUpdated();
			int num = 0;
			int i;
			for (i = 0; i < this._availableDisplays.Count; i++)
			{
				num = Math.Max(this._standardResolutions.FindLastIndex((Resolution x) => x.CompareTo(this._availableDisplays[i].Res) <= 0), num);
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
			if (!Platform.Current.GetDisplays(ref this._availableDisplays))
			{
				ScreenResolutionController.Log.Error("Failed to enumerate Monitors");
				RectInt rect;
				if (Platform.Current.GetDesktopRect(out rect))
				{
					Resolution res;
					res.Width = rect.width;
					res.Height = rect.height;
					this._availableDisplays.Add(new Display
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
				if (Input.mousePosition.y > (float)(Screen.height - 100) && Input.GetMouseButtonDown(0))
				{
					GameObject hoveredObject = UICamera.hoveredObject;
					if (hoveredObject != null && hoveredObject.CompareTag("ScreenRepositiningArea"))
					{
						if (Application.isEditor)
						{
							return;
						}
						this._startMovingAround = true;
						Platform.Current.GetCursorPos(out this._currentMousePos);
						this._lastMousetPos = this._currentMousePos;
					}
				}
			}
			else
			{
				Platform.Current.GetCursorPos(out this._currentMousePos);
				if (this._startMovingAround && (this._lastMousetPos.x != this._currentMousePos.x || this._lastMousetPos.y != this._currentMousePos.y))
				{
					RectInt rectInt;
					if (!Platform.Current.GetWindowRect(ScreenResolutionController._windowHandle, out rectInt))
					{
						return;
					}
					int num = this._lastMousetPos.x - this._currentMousePos.x;
					int num2 = this._lastMousetPos.y - this._currentMousePos.y;
					this.SetPosition(rectInt.xMin - num, rectInt.yMin - num2, 0, 0);
				}
				this._lastMousetPos = this._currentMousePos;
			}
		}

		public void SetPosition(int x, int y, int resX = 0, int resY = 0)
		{
			Platform.Current.SetWindowPosition(ScreenResolutionController._windowHandle, x, y, resX, resY);
		}

		public void Minimize()
		{
			Platform.Current.ShowWindow(ScreenResolutionController._windowHandle, true);
		}

		public void HighlightWindow(bool startTopmostWindow)
		{
			ScreenResolutionController.StartFlashWindow();
			ScreenResolutionController.ForceShowWindow();
			if (startTopmostWindow)
			{
				ScreenResolutionController.StartTopmostWindow();
			}
		}

		private static void StartFlashWindow()
		{
			Platform.Current.SetFlashWindow(ScreenResolutionController._windowHandle, true);
		}

		public static void StopFlashWindow()
		{
			Platform.Current.SetFlashWindow(ScreenResolutionController._windowHandle, false);
		}

		private static void ForceShowWindow()
		{
			IntPtr foregroundWindow = Platform.Current.GetForegroundWindow();
			if (foregroundWindow == ScreenResolutionController._windowHandle)
			{
				return;
			}
			ScreenResolutionController.Log.Debug("Forcing game window to show");
			Platform.Current.ShowWindow(ScreenResolutionController._windowHandle, false);
		}

		private static void StartTopmostWindow()
		{
			Platform.Current.SetTopmostWindow(ScreenResolutionController._windowHandle, true);
		}

		public static void EndTopmostWindow()
		{
			if (!Application.isFocused)
			{
				return;
			}
			Platform.Current.SetTopmostWindow(ScreenResolutionController._windowHandle, false);
		}

		public static void SetWindowResolution(Resolution resolution)
		{
			if (Platform.Current.IsConsole())
			{
				return;
			}
			ScreenResolutionController.Log.DebugFormat("SetWindowResolution: {0} Fullscreen: {1}", new object[]
			{
				resolution,
				Screen.fullScreen
			});
			Screen.SetResolution(resolution.Width, resolution.Height, Screen.fullScreen);
		}

		public void SetDisplay(int index)
		{
			ScreenResolutionController.Log.DebugFormat("SetDisplay: {0}", new object[]
			{
				index
			});
			PlayerPrefs.SetInt("UnitySelectMonitor", index);
			Display display = this._availableDisplays[index];
			if (this.IsFullscreen())
			{
				Platform.Current.MoveWindow(ScreenResolutionController._windowHandle, display.Rect.xMin, display.Rect.yMin, display.Res.Width, display.Res.Height, true);
			}
			else
			{
				RectInt rectInt;
				if (!Platform.Current.GetWindowRect(ScreenResolutionController._windowHandle, out rectInt))
				{
					ScreenResolutionController.Log.Error("Failed to get window rect, won't move window");
					return;
				}
				int x = (display.Res.Width - rectInt.width >> 1) + display.Rect.xMin;
				int y = (display.Res.Height - rectInt.height >> 1) + display.Rect.yMin;
				Platform.Current.MoveWindow(ScreenResolutionController._windowHandle, x, y, rectInt.width, rectInt.height, false);
			}
		}

		public static void SetFullscreen(bool value)
		{
			ScreenResolutionController.Log.DebugFormat("SetFullscreen: {0}", new object[]
			{
				value
			});
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
			if (Platform.Current.IsConsole())
			{
				ScreenResolutionController.Log.WarnFormat("TODO/FIX - SetQualityLevel disabled on consoles. Current running level: {0}", new object[]
				{
					QualitySettings.GetQualityLevel()
				});
				return;
			}
			if (persist)
			{
				PlayerPrefs.SetInt("QUALITYLEVEL", (int)level);
			}
			QualitySettings.SetQualityLevel((int)level, false);
			QualitySettings.vSyncCount = ScreenResolutionController.VerticalSync;
			ScreenResolutionController.Log.InfoFormat("Quality setting set. Quality={0} persist={1}", new object[]
			{
				level,
				persist
			});
		}

		public static void SetInGameQualityLevel()
		{
			ScreenResolutionController.SetQualityLevel(ScreenResolutionController.SelectedQuality, true);
		}

		public Vector2 GetClientWindowOffset()
		{
			Vector2Int vector2Int;
			if (Platform.Current.GetClientRectOffset(ScreenResolutionController._windowHandle, out vector2Int))
			{
				return new Vector2((float)vector2Int.x, (float)vector2Int.y);
			}
			ScreenResolutionController.Log.Warn("Could not get client window offset rect.");
			return Vector2.zero;
		}

		public Vector2 GetClientWindowDimensions()
		{
			float num;
			float num2;
			if (Screen.fullScreen)
			{
				Display currentDisplayInfo = this.GetCurrentDisplayInfo();
				num = (float)currentDisplayInfo.Res.Width;
				num2 = (float)currentDisplayInfo.Res.Height;
			}
			else
			{
				num = (float)Screen.width;
				num2 = (float)Screen.height;
			}
			return new Vector2(num, num2);
		}

		private const string ClientWindowText = "Heavy Metal Machines";

		private const string DisplayKey = "UnitySelectMonitor";

		private const string VsyncKey = "VSYNC";

		private const string FpsKey = "TARGETFPS";

		private const string QualityKey = "QUALITYLEVEL";

		private static readonly BitLogger Log = new BitLogger(typeof(ScreenResolutionController));

		private static IntPtr _windowHandle;

		public GameState Login;

		public GameState Game;

		private readonly List<Resolution> _standardResolutions = new List<Resolution>
		{
			new Resolution(1280, 720),
			new Resolution(1280, 1024),
			new Resolution(1360, 768),
			new Resolution(1366, 768),
			new Resolution(1440, 900),
			new Resolution(1600, 900),
			new Resolution(1680, 1050),
			new Resolution(1920, 1080)
		};

		private readonly List<Resolution> _availableResolutions = new List<Resolution>();

		private List<Display> _availableDisplays = new List<Display>();

		private Resolution _currentResolution;

		private Vector2Int _currentMousePos;

		private Vector2Int _lastMousetPos;

		private bool _startMovingAround;

		private bool _lastFullscreen;

		private static ScreenResolutionController.QualityLevels SelectedQuality = ScreenResolutionController.QualityLevels.LowTextures;

		public enum QualityLevels
		{
			LowTextures,
			HighTextures,
			PreviewItem,
			PS4
		}

		public delegate void OnResolutionChange();
	}
}
