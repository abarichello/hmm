using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.UpdateStream;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	[RequireComponent(typeof(TransformStream))]
	public class FollowCombatServerGadgetBodyMovement : MonoBehaviour, IGadgetBodyMovement
	{
		public void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			this._body = body;
			IParameterTomate<ICombatObject> parameterTomate = (IParameterTomate<ICombatObject>)this._combatToFollow.ParameterTomate;
			this._ownerTransform = parameterTomate.GetValue(gadgetContext).Transform;
			IParameterTomate<float> parameterTomate2 = (IParameterTomate<float>)this._speed.ParameterTomate;
			this._speedValue = parameterTomate2.GetValue(gadgetContext);
		}

		public Vector3 GetPosition(float elapsedTime)
		{
			Vector3 position = this._ownerTransform.position;
			Vector3 position2 = this._body.Position;
			Vector3 vector = position - position2;
			Vector3 normalized = vector.normalized;
			Vector3 vector2 = normalized * this._speedValue * Time.deltaTime;
			if (10f > vector.sqrMagnitude)
			{
				this.Finished = true;
			}
			if (vector2.sqrMagnitude > vector.sqrMagnitude)
			{
				vector2 = vector;
			}
			return position2 + vector2;
		}

		public Vector3 GetDirection()
		{
			return (this._body.Position - this._ownerTransform.position).normalized;
		}

		public bool Finished { get; private set; }

		public void Destroy()
		{
			this.Finished = false;
			this._body = null;
			this._ownerTransform = null;
			this._speedValue = 0f;
		}

		[SerializeField]
		protected BaseParameter _combatToFollow;

		[SerializeField]
		protected BaseParameter _speed;

		private IGadgetBody _body;

		private Transform _ownerTransform;

		private float _speedValue;

		private const float SqrFinishTolerance = 10f;
	}
}
