using System;
using HeavyMetalMachines.Platform;

namespace HeavyMetalMachines.Frontend.Apis
{
	public interface ICursorClipController
	{
		bool IsCustomCursorClipSet { get; }

		void UpdateClipCursor(bool forceClipCursor = false);

		void EnableCursorClipToClientWindow();

		void DisableCursorClipToClientWindow();

		void SetCustomCursorClipArea(WindowsPlatform.RECT clipArea);

		void ClearCustomCursorClipArea();
	}
}
