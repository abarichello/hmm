using System;
using System.Diagnostics;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class PropController : GameHubBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<Collider> OnBreakEvent;

		private void Awake()
		{
			this.selfCollider = base.GetComponent<Collider>();
			this.selfCollider.isTrigger = true;
			this.colliders = base.GetComponentsInChildren<Collider>();
			this.rigidbodies = base.GetComponentsInChildren<Rigidbody>();
			for (int i = 0; i < this.colliders.Length; i++)
			{
				Collider collider = this.colliders[i];
				if (!(collider == this.selfCollider))
				{
					collider.enabled = false;
				}
			}
			for (int j = 0; j < this.rigidbodies.Length; j++)
			{
				Rigidbody rigidbody = this.rigidbodies[j];
				rigidbody.Sleep();
			}
			this.projectileLayer = 1 << LayerMask.NameToLayer("Projectile");
			base.gameObject.layer = LayerMask.NameToLayer("Props");
			this.validColliderLayer = LayerMask.GetMask(new string[]
			{
				"PropCollider",
				"Projectile"
			});
			for (int k = 0; k < this.dependencies.Length; k++)
			{
				PropController propController = this.dependencies[k];
				propController.OnBreakEvent += this.OnParentBreak;
			}
		}

		private void OnParentBreak(Collider col)
		{
			this.brokenDependencies++;
			if (this.brokenDependencies < this.dependencies.Length)
			{
				return;
			}
			this.Break(col);
		}

		private void Update()
		{
			if (!this.triggered)
			{
				return;
			}
			this.lifeTime -= Time.deltaTime;
			if (!this.dead && this.lifeTime < 0f)
			{
				for (int i = 0; i < this.colliders.Length; i++)
				{
					Collider collider = this.colliders[i];
					collider.enabled = false;
				}
				for (int j = 0; j < this.rigidbodies.Length; j++)
				{
					Rigidbody rigidbody = this.rigidbodies[j];
					rigidbody.WakeUp();
				}
				this.dead = true;
				this.lifeTime = 5f;
			}
			if (this.dead && this.lifeTime < 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}

		private void OnFixedUpdate()
		{
			for (int i = 0; i < this.rigidbodies.Length; i++)
			{
				Rigidbody rigidbody = this.rigidbodies[i];
				rigidbody.AddForce(Physics.gravity * base.GetComponent<Rigidbody>().mass);
			}
		}

		private void OnTriggerEnter(Collider col)
		{
			this.Break(col);
		}

		private void Break(Collider col)
		{
			if (this.triggered)
			{
				return;
			}
			int num = 1 << col.gameObject.layer;
			if ((num & this.validColliderLayer) == 0)
			{
				return;
			}
			this.triggered = true;
			this.selfCollider.enabled = false;
			for (int i = 0; i < this.colliders.Length; i++)
			{
				Collider collider = this.colliders[i];
				collider.enabled = true;
			}
			for (int j = 0; j < this.rigidbodies.Length; j++)
			{
				Rigidbody rigidbody = this.rigidbodies[j];
				rigidbody.WakeUp();
				if (num == this.projectileLayer)
				{
					rigidbody.AddExplosionForce((float)Random.Range(400, 800) * rigidbody.mass, col.transform.position, 10f);
				}
				else
				{
					rigidbody.AddForce(0f, (float)Random.Range(200, 300) * rigidbody.mass, 0f);
				}
			}
			this.lifeTime = 5f;
			if (this.OnBreakEvent != null)
			{
				this.OnBreakEvent(col);
			}
		}

		private Rigidbody[] rigidbodies;

		private Collider[] colliders;

		public bool isBreakeable;

		private int validColliderLayer;

		private Collider selfCollider;

		private bool triggered;

		private float lifeTime;

		private bool dead;

		private int projectileLayer;

		public PropController[] dependencies;

		private int brokenDependencies;
	}
}
