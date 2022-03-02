using System;
using Hoplon.Input.UiNavigation;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public abstract class EscMenuScreenGui : GameHubBehaviour
	{
		public virtual void Show()
		{
			this.pivot.SetActive(true);
			this._uiNavigationSubGroupHolder.SubGroupFocusGet();
		}

		public virtual void Hide()
		{
			this.pivot.SetActive(false);
			this._uiNavigationSubGroupHolder.SubGroupFocusRelease();
		}

		public abstract void ReloadCurrent();

		public abstract void ResetDefault();

		[SerializeField]
		private GameObject pivot;

		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;
	}
}
