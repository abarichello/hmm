using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class CaterpillarDamageListener : GameHubBehaviour
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub == null)
			{
				Debug.LogWarning("GameHub is not running, couldn't load data");
				base.enabled = false;
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
			GameHubBehaviour.Hub.Resources.PrefabPreCache(this.EffectPrefab, 1);
		}

		private void Start()
		{
			this._combat = base.GetComponentInParent<CombatObject>();
			if (this._combat != null)
			{
				this._combat.OnDamageReceivedFullData += this.OnDamageReceived;
				this._combat.ListenToObjectUnspawn += this.ListenToObjectUnspawn;
				return;
			}
			base.enabled = false;
		}

		private void Update()
		{
			if (this._effectInstance == null)
			{
				return;
			}
			if (Time.time - this._effectStart > this.EffectDuration)
			{
				this._effectInstance.Destroy(BaseFX.EDestroyReason.Default);
				this._effectInstance = null;
			}
		}

		private void OnDestroy()
		{
			if (this._combat != null)
			{
				this._combat.OnDamageReceivedFullData -= this.OnDamageReceived;
				this._combat.ListenToObjectUnspawn -= this.ListenToObjectUnspawn;
				this._combat = null;
			}
			if (this._effectInstance)
			{
				this._effectInstance.Destroy(BaseFX.EDestroyReason.Default);
				this._effectInstance = null;
			}
		}

		private void OnDamageReceived(float amount, Vector3 direction, Vector3 position)
		{
			if (direction.Equals(Vector3.zero))
			{
				return;
			}
			if (!PhysicsUtils.IsInFront(base.transform.position, base.transform.forward, position) || !PhysicsUtils.IsFacing(base.transform.forward, direction))
			{
				return;
			}
			this._effectStart = Time.time;
			if (Vector3.Dot(this.Direction, base.transform.forward) >= 0f)
			{
				this.Direction = direction;
				this.Position = position;
			}
			if (this._effectInstance == null)
			{
				this._effectInstance = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.EffectPrefab, base.transform.position, base.transform.rotation);
				GameHubBehaviour.Hub.Drawer.AddEffect(this._effectInstance.transform);
				this._effectInstance.baseMasterVFX = this.EffectPrefab;
				this._effectInstance = this._effectInstance.Activate(this._combat.Id, this._combat.Id, base.transform);
			}
		}

		private void ListenToObjectUnspawn(CombatObject obj, UnspawnEvent msg)
		{
			if (this._effectInstance)
			{
				this._effectInstance.Destroy(BaseFX.EDestroyReason.Default);
				this._effectInstance = null;
			}
		}

		private CombatObject _combat;

		public MasterVFX EffectPrefab;

		public float EffectDuration;

		private MasterVFX _effectInstance;

		private float _effectStart;

		[HideInInspector]
		public Vector3 Direction;

		[HideInInspector]
		public Vector3 Position;
	}
}
