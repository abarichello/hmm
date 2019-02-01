using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuTeamsButtonController : GameHubBehaviour
	{
		private MainMenuGui MainMenuGui
		{
			get
			{
				return (!(this._mainMenuGui == null)) ? this._mainMenuGui : (this._mainMenuGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>());
			}
		}

		protected void OnEnable()
		{
		}

		protected void OnDisable()
		{
		}

		public void ButtonOnClick()
		{
			OpenUrlUtils.OpenTeamsUrl(GameHubBehaviour.Hub, OpenUrlUtils.HardcodedWidth, (int)((float)Screen.height * 0.9f));
		}

		public GameObject WindowGameObject;

		public Animation ButtonAnimation;

		private MainMenuGui _mainMenuGui;
	}
}
