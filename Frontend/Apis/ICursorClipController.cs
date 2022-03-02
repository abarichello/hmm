using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend.Apis
{
	public interface ICursorClipController
	{
		bool IsCustomCursorClipSet { get; }

		void UpdateClipCursor(bool forceClipCursor = false);

		void EnableCursorClipToClientWindow();

		void DisableCursorClipToClientWindow();

		void SetCustomCursorClipArea(RectInt clipArea);

		void ClearCustomCursorClipArea();
	}
}
