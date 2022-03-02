using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkColliderTrail : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			base.PerkInitialized();
			this._collidersPositions.Clear();
			this._startTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (this.UseGadgetRadius)
			{
				this.ColliderRadius = this.Effect.Gadget.Radius;
			}
			this.DropCollider();
		}

		protected void Update()
		{
			if (GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._startTime >= this.IntervalMillis)
			{
				this._startTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
				this.DropCollider();
			}
			for (int i = 0; i < this._collidersPositions.Count; i++)
			{
				this._collidersTransforms[i].position = this._collidersPositions[i];
			}
		}

		private void DropCollider()
		{
			if (this._collidersPositions.Count >= this._collidersTransforms.Count)
			{
				GameObject gameObject = new GameObject();
				SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
				Transform transform = gameObject.transform;
				sphereCollider.radius = this.ColliderRadius;
				sphereCollider.isTrigger = true;
				sphereCollider.gameObject.layer = 13;
				transform.transform.parent = base._trans;
				transform.localPosition = Vector3.zero;
				this._collidersPositions.Add(transform.position);
				this._collidersTransforms.Add(transform);
			}
			else
			{
				this._collidersPositions.Add(base._trans.position);
				this._collidersTransforms[this._collidersPositions.Count - 1].GetComponent<Collider>().enabled = true;
			}
		}

		public override void PerkDestroyed(DestroyEffectMessage destroyEffectMessage)
		{
			for (int i = 0; i < this._collidersTransforms.Count; i++)
			{
				this._collidersTransforms[i].GetComponent<Collider>().enabled = false;
				this._collidersTransforms[i].localPosition = Vector3.zero;
			}
			this._collidersPositions.Clear();
		}

		public int IntervalMillis;

		public float ColliderRadius;

		public bool UseGadgetRadius;

		private List<Vector3> _collidersPositions = new List<Vector3>();

		private List<Transform> _collidersTransforms = new List<Transform>();

		private int _startTime;
	}
}
