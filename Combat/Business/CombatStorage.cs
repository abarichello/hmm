using System;
using System.Collections.Generic;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Combat.Business
{
	public class CombatStorage : ICombatStorage
	{
		public CombatStorage()
		{
			this._storage = new Dictionary<int, ICombatObject>();
		}

		public ICombatObject GetByObjId(int objId)
		{
			ICombatObject result;
			if (this._storage.TryGetValue(objId, out result))
			{
				return result;
			}
			return null;
		}

		public void Set(int objId, ICombatObject controller)
		{
			this._storage.Add(objId, controller);
		}

		private readonly IDictionary<int, ICombatObject> _storage;
	}
}
