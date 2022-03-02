using System;
using HeavyMetalMachines.Combat.Modifier;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block.Parameter.Filter
{
	public class CompareStatus : ScriptableObject, IParameterComparison
	{
		public bool Compare(object context)
		{
			ICombatObject value = this._combat.GetValue<ICombatObject>(context);
			bool flag = value.Attributes.CurrentStatus.HasFlag(this._status);
			if (this._status == StatusKind.NewStatusModifier)
			{
				flag &= value.Attributes.Status.Contains(this._statusModifier.ID);
			}
			return flag;
		}

		[Restrict(true, new Type[]
		{
			typeof(ICombatObject)
		})]
		[SerializeField]
		private BaseParameter _combat;

		[SerializeField]
		private StatusKind _status;

		[SerializeField]
		private StatusModifier _statusModifier;
	}
}
