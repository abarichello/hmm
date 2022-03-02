using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class CombatLayer : GameHubBehaviour, ICachedObject, ISerializationCallbackReceiver
	{
		private void Awake()
		{
			this._transform = base.transform;
			this._updater = new TimedUpdater(50, true, false);
		}

		private void Start()
		{
			this.ignoreLayer = LayerMask.NameToLayer("PropCollider");
			this._layerChanges.Add(new CombatLayer.LayerChange
			{
				Layer = this._transform.gameObject.layer
			});
			this._targetLayerMask = (int)(LayerManager.Mask.OverlapLayer & ~(int)LayerManager.GetMask(this._transform.gameObject.layer));
			this._myCombatObject = base.GetComponent<CombatObject>();
			if (this._myCombatObject.IsBomb)
			{
				this._targetCollidingLayer = 19;
			}
			else
			{
				this._targetCollidingLayer = 20;
			}
		}

		private void Update()
		{
			if (this._updater.ShouldHalt() || !this._shouldRevertLayerLater)
			{
				return;
			}
			this.TryToRevertLayer();
		}

		private void SetLayer(int layer)
		{
			LayerManager.SetLayerRecursively(this._transform, layer, this.ignoreLayer);
		}

		public void ChangeLayer(LayerManager.Layer layer, CombatLayer.ILayerChanger changer)
		{
			this._layerChanges.Add(new CombatLayer.LayerChange
			{
				Changer = changer,
				Layer = (int)layer
			});
			this.SetLayer(this._layerChanges[this._layerChanges.Count - 1].Layer);
		}

		public void RevertLayer(CombatLayer.ILayerChanger changer)
		{
			this._layerChanges.RemoveAll((CombatLayer.LayerChange l) => l.Changer == changer);
			this.TryToRevertLayer();
		}

		private void TryToRevertLayer()
		{
			int hits = this.GetHits();
			this._shouldRevertLayerLater = (hits > 0);
			if (hits == 0)
			{
				CombatLayer.Log.DebugFormat("{0} changing layer succesfully from {1} to {2} after {3} tries.", new object[]
				{
					base.gameObject.name,
					(LayerManager.Layer)base.gameObject.layer,
					(LayerManager.Layer)this._layerChanges[this._layerChanges.Count - 1].Layer,
					this._numOfTries
				});
				this._numOfTries = 0;
				this.SetLayer(this._layerChanges[this._layerChanges.Count - 1].Layer);
			}
			else
			{
				this._numOfTries++;
				if (base.gameObject.layer != this._targetCollidingLayer)
				{
					this.SetLayer(this._targetCollidingLayer);
				}
			}
		}

		private int GetHits()
		{
			this.ClearCollidersBeneath();
			int num = Physics.OverlapSphereNonAlloc(this._transform.position, this._myCombatObject.Movement.Info.LayerChangeOverlapRadius, this._collidersBeneath, this._targetLayerMask);
			this._myColliders.AddRange(Array.FindAll<Collider>(this._collidersBeneath, new Predicate<Collider>(this.ShouldIgnoreCollider)));
			num -= this._myColliders.Count;
			this._myColliders.Clear();
			return num;
		}

		private bool ShouldIgnoreCollider(Collider targetCollider)
		{
			if (targetCollider == null)
			{
				return false;
			}
			CombatRef component = targetCollider.gameObject.GetComponent<CombatRef>();
			if (CombatLayer.IsValidCombatRef(component))
			{
				return this.IsSameTeam(component);
			}
			if (!CombatLayer.TargetHasAParent(targetCollider))
			{
				return false;
			}
			component = targetCollider.transform.parent.GetComponent<CombatRef>();
			return CombatLayer.IsValidCombatRef(component) && this.IsSameTeam(component);
		}

		private bool IsSameTeam(CombatRef combatRef)
		{
			return combatRef.Combat.Team == this._myCombatObject.Team;
		}

		private static bool TargetHasAParent(Collider targetCollider)
		{
			return targetCollider.transform.parent != null;
		}

		private static bool IsValidCombatRef(CombatRef combatRef)
		{
			return combatRef && combatRef.Combat;
		}

		private void ClearCollidersBeneath()
		{
			for (int i = 0; i < this._collidersBeneath.Length; i++)
			{
				this._collidersBeneath[i] = null;
			}
		}

		public void OnSendToCache()
		{
			if (this._layerChanges.Count > 1)
			{
				this._layerChanges.RemoveRange(1, this._layerChanges.Count - 1);
			}
		}

		public void OnGetFromCache()
		{
		}

		public void OnEnable()
		{
			if (this._transform != null && this._layerChanges != null && this._layerChanges.Count == 0)
			{
				this._layerChanges.Add(new CombatLayer.LayerChange
				{
					Layer = this._transform.gameObject.layer
				});
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CombatLayer));

		private CombatObject _myCombatObject;

		private TimedUpdater _updater;

		private int _targetLayerMask;

		private int _targetCollidingLayer;

		private bool _shouldRevertLayerLater;

		private readonly List<CombatLayer.LayerChange> _layerChanges = new List<CombatLayer.LayerChange>();

		private readonly List<Collider> _myColliders = new List<Collider>(16);

		private Transform _transform;

		private readonly Collider[] _collidersBeneath = new Collider[10];

		public int ignoreLayer;

		private int _numOfTries;

		private struct LayerChange
		{
			public CombatLayer.ILayerChanger Changer;

			public int Layer;
		}

		public interface ILayerChanger
		{
		}
	}
}
