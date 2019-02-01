using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class CombatRef : GameHubBehaviour
	{
		protected virtual void OnEnable()
		{
			this._col = base.GetComponent<Collider>();
			if (this._col != null)
			{
				CombatRef.Colliders[this._col] = this;
			}
		}

		protected virtual void OnDisable()
		{
			if (this._col != null)
			{
				CombatRef.Colliders.Remove(this._col);
			}
		}

		public static CombatObject GetCombat(int id)
		{
			Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(id);
			return CombatRef.GetCombat(@object);
		}

		public static CombatObject GetCombat(Component comp)
		{
			if (comp == null)
			{
				return null;
			}
			Collider collider = comp as Collider;
			if (collider != null)
			{
				CombatRef combatRef = null;
				return (!CombatRef.Colliders.TryGetValue(collider, out combatRef)) ? null : combatRef.Combat;
			}
			CombatRef component = comp.GetComponent<CombatRef>();
			if (!component)
			{
				return null;
			}
			CombatObject combatObject = component as CombatObject;
			if (combatObject != null)
			{
				return combatObject;
			}
			return component.Combat;
		}

		private static readonly Dictionary<Collider, CombatRef> Colliders = new Dictionary<Collider, CombatRef>();

		public CombatObject Combat;

		private Collider _col;
	}
}
