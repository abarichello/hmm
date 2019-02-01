using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body.Filter
{
	[Serializable]
	public class StatusFilter : ScriptableObject, ICombatFilter
	{
		public bool Match(ICombatObject first, ICombatObject other, Collider col)
		{
			return first.Attributes.CurrentStatus.HasFlag(this._status);
		}

		[SerializeField]
		private StatusKind _status;
	}
}
