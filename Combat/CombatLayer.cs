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

		public void ChangeLayer(LayerManager.Layer layer, PerkChangeLayer perk)
		{
			this._layerChanges.Add(new CombatLayer.LayerChange
			{
				Perk = perk,
				Layer = (int)layer
			});
			this.SetLayer(this._layerChanges[this._layerChanges.Count - 1].Layer);
		}

		public void RevertLayer(PerkChangeLayer perk)
		{
			this._layerChanges.RemoveAll((CombatLayer.LayerChange l) => l.Perk == perk);
			this.TryToRevertLayer();
		}

		private void TryToRevertLayer()
		{
			int hits = this.GetHits();
			this._shouldRevertLayerLater = (hits > 0);
			if (hits == 0)
			{
				this.SetLayer(this._layerChanges[this._layerChanges.Count - 1].Layer);
			}
			else
			{
				for (int i = 0; i < this._collidersBeneath.Length; i++)
				{
					Collider x = this._collidersBeneath[i];
					if (x == null)
					{
						break;
					}
				}
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
			return component && component.Combat && component.Combat.Team == this._myCombatObject.Team;
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

		private struct LayerChange
		{
			public PerkChangeLayer Perk;

			public int Layer;
		}
	}
}
