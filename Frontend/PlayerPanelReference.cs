using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PlayerPanelReference : GameHubBehaviour
	{
		private void Awake()
		{
			this._showAnimator = base.GetComponent<Animator>();
		}

		public void DisableGameObject()
		{
			base.gameObject.SetActive(false);
		}

		public void Show()
		{
			this._showAnimator.SetBool("active", true);
		}

		public void Hide()
		{
			this._showAnimator.SetBool("active", false);
		}

		public UILabel PlayerNameLabel;

		public NGUIWidgetAlpha WidgetForAlpha;

		private Animator _showAnimator;
	}
}
