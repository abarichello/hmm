using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public class DirectionalSteering : ISteering
	{
		public bool MovingCar
		{
			get
			{
				return this._accelerateCar;
			}
		}

		public bool AcceleratingForward
		{
			get
			{
				return this._accelerateCar;
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
			this._currentPosition = Vector3.Slerp(this._currentPosition, this._targetPosition, Time.smoothDeltaTime * botContext.BotParameters.DirectionalSteeringSnapMultiplier);
			botContext.BotInput.SteerInput(this._currentPosition, this._accelerateCar);
		}

		public void StopMoving(ISteeringBotContext botContext)
		{
			this._snapToPosition = true;
			this._accelerateCar = false;
			this._targetPosition = Vector3.zero;
			this._currentPosition = Vector3.zero;
		}

		public void SteerToPosition(ISteeringBotContext botContext, Vector3 position)
		{
			this._targetPosition = position;
			this._accelerateCar = true;
			if (this._snapToPosition)
			{
				this._snapToPosition = false;
				this._currentPosition = this._targetPosition;
			}
		}

		private Vector3 _targetPosition;

		private Vector3 _currentPosition;

		private bool _accelerateCar;

		private bool _snapToPosition = true;
	}
}
