using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.GameStates.Game.HudGadgetCursor
{
	public class HudCursorController : GameHubBehaviour
	{
		public void Update()
		{
			this._mainRectTransform.localPosition = Input.mousePosition;
		}

		[SerializeField]
		private RectTransform _mainRectTransform;
	}
}
