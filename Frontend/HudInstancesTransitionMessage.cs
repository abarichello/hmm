using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudInstancesTransitionMessage : GameHubBehaviour
	{
		public void Hide()
		{
			if (this.IsPlaying())
			{
				this.TransitionAnimation.Stop();
			}
			base.gameObject.SetActive(false);
			if (!HudInstancesController.IsInShopState())
			{
				this._didAnimation = false;
			}
		}

		public void Show()
		{
			base.gameObject.SetActive(true);
			if (!this._didAnimation)
			{
				this._didAnimation = true;
				this.TransitionAnimation.Play();
			}
		}

		public bool IsPlaying()
		{
			return this.TransitionAnimation.isPlaying;
		}

		public Animation TransitionAnimation;

		private bool _didAnimation;
	}
}
