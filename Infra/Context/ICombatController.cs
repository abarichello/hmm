using System;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Context
{
	public interface ICombatController
	{
		void AddModifiers(ModifierData[] datas, ICombatObject causer, int eventId, bool barrierHit);

		void AddModifiers(ModifierData[] datas, ICombatObject causer, int eventId, Vector3 direction, Vector3 position, bool barrierHit);

		void AddPassiveModifiers(ModifierData[] datas, ICombatObject causer, int eventId);

		void RemovePassiveModifiers(ModifierData[] datas, ICombatObject causer, int eventId);
	}
}
