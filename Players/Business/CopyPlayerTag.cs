using System;
using HeavyMetalMachines.Core.Extensions;

namespace HeavyMetalMachines.Players.Business
{
	public class CopyPlayerTag : ICopyPlayerTag
	{
		public void CopyToClipboard(IPlayer player)
		{
			player.PlayerTag.ToString().CopyToClipboard();
		}
	}
}
