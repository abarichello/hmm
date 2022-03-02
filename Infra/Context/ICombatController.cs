using System;
using HeavyMetalMachines.Combat;

namespace HeavyMetalMachines.Infra.Context
{
	public interface ICombatController
	{
		void AddModifiers(ModifierData[] datas, ICombatObject causer, int eventId, bool barrierHit);

		void AddModifier(ModifierData datas, ICombatObject causer, int eventId, bool barrierHit);

		void AddPassiveModifier(ModifierData data, ICombatObject causer, int eventId);

		void AddPassiveModifiers(ModifierData[] datas, ICombatObject causer, int eventId);

		void RemovePassiveModifiers(ModifierData[] datas, ICombatObject causer, int eventId);
	}
}
