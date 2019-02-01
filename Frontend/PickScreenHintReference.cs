using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PickScreenHintReference : GameHubBehaviour
	{
		public void Show()
		{
			this.isVisible = true;
			base.gameObject.SetActive(true);
			this._animation.Play();
			this._uiLabel.text = Language.Get(this.draft, TranslationSheets.PickMode);
		}

		public void Hide()
		{
			this.isVisible = false;
			base.gameObject.SetActive(false);
		}

		[SerializeField]
		private Animation _animation;

		[SerializeField]
		private UILabel _uiLabel;

		public string draft;

		public bool isVisible;
	}
}
