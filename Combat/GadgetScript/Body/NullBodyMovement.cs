using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	internal class NullBodyMovement : IGadgetBodyMovement
	{
		public NullBodyMovement(Transform transform)
		{
			this._transform = transform;
		}

		public Vector3 GetPosition(float elapsedTime)
		{
			return (!this._transform.parent) ? this._originalLocalPosition : (this._transform.parent.position + this._originalLocalPosition);
		}

		public Vector3 GetDirection()
		{
			if (this._transform)
			{
				return this._transform.forward;
			}
			Debug.LogWarning(this._transform, this._transform);
			return Vector3.forward;
		}

		public void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			this._originalLocalPosition = this._transform.localPosition;
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
		}

		private Vector3 _originalLocalPosition;

		private Transform _transform;
	}
}
