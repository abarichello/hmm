using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	internal class AttachToDummyBodyMovement : MonoBehaviour, IGadgetBodyMovement
	{
		private void LateUpdate()
		{
			if (this._isInitialized && this._synchronizeRotation)
			{
				base.transform.rotation = this._dummyTransform.rotation;
			}
		}

		public Vector3 GetPosition(float elapsedTime)
		{
			return this._dummyTransform.position + this.GetDummyVelocity();
		}

		public Vector3 GetDirection()
		{
			if (this._synchronizeRotation)
			{
				return this._dummyTransform.forward;
			}
			return base.transform.forward;
		}

		public void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			this._combatObject = this._combatObjectParameter.GetValue(gadgetContext);
			this._dummyRigidBody = ((Component)this._combatObject).GetComponent<Rigidbody>();
			this._dummyHasRigidBody = (this._dummyRigidBody != null);
			if (this._combatObject == null)
			{
				string text = string.Empty;
				if (gadgetContext != null)
				{
					text = gadgetContext.ToString();
				}
				int num = -1;
				if (body != null)
				{
					num = body.Id;
				}
				int num2 = -1;
				if (eventContext != null)
				{
					num2 = eventContext.Id;
				}
				AttachToDummyBodyMovement.Log.WarnFormatStackTrace("Combat object was null. DummyKind: {0}; CustomDummyName: {1}; GadgetContext: {2}; BodyId: {3} EventContextId: {4}", new object[]
				{
					this._dummyKind,
					this._customDummyName,
					text,
					num,
					num2
				});
				return;
			}
			this._dummyTransform = this._combatObject.Dummy.GetDummy(this._dummyKind, this._customDummyName, null);
			this._isInitialized = true;
		}

		public bool Finished
		{
			get
			{
				return false;
			}
		}

		public void Destroy()
		{
			this._dummyTransform = null;
			this._combatObject = null;
			this._isInitialized = false;
			this._dummyRigidBody = null;
			this._dummyHasRigidBody = false;
		}

		public ICombatObject GetTarget()
		{
			return this._combatObject;
		}

		private Vector3 GetDummyVelocity()
		{
			if (this._dummyHasRigidBody)
			{
				return this._dummyRigidBody.velocity * Time.deltaTime;
			}
			return Vector3.zero;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(AttachToDummyBodyMovement));

		[SerializeField]
		private CombatObjectParameter _combatObjectParameter;

		[SerializeField]
		private CDummy.DummyKind _dummyKind;

		[SerializeField]
		private string _customDummyName;

		[Tooltip("Wether to syncrhonize or not the rotation of this body with the combat object.")]
		[SerializeField]
		private bool _synchronizeRotation = true;

		private Transform _dummyTransform;

		private ICombatObject _combatObject;

		private bool _isInitialized;

		private Rigidbody _dummyRigidBody;

		private bool _dummyHasRigidBody;
	}
}
