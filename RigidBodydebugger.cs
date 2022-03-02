using System;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class RigidBodydebugger : MonoBehaviour
	{
		public void Awake()
		{
			this._rigidbody = base.GetComponent<Rigidbody>();
		}

		public void Update()
		{
			Debug.LogWarningFormat("pos {0} vel {1} rot {2} ang {3}", new object[]
			{
				this._rigidbody.position,
				this._rigidbody.velocity,
				this._rigidbody.rotation,
				this._rigidbody.angularVelocity
			});
			Debug.DrawRay(this._rigidbody.position, this._rigidbody.velocity, Color.magenta, 30f);
		}

		private Rigidbody _rigidbody;
	}
}
