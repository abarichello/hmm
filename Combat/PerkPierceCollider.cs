using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkPierceCollider : BasePerk, IPerkWithCollision
	{
		public override void PerkInitialized()
		{
			base.PerkInitialized();
			this._myCollider = base.GetComponent<Collider>();
			this._body = base.GetComponent<Rigidbody>();
		}

		public int Priority()
		{
			return 0;
		}

		public void OnHit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool isBarrier)
		{
			Physics.IgnoreCollision(other, this._myCollider);
			this._ignoredColliders.Add(other);
			this._body.position = this.Effect.LastPosition;
		}

		public void OnStay(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnEnter(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnExit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public override void PerkDestroyed(DestroyEffectMessage destroyEffectMessage)
		{
			base.PerkDestroyed(destroyEffectMessage);
			for (int i = 0; i < this._ignoredColliders.Count; i++)
			{
				Physics.IgnoreCollision(this._ignoredColliders[i], this._myCollider, false);
			}
			this._ignoredColliders.Clear();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkPierceCollider));

		private List<Collider> _ignoredColliders = new List<Collider>();

		private Collider _myCollider;

		private Rigidbody _body;
	}
}
