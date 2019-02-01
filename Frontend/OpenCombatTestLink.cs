using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class OpenCombatTestLink : OpenLink
	{
		private void Start()
		{
			this._url = string.Format(GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.CombatTestKnowMoreURL), Language.CurrentLanguage());
			this._logBi = true;
			this._biTag = ClientBITags.CombatTestKnowMore;
			this._collider = base.GetComponent<BoxCollider>();
			if (!(GameHubBehaviour.Hub.State.Current is MainMenu))
			{
				base.gameObject.SetActive(false);
				return;
			}
			MainMenuGui.OnLobbyUpdate += this.OnLobbyUpdate;
		}

		private void OnDestroy()
		{
			MainMenuGui.OnLobbyUpdate -= this.OnLobbyUpdate;
			this._collider = null;
		}

		private void OnLobbyUpdate(bool isInLobby)
		{
			this._collider.enabled = isInLobby;
		}

		private BoxCollider _collider;
	}
}
