using System;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	[RequireComponent(typeof(SphereCollider))]
	public class CameraFocusTrigger : GameHubBehaviour
	{
		private void Start()
		{
			this.position = base.transform.position;
			this.size = base.GetComponent<SphereCollider>().radius;
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				Object.Destroy(this);
			}
		}

		private void OnTriggerEnter(Collider collider)
		{
			CameraFocusTrigger.activeTrigger = this;
			Identifiable component = collider.GetComponent<Identifiable>();
			if (component != null && component.ObjId == GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
			{
				this.currentCollider = collider;
				Vector3 vector = collider.transform.position - base.transform.position;
				vector.y = 0f;
				if (!this.middleRamp || (this.middleRamp && Vector3.Dot(base.transform.forward, vector) > 0f))
				{
					this._gameCamera.SetFocusTarget(this);
				}
			}
		}

		private void OnTriggerExit(Collider collider)
		{
			if (CameraFocusTrigger.activeTrigger != this)
			{
				return;
			}
			if (collider == this.currentCollider)
			{
				this._gameCamera.SetFocusTarget(null);
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
			float radius = ((SphereCollider)base.GetComponent<Collider>()).radius;
			Gizmos.DrawSphere(base.transform.position, radius);
			if (this.middleRamp)
			{
				Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
				Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.forward * radius);
			}
		}

		[InjectOnClient]
		private IGameCamera _gameCamera;

		private static CameraFocusTrigger activeTrigger;

		private Collider currentCollider;

		public Transform Target;

		public Vector3 position;

		public float size;

		public bool middleRamp;

		public float extraViewAreaMultiplier = 2f;
	}
}
