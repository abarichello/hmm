using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkAggro : BasePerk
	{
		private float AggroRange
		{
			get
			{
				return this.Effect.Data.Range;
			}
		}

		private TeamKind Team
		{
			get
			{
				return this.Effect.Gadget.Combat.Team;
			}
		}

		public bool WantsToAttack
		{
			get
			{
				return this._wantsToAttack;
			}
		}

		public CombatObject AggroTarget
		{
			get
			{
				return this._aggroTarget;
			}
		}

		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._myTranform = base.transform;
		}

		private void FixedUpdate()
		{
			this.FixedUpdateServer();
		}

		protected virtual void FixedUpdateServer()
		{
			this.AggroCheck();
		}

		private void AggroCheck()
		{
			if (!this._forcedTarget || (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() > this._forcedAggroTime)
			{
				this._forcedTarget = false;
				this.SearchTarget();
				this.CheckTarget();
			}
			this._hasTarget = (this._aggroTarget && this._aggroTarget.IsAlive());
			if (this._hasTarget)
			{
				this._wantsToAttack = true;
				return;
			}
			this._wantsToAttack = false;
			this._forcedTarget = false;
		}

		private void CheckTarget()
		{
			if (!this._aggroTarget)
			{
				return;
			}
			if (!this._aggroTarget.IsAlive())
			{
				this._aggroTarget = null;
			}
		}

		private void SearchTarget()
		{
			if (this._searchUpdater.ShouldHalt())
			{
				return;
			}
			if (this.AIType == CreepAggroAIKind.Siege && this._aggroTarget && (this._aggroTarget.IsTurret || this._aggroTarget.IsBuilding))
			{
				return;
			}
			this._aggroTarget = null;
			Collider[] array = Physics.OverlapSphere(this._myTranform.position, this.AggroRange, 1077058560);
			SortedList<float, CombatObject> sortedList = new SortedList<float, CombatObject>();
			for (int i = 0; i < array.Length; i++)
			{
				CombatObject combat = CombatRef.GetCombat(array[i]);
				if (combat)
				{
					float sqrMagnitude = (combat.transform.position - this._myTranform.position).sqrMagnitude;
					sortedList[sqrMagnitude] = combat;
				}
			}
			foreach (KeyValuePair<float, CombatObject> keyValuePair in sortedList)
			{
				if (this.TryAttack(keyValuePair.Value))
				{
					break;
				}
			}
		}

		private bool TryAttack(CombatObject combat)
		{
			if (!combat || !combat.IsAlive())
			{
				return false;
			}
			if (combat.Team == this.Team)
			{
				return false;
			}
			this._aggroTarget = combat;
			return true;
		}

		public void ForceAggro(int target, float lifeTime)
		{
			Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(target);
			CombatObject combatObject = (!@object) ? null : @object.GetComponent<CombatObject>();
			if (!combatObject)
			{
				return;
			}
			this._aggroTarget = combatObject;
			this._forcedTarget = true;
			this._forcedAggroTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + (long)(lifeTime * 1000f);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkAggro));

		public CreepAggroAIKind AIType;

		private TimedUpdater _searchUpdater = new TimedUpdater
		{
			PeriodMillis = 250
		};

		private CombatObject _aggroTarget;

		private bool _hasTarget;

		private bool _forcedTarget;

		protected bool _wantsToAttack;

		private long _forcedAggroTime;

		private Transform _myTranform;
	}
}
