using System;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuLobbyGui : GameHubBehaviour
	{
		protected void OnEnable()
		{
			this._versionLabel.text = "Release.15.00.250";
		}

		private string GetDeployValue()
		{
			if (this._configLoader.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				return "SkipSwordfish";
			}
			return this._configLoader.GetValue(ConfigAccess.SFBaseUrl);
		}

		[SerializeField]
		private UILabel _versionLabel;

		[Inject]
		private IConfigLoader _configLoader;
	}
}
