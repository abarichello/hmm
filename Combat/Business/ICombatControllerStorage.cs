using System;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Combat.Business
{
	public interface ICombatControllerStorage
	{
		ICombatController GetByObjId(int objId);

		void Set(int objId, ICombatController controller);
	}
}
