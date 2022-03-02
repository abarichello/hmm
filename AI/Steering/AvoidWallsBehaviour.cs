using System;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.AI.Steering
{
	public class AvoidWallsBehaviour : ISteeringBehaviour
	{
		public void Evaluate(ISteeringBotContext steeringBot, ISteeringContextResult result)
		{
			Vector3 normalized = steeringBot.BotSubject.SubjectRigidbody.velocity.normalized;
			Vector3 position = steeringBot.BotSubject.SubjectTransform.position;
			float axisIndex = this._steeringParameters.RoundToIndex(normalized);
			for (int i = 0; i < this._steeringParameters.DirectionCount; i++)
			{
				Vector3 direction = this._steeringParameters.GetDirection(i);
				if (Physics.Raycast(position, direction, 50f, LayerManager.GetWallMask(steeringBot.IsCarryingBomb)))
				{
					float num = Vector3.Angle(direction, normalized) * 0.017453292f;
					float num2 = Mathf.Cos(num) * 0.5f;
					result.DangerMap[i] = Mathf.Max(result.DangerMap[i], num2);
					int mirroredDirection = this._steeringParameters.GetMirroredDirection(i, axisIndex);
					result.InterestMap[mirroredDirection] = Mathf.Max(result.InterestMap[mirroredDirection], num2);
				}
			}
		}

		public void ApplyLastEvaluation(ISteeringBotContext steeringBot, ISteeringContextResult result)
		{
			throw new NotImplementedException();
		}

		public void Format(ISteeringContextFormater formater)
		{
			throw new NotImplementedException();
		}

		[Inject]
		private ISteeringContextParameters _steeringParameters;
	}
}
