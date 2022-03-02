using System;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Combat.Business
{
	public interface ICombatStorage
	{
		ICombatObject GetByObjId(int objId);

		void Set(int objId, ICombatObject controller);
	}
}
