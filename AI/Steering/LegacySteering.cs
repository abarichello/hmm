using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public class LegacySteering : ISteering
	{
		public bool MovingCar
		{
			get
			{
				return this._vertical != 0f;
			}
		}

		public bool AcceleratingForward
		{
			get
			{
				return this._vertical > 0f;
			}
		}

		public Vector3 MousePosition
		{
			get
			{
				return Vector3.zero;
			}
		}

		public void Update(ISteeringBotContext botContext)
		{
			botContext.BotInput.SteerInput(this._horizontal, this._vertical);
		}

		public void StopMoving(ISteeringBotContext botContext)
		{
			this._horizontal = 0f;
			this._vertical = 0f;
		}

		public void SteerToPosition(ISteeringBotContext botContext, Vector3 position)
		{
			Vector3 vector = position - botContext.BotSubject.SubjectTransform.position;
			this._vertical = 1f;
			Vector3 vector2 = botContext.BotSubject.SubjectTransform.InverseTransformDirection(vector);
			this._horizontal = ((Mathf.Abs(vector2.x) <= 1f) ? vector2.x : vector2.normalized.x);
		}

		private float _horizontal;

		private float _vertical;
	}
}
