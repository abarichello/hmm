using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	public class GadgetBodyLerpMovement : MonoBehaviour, IGadgetBodyMovement
	{
		public bool Finished { get; private set; }

		public Vector3 GetPosition(float elapsedTime)
		{
			return Vector3.Lerp(this._initialPosition, this._targetTransform.position, elapsedTime / this._durationTime);
		}

		public Vector3 GetDirection()
		{
			return base.transform.forward;
		}

		public void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			this.Finished = false;
			this._elapsedTime = 0f;
			GadgetBodyParameter gadgetBodyParameter = (GadgetBodyParameter)BaseParameter.GetParameter(this._targetBody.ContentId);
			GadgetBody value = gadgetBodyParameter.GetValue(gadgetContext);
			this._targetTransform = value.transform;
			this._durationTime = this._duration.GetValue(gadgetContext);
			this._initialPosition = base.transform.position;
		}

		public void Destroy()
		{
		}

		private void Update()
		{
			if (this.Finished)
			{
				return;
			}
			this._elapsedTime += Time.deltaTime;
			this.Finished = (this._elapsedTime >= this._durationTime);
		}

		[SerializeField]
		private GadgetBodyParameter _targetBody;

		[SerializeField]
		private FloatParameter _duration;

		private Vector3 _initialPosition;

		private float _elapsedTime;

		private float _durationTime;

		private Transform _targetTransform;
	}
}
