using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkLookAtDummy : BasePerk
	{
		public override void PerkInitialized()
		{
			base.PerkInitialized();
			this._isClient = GameHubBehaviour.Hub.Net.IsClient();
			if (this._isClient)
			{
				base.enabled = false;
				return;
			}
			this._lookAtTarget = this.Effect.Data.SourceCombat.GetDummy(this._dummy, this._customDummyName);
			if (!this._lookAtTarget)
			{
				base.enabled = false;
			}
			this._transform = base.transform;
			this._rigidbodyID = 0;
			this._rigidbody = base.GetComponent<Rigidbody>();
			if (this._rigidbody)
			{
				this._rigidbodyID = this._rigidbody.GetInstanceID();
			}
		}

		private void FixedUpdate()
		{
			if (this._isClient)
			{
				return;
			}
			if (this._overridePhysics && this._rigidbodyID != 0)
			{
				this._rigidbody.angularVelocity = Vector3.zero;
			}
			Vector3 vector = this._lookAtTarget.position - this._transform.position;
			vector.y = 0f;
			Quaternion quaternion = Quaternion.LookRotation(vector);
			Quaternion rotation = Quaternion.RotateTowards(this._transform.rotation, quaternion, this._maxDegreesPerSecond * Time.fixedDeltaTime);
			this._transform.rotation = rotation;
		}

		[SerializeField]
		private CDummy.DummyKind _dummy;

		[SerializeField]
		private string _customDummyName;

		[SerializeField]
		private float _maxDegreesPerSecond;

		[SerializeField]
		[Tooltip("If true, the angular velocity of the rigidbody will be reset every FixedUpdate. You can also freeze rotation in the rigidbody but it will make it act less realistic.")]
		private bool _overridePhysics = true;

		private Transform _lookAtTarget;

		private int _rigidbodyID;

		private Rigidbody _rigidbody;

		private Transform _transform;

		private bool _isClient;
	}
}
