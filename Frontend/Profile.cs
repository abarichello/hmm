using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class Profile : GameState
	{
		public void GoToLogin()
		{
			GameHubBehaviour.Hub.State.GotoState(this.Splash, false);
		}

		[SerializeField]
		private Splash Splash;
	}
}
