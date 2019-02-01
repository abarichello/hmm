using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Scene.Buff
{
	public class SceneryDisableForTeam : GameHubBehaviour
	{
		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				if (GameHubBehaviour.Hub.Players.CurrentPlayerTeam == TeamKind.Blue && this.BluEnabled)
				{
					return;
				}
				if (GameHubBehaviour.Hub.Players.CurrentPlayerTeam == TeamKind.Red && this.RedEnabled)
				{
					return;
				}
			}
			base.gameObject.SetActive(false);
		}

		public bool RedEnabled;

		public bool BluEnabled;
	}
}
