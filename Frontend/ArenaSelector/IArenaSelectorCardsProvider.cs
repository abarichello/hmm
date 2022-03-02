using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend.ArenaSelector
{
	public interface IArenaSelectorCardsProvider
	{
		ArenaCardInfo[] Get(Sprite[] arenaSprites);
	}
}
