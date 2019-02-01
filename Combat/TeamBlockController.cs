using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class TeamBlockController : GameHubBehaviour
	{
		private void Awake()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._cols = base.GetComponentsInChildren<Collider>();
		}

		public void Add(CombatObject obj)
		{
			if (this.Team != obj.Team)
			{
				return;
			}
			obj.ListenToEnable += this.OnCombatEnabled;
			this._objects.Add(obj);
			this.OnCombatEnabled(obj);
		}

		public void Remove(CombatObject obj)
		{
			obj.ListenToEnable -= this.OnCombatEnabled;
			this._objects.Remove(obj);
		}

		private void OnCombatEnabled(CombatObject obj)
		{
			Collider component = obj.GetComponent<Collider>();
			for (int i = 0; i < this._cols.Length; i++)
			{
				Physics.IgnoreCollision(this._cols[i], component, true);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(TeamBlockController));

		public TeamKind Team;

		private Collider[] _cols;

		private List<CombatObject> _objects = new List<CombatObject>();
	}
}
