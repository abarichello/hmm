using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class GlobalStates : GameHubObject
	{
		public bool LockAllPlayers
		{
			get
			{
				return this._lockAllPlayers;
			}
			set
			{
				if (GameHubObject.Hub.Match.LevelIsTutorial() || GameHubObject.Hub.Net.isTest)
				{
					this._lockAllPlayers = false;
					return;
				}
				if (GameHubObject.Hub.Net.IsServer())
				{
					this._lockAllPlayers = value;
				}
			}
		}

		private bool _lockAllPlayers;
	}
}
