using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	internal class GoToDirectionBehaviour : ISteeringBehaviour
	{
		public GoToDirectionBehaviour(IGoToDirectionParameters parameters, ISteeringContextParameters steeringParameters)
		{
			this._parameters = parameters;
			this._steeringParameters = steeringParameters;
			this._lastEvaluation = new float[this._steeringParameters.DirectionCount];
		}

		public void Evaluate(ISteeringBotContext steeringBot, ISteeringContextResult result)
		{
			if (steeringBot.DesiredDestination == null)
			{
				for (int i = 0; i < this._steeringParameters.DirectionCount; i++)
				{
					this._lastEvaluation[i] = 0f;
				}
				return;
			}
			Vector3 normalized = (steeringBot.DesiredDestination.Value - steeringBot.BotSubject.SubjectTransform.position).normalized;
			for (int j = 0; j < this._steeringParameters.DirectionCount; j++)
			{
				Vector3 direction = this._steeringParameters.GetDirection(j);
				float num = Vector3.Angle(direction, normalized) * 0.017453292f;
				this._lastEvaluation[j] = Mathf.Max(0f, Mathf.Cos(num) * this._parameters.Force);
			}
			this.ApplyLastEvaluation(steeringBot, result);
		}

		public void ApplyLastEvaluation(ISteeringBotContext steeringBot, ISteeringContextResult result)
		{
			for (int i = 0; i < this._steeringParameters.DirectionCount; i++)
			{
				result.InterestMap[i] = Mathf.Max(result.InterestMap[i], this._lastEvaluation[i]);
			}
		}

		public void Format(ISteeringContextFormater formater)
		{
			formater.FormatBehaviour(this, this._lastEvaluation, null);
		}

		private ISteeringContextParameters _steeringParameters;

		private IGoToDirectionParameters _parameters;

		private readonly float[] _lastEvaluation;
	}
}
