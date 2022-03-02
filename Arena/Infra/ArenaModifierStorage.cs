using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;

namespace HeavyMetalMachines.Arena.Infra
{
	public class ArenaModifierStorage : IArenaModifierStorage
	{
		public ArenaModifierStorage()
		{
			this._storage = new Dictionary<ArenaModifierCondition, ModifierData[]>();
			this._emptyModifierData = new ModifierData[0];
		}

		public ModifierData[] GetByArenaCondition(ArenaModifierCondition condition)
		{
			ModifierData[] result;
			if (this._storage.TryGetValue(condition, out result))
			{
				return result;
			}
			return this._emptyModifierData;
		}

		public void Set(ArenaModifierCondition condition, ModifierData[] datas)
		{
			this._storage.Add(condition, datas);
		}

		private readonly IDictionary<ArenaModifierCondition, ModifierData[]> _storage;

		private readonly ModifierData[] _emptyModifierData;
	}
}
