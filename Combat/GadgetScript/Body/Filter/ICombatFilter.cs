using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body.Filter
{
	public interface ICombatFilter
	{
		bool Match(ICombatObject first, ICombatObject other, Collider col);
	}
}
