using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend.ArenaSelector
{
	[Serializable]
	public struct ArenaCardInfo
	{
		public bool isTargetArena;

		public string NameText;

		public Sprite ImageSprite;

		public bool Locked;

		public int ArenaIndex;

		public string UnlockLevelText;
	}
}
