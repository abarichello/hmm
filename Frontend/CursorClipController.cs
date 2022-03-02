using System;
using HeavyMetalMachines.Frontend.Apis;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class CursorClipController : ICursorClipController
	{
		public CursorClipController()
		{
			this.Initialize();
		}

		public bool IsCustomCursorClipSet { get; private set; }

		public bool IsClientWindowClipSet
		{
			get
			{
				return this._isClientWindowClipEnabled;
			}
		}

		private void Initialize()
		{
			this._windowHandle = Platform.Current.GetCurrentWindowHandle("Heavy Metal Machines");
			if (this._windowHandle == IntPtr.Zero)
			{
				CursorClipController.Log.Warn("Could not find window handler for cursor clipping.");
			}
		}

		public void EnableCursorClipToClientWindow()
		{
			this.DoEnableCursorClipToClientWindow();
		}

		protected void DoEnableCursorClipToClientWindow()
		{
			this.SaveCurrentCursorRectToCache();
			this._isClientWindowClipEnabled = true;
			this.UpdateClipCursor(true);
			CursorClipController.Log.Info("Cursor clip to client window was enabled.");
		}

		public void DisableCursorClipToClientWindow()
		{
			this.DoDisableCursorClipToClientWindow();
		}

		protected void DoDisableCursorClipToClientWindow()
		{
			this.LoadCachedClipCursorRectFromCache();
			this._isClientWindowClipEnabled = false;
			CursorClipController.Log.Info("Cursor clip to client window was disabled.");
		}

		private void SaveCurrentCursorRectToCache()
		{
			if (!Platform.Current.GetClipCursor(out this._cachedCursorRect))
			{
				CursorClipController.Log.Warn("Failed to get current clip cursor to cache it.");
			}
			this._isCursorPosCached = true;
			CursorClipController.Log.Info("Cached cursor clip area was saved.");
		}

		private void LoadCachedClipCursorRectFromCache()
		{
			if (!this._isCursorPosCached)
			{
				return;
			}
			this.ClipCursor(ref this._cachedCursorRect);
			this._cachedCursorRect = default(RectInt);
			this._isCursorPosCached = false;
			CursorClipController.Log.Info("Cached cursor clip area was loaded.");
		}

		public void SetCustomCursorClipArea(RectInt clipArea)
		{
			this._customCursorClipArea = clipArea;
			this.IsCustomCursorClipSet = true;
			CursorClipController.Log.Info("Custom cursor lock area was set.");
			if (!this._isClientWindowClipEnabled)
			{
				this.SaveCurrentCursorRectToCache();
			}
			this.UpdateClipCursor(true);
		}

		public void ClearCustomCursorClipArea()
		{
			if (!this.IsCustomCursorClipSet)
			{
				return;
			}
			this._customCursorClipArea = default(RectInt);
			this.IsCustomCursorClipSet = false;
			if (!this._isClientWindowClipEnabled)
			{
				this.LoadCachedClipCursorRectFromCache();
			}
			else
			{
				this.UpdateClipCursor(true);
			}
			CursorClipController.Log.InfoFormat("Custom cursor lock was cleared.", new object[0]);
		}

		public void UpdateClipCursor(bool forceClipCursor = false)
		{
			if (this.IsWindowHandlerNotValid())
			{
				return;
			}
			if (!forceClipCursor && this.IsWaitingToClip())
			{
				return;
			}
			if (!this.IsClientWindowSelected())
			{
				this._clipCursorUpdater.PeriodMillis = 15;
				this._clipCursorUpdater.Reset();
				return;
			}
			this._clipCursorUpdater.PeriodMillis = 5000;
			this._clipCursorUpdater.Reset();
			if (this.IsCustomCursorClipSet)
			{
				this.ClipCursorToCustomArea();
			}
			else if (this._isClientWindowClipEnabled)
			{
				this.ClipCursorToClientWindow();
			}
		}

		private bool IsWindowHandlerNotValid()
		{
			return this._windowHandle == IntPtr.Zero;
		}

		protected virtual bool IsClientWindowSelected()
		{
			IntPtr foregroundWindow = Platform.Current.GetForegroundWindow();
			return foregroundWindow == this._windowHandle;
		}

		private bool IsWaitingToClip()
		{
			return this._clipCursorUpdater.ShouldHalt();
		}

		private void ClipCursorToCustomArea()
		{
			this.ClipCursor(ref this._customCursorClipArea);
		}

		private void ClipCursorToClientWindow()
		{
			RectInt rectInt;
			if (!Platform.Current.GetClientRectForClipping(this._windowHandle, out rectInt))
			{
				CursorClipController.Log.Warn("Could not get window reference. Won't clip cursor.");
				return;
			}
			this.ClipCursor(ref rectInt);
		}

		protected virtual void ClipCursor(ref RectInt rect)
		{
			Platform.Current.ClipCursor(ref rect);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CursorClipController));

		private const string ClientWindowText = "Heavy Metal Machines";

		private const int ClipCursorFrequencyInMillis = 5000;

		protected IntPtr _windowHandle;

		private bool _isClientWindowClipEnabled;

		private TimedUpdater _clipCursorUpdater = new TimedUpdater
		{
			PeriodMillis = 5000
		};

		protected bool _isCursorPosCached;

		protected RectInt _cachedCursorRect;

		private RectInt _customCursorClipArea;
	}
}
