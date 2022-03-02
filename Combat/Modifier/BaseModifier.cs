using System;
using System.Collections.Generic;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Modifier
{
	public abstract class BaseModifier : ScriptableObject
	{
		public int ID
		{
			get
			{
				return this._id;
			}
		}

		public abstract void Apply(ICombatObject causer, ICombatObject target, IHMMContext hmm);

		protected virtual void OnEnable()
		{
			BaseModifier._modifiers[this._id] = this;
		}

		protected virtual void OnValidate()
		{
			BaseModifier baseModifier;
			while (BaseModifier._modifiers.TryGetValue(this._id, out baseModifier) && baseModifier != this)
			{
				this._id++;
			}
		}

		private static IDictionary<int, BaseModifier> _modifiers = new Dictionary<int, BaseModifier>();

		[SerializeField]
		[ReadOnly]
		private int _id = BaseModifier._modifiers.Count;
	}
}
