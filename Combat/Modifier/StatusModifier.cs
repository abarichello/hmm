using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Modifier
{
	[CreateAssetMenu(menuName = "Modifier/StatusModifier")]
	public sealed class StatusModifier : BaseModifier
	{
		public override void Apply(ICombatObject causer, ICombatObject target, IHMMContext hmm)
		{
			target.Attributes.Status.Add(base.ID);
		}
	}
}
