using System;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Players.Business;

namespace HeavyMetalMachines.Boosters.Business
{
	public class GetLocalPlayerFounderBooster : IGetLocalPlayerFounterBooster
	{
		public GetLocalPlayerFounderBooster(ILocalPlayerStorage localPlayer)
		{
			this._localPlayer = localPlayer;
		}

		public bool HasFounderBooster()
		{
			FounderLevel founderLevel = this.GetFounderLevel();
			return FounderLevelEx.CheckHasFlag(founderLevel, 4) || FounderLevelEx.CheckHasFlag(founderLevel, 2) || FounderLevelEx.CheckHasFlag(founderLevel, 1);
		}

		public FounderLevel GetFounderLevel()
		{
			Player player = this._localPlayer.Player as Player;
			if (player == null)
			{
				return 0;
			}
			PlayerBag bag = player.Bag;
			return bag.FounderPackLevel;
		}

		private readonly ILocalPlayerStorage _localPlayer;
	}
}
