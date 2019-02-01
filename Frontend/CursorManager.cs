using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Frontend.Apis;
using HeavyMetalMachines.Platform;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class CursorManager : GameHubBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> ChangeVisibilityCallback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<CursorManager.CursorTypes> CursorTypeChangedCallback;

		private void Awake()
		{
			this._cursorClipController = new CursorClipController();
			this._isClientWindowCursorClipEnabled = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.ClipCursor);
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.ListenToStateChanged;
			this._screenResolutionController = GameHubBehaviour.Hub.GuiScripts.ScreenResolution;
		}

		private void Update()
		{
			this._cursorClipController.UpdateClipCursor(false);
		}

		private void ListenToStateChanged(GameState changedstate)
		{
			if (!this._isClientWindowCursorClipEnabled)
			{
				return;
			}
			if (changedstate is Game)
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
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.ListenToStateChanged;
		}

		protected void Start()
		{
			this._cursorVisibility = Cursor.visible;
			this.SetCursor(CursorManager.CursorTypes.MainMenuCursor);
		}

		public void Push(bool visible, CursorManager.CursorTypes cursorType)
		{
			CursorManager.CursorState t = default(CursorManager.CursorState);
			t.IsVisible = this.IsShowingMouse();
			t.CursorType = this.GetCursorType();
			this._cursorStatesStack.Push(t);
			this.ShowAndSetCursor(visible, cursorType);
		}

		public void Pop()
		{
			CursorManager.CursorState cursorState = this._cursorStatesStack.Pop();
			this.ShowAndSetCursor(cursorState.IsVisible, cursorState.CursorType);
		}

		private void SetCursor(CursorManager.CursorTypes cursorType)
		{
			switch (cursorType)
			{
			case CursorManager.CursorTypes.MainMenuCursor:
			case CursorManager.CursorTypes.MatchstatsCursor:
			case CursorManager.CursorTypes.OptionsCursor:
				Cursor.SetCursor(this.mainMenuCursor, Vector2.zero, CursorMode.Auto);
				break;
			case CursorManager.CursorTypes.GameCursor:
				Cursor.SetCursor(this.gameplayCursor, new Vector2((float)this.gameplayCursor.width * 0.5f, (float)this.gameplayCursor.height * 0.5f), CursorMode.Auto);
				break;
			case CursorManager.CursorTypes.InGameShopCursor:
				Cursor.SetCursor(this.emptyCursor, Vector2.zero, CursorMode.Auto);
				break;
			}
			this._cursorType = cursorType;
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
			Cursor.visible = visible;
		}

		public void LockCursor(bool _lock)
		{
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
				Cursor.visible = this._cursorVisibility;
			}
			else
			{
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			}
		}

		private IEnumerator PostponedShowCursorVisible()
		{
			yield return null;
			Cursor.visible = true;
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
			Vector2 vector = new Vector2(horOffset, verOffset);
			int num3 = (int)(vector.x * num + num);
			int num4 = (int)(vector.y * num2 + num2);
			num3 += (int)clientWindowOffset.x;
			num4 += (int)clientWindowOffset.y;
			WindowsPlatform.RECT customCursorClipArea;
			customCursorClipArea.Left = num3;
			customCursorClipArea.Top = num4;
			customCursorClipArea.Right = num3 + 1;
			customCursorClipArea.Bottom = num4 + 1;
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
