using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Frontend.Apis;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using Hoplon.Input;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class CursorManager : GameHubBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> ChangeVisibilityCallback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<CursorManager.CursorTypes> CursorTypeChangedCallback;

		public static bool IsCursorVisible
		{
			get
			{
				return Cursor.visible;
			}
		}

		private void Awake()
		{
			this._cursorClipController = new CursorClipController();
			this._isClientWindowCursorClipEnabled = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.ClipCursor);
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.ListenToStateChanged;
			this._screenResolutionController = GameHubBehaviour.Hub.GuiScripts.ScreenResolution;
		}

		protected void Start()
		{
			this._activeDeviceChangeDisposable = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(Observable.Do<InputDevice>(this._activeDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), delegate(InputDevice device)
			{
				this._lastActiveDevice = device;
			}), new Action<InputDevice>(this.UpdateCursorWhenJoystickIsTheCurrentDevice)));
			this._cursorVisibility = Cursor.visible;
			this.SetCursor(CursorManager.CursorTypes.MainMenuCursor);
		}

		private void UpdateCursorWhenJoystickIsTheCurrentDevice(InputDevice device)
		{
			if (device == 3)
			{
				this.ShowCursor(false);
				UICamera.Notify(UICamera.hoveredObject, "OnHover", false);
				Cursor.lockState = 1;
				return;
			}
			this.ShowCursor(true);
			Cursor.lockState = 0;
		}

		private void Update()
		{
			this._cursorClipController.UpdateClipCursor(false);
		}

		private void ListenToStateChanged(GameState changedState)
		{
			if (!this._isClientWindowCursorClipEnabled)
			{
				return;
			}
			if (changedState is Game)
			{
				this._cursorClipController.EnableCursorClipToClientWindow();
			}
			else
			{
				this._cursorClipController.DisableCursorClipToClientWindow();
			}
		}

		private void OnDestroy()
		{
			this.Dispose();
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.ListenToStateChanged;
		}

		private void Dispose()
		{
			if (this._activeDeviceChangeDisposable != null)
			{
				this._activeDeviceChangeDisposable.Dispose();
				this._activeDeviceChangeDisposable = null;
			}
		}

		public void Push(bool visible, CursorManager.CursorTypes cursorType)
		{
			CursorManager.CursorState item = default(CursorManager.CursorState);
			item.IsVisible = ((this._lastActiveDevice != 3) ? this.IsShowingMouse() : this._cursorVisibility);
			item.CursorType = this.GetCursorType();
			this._cursorStatesStack.Push(item);
			this.ShowAndSetCursor(visible, cursorType);
		}

		public void Pop()
		{
			CursorManager.CursorState cursorState = this._cursorStatesStack.Pop();
			this.ShowAndSetCursor(cursorState.IsVisible, cursorState.CursorType);
		}

		private void SetCursor(CursorManager.CursorTypes cursorType)
		{
			this._cursorType = cursorType;
			if (!Application.isFocused)
			{
				return;
			}
			switch (cursorType)
			{
			case CursorManager.CursorTypes.MainMenuCursor:
			case CursorManager.CursorTypes.MatchstatsCursor:
			case CursorManager.CursorTypes.OptionsCursor:
				Cursor.SetCursor(this.mainMenuCursor, Vector2.zero, 0);
				break;
			case CursorManager.CursorTypes.GameCursor:
				Cursor.SetCursor(this.gameplayCursor, new Vector2((float)this.gameplayCursor.width * 0.5f, (float)this.gameplayCursor.height * 0.5f), 0);
				break;
			case CursorManager.CursorTypes.InGameShopCursor:
				Cursor.SetCursor(this.emptyCursor, Vector2.zero, 0);
				break;
			}
			this._setCursorLastFrame = Time.frameCount;
		}

		public CursorManager.CursorTypes GetCursorType()
		{
			return this._cursorType;
		}

		private void ShowCursor(bool visible)
		{
			bool cursorVisibility = this._cursorVisibility;
			this._cursorVisibility = visible;
			if (cursorVisibility != visible && this.ChangeVisibilityCallback != null)
			{
				this._cursorVisibility = visible;
				this.ChangeVisibilityCallback(visible);
			}
			if (this._postponedShowCursorVisibleCoroutine != null)
			{
				base.StopCoroutine(this._postponedShowCursorVisibleCoroutine);
				this._postponedShowCursorVisibleCoroutine = null;
			}
			if (visible && this._setCursorLastFrame == Time.frameCount)
			{
				this._setCursorLastFrame = -1;
				this._postponedShowCursorVisibleCoroutine = base.StartCoroutine(this.PostponedShowCursorVisible());
				return;
			}
			this.SetUnityCursorVisible(visible);
		}

		private void SetUnityCursorVisible(bool visible)
		{
			Cursor.visible = (visible && this._lastActiveDevice != 3);
		}

		public bool IsShowingMouse()
		{
			return Cursor.visible;
		}

		protected void OnApplicationFocus(bool hasFocus)
		{
			if (hasFocus)
			{
				this.SetCursor(this._cursorType);
				this.SetUnityCursorVisible(this._cursorVisibility);
			}
			else
			{
				Cursor.SetCursor(null, Vector2.zero, 0);
			}
		}

		private IEnumerator PostponedShowCursorVisible()
		{
			yield return null;
			this.SetUnityCursorVisible(true);
			this._postponedShowCursorVisibleCoroutine = null;
			yield break;
		}

		public void ShowAndSetCursor(bool visible, CursorManager.CursorTypes cursorType)
		{
			bool flag = this._cursorType != cursorType;
			if (!visible)
			{
				this.ShowCursor(false);
				this.SetCursor(cursorType);
			}
			else
			{
				this.SetCursor(cursorType);
				this.ShowCursor(true);
			}
			if (flag && this.CursorTypeChangedCallback != null)
			{
				this.CursorTypeChangedCallback(cursorType);
			}
		}

		public bool IsCursorLockedByOffset()
		{
			return this._cursorClipController.IsCustomCursorClipSet;
		}

		public void LockCursorByOffset(float horOffset, float verOffset)
		{
			Vector2 clientWindowDimensions = this._screenResolutionController.GetClientWindowDimensions();
			float num = clientWindowDimensions.x * 0.5f;
			float num2 = clientWindowDimensions.y * 0.5f;
			Vector2 clientWindowOffset = this._screenResolutionController.GetClientWindowOffset();
			CursorManager.Log.DebugFormat("Client window resolution is: {0},{1}; window offset is: {2},{3}", new object[]
			{
				clientWindowDimensions.x,
				clientWindowDimensions.y,
				clientWindowOffset.x,
				clientWindowOffset.y
			});
			Vector2 vector;
			vector..ctor(horOffset, verOffset);
			int num3 = (int)(vector.x * num + num);
			int num4 = (int)(vector.y * num2 + num2);
			num3 += (int)clientWindowOffset.x;
			num4 += (int)clientWindowOffset.y;
			CursorManager.Log.DebugFormat("Mouse offset is: {0},{1}; target lock pos is: {2},{3}", new object[]
			{
				vector.x,
				vector.y,
				num3,
				num4
			});
			RectInt customCursorClipArea = default(RectInt);
			customCursorClipArea.xMin = num3;
			customCursorClipArea.yMin = num4;
			customCursorClipArea.xMax = num3 + 1;
			customCursorClipArea.yMax = num4 + 1;
			this._cursorClipController.SetCustomCursorClipArea(customCursorClipArea);
		}

		public void UnlockCursorByOffset()
		{
			this._cursorClipController.ClearCustomCursorClipArea();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CursorManager));

		public Camera MainCamera;

		public Texture2D mainMenuCursor;

		public Texture2D gameplayCursor;

		public Texture2D emptyCursor;

		private CursorManager.CursorTypes _cursorType;

		private bool _cursorVisibility;

		private int _setCursorLastFrame;

		private Coroutine _postponedShowCursorVisibleCoroutine;

		private Stack<CursorManager.CursorState> _cursorStatesStack = new Stack<CursorManager.CursorState>(5);

		private ICursorClipController _cursorClipController;

		private bool _isClientWindowCursorClipEnabled;

		private IScreenResolutionController _screenResolutionController;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _activeDeviceChangeNotifier;

		[InjectOnClient]
		private IScoreBoard _gameModeStateProvider;

		private InputDevice _lastActiveDevice;

		private IDisposable _activeDeviceChangeDisposable;

		public enum CursorTypes
		{
			MainMenuCursor,
			GameCursor,
			InGameShopCursor,
			MatchstatsCursor,
			OptionsCursor
		}

		private struct CursorState
		{
			public bool IsVisible;

			public CursorManager.CursorTypes CursorType;
		}
	}
}
