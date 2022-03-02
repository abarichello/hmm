using System;
using System.Collections.Generic;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Combat.Business
{
	public class CombatControllerStorage : ICombatControllerStorage
	{
		public CombatControllerStorage()
		{
			this._storage = new Dictionary<int, ICombatController>();
		}

		public ICombatController GetByObjId(int objId)
		{
			ICombatController result;
			if (this._storage.TryGetValue(objId, out result))
			{
				return result;
			}
			return null;
		}

		public void Set(int objId, ICombatController controller)
		{
			this._storage.Add(objId, controller);
		}

		private readonly IDictionary<int, ICombatController> _storage;
	}
}
