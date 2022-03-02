using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	internal class AvoidElementBehaviour : ISteeringBehaviour
	{
		public AvoidElementBehaviour(IAIStaticSceneElementIterator staticScene, ISteeringContextParameters steeringParameters, IAvoidElementParameters parameters)
		{
			this._lastInterestEvaluation = new float[steeringParameters.DirectionCount];
			this._lastDangerEvaluation = new float[steeringParameters.DirectionCount];
			this._aiStaticScene = staticScene;
			this._steeringParameters = steeringParameters;
			this._parameters = parameters;
			this._evaluateAction = new Action<IAIStaticSceneElement>(this.EvaluateElement);
		}

		public void Evaluate(ISteeringBotContext steeringBot, ISteeringContextResult result)
		{
			if (!steeringBot.IsCarryingBomb && this._parameters.OnlyWhenCarryingBomb)
			{
				return;
			}
			Vector3 normalized = steeringBot.BotSubject.SubjectRigidbody.velocity.normalized;
			Vector3 position = steeringBot.BotSubject.SubjectTransform.position;
			Vector2 vector = Vector2.one * this._parameters.Range;
			Vector2 minBound = position.ToVector2XZ() - vector;
			Vector2 maxBound = position.ToVector2XZ() + vector;
			this.EvaluationData = new AvoidElementBehaviour.ElementEvaluationData
			{
				Heading = normalized,
				Origin = position,
				MinBound = minBound,
				MaxBound = maxBound,
				BotContext = steeringBot
			};
			for (int i = 0; i < this._steeringParameters.DirectionCount; i++)
			{
				this._lastDangerEvaluation[i] = 0f;
				this._lastInterestEvaluation[i] = 0f;
			}
			if (this._parameters.ElementKind == null)
			{
				this._aiStaticScene.CallOnAllElementsInBounds(this.EvaluationData.MinBound, this.EvaluationData.MaxBound, this._evaluateAction);
			}
			else
			{
				this._aiStaticScene.CallOnElementsInBounds(this.EvaluationData.MinBound, this.EvaluationData.MaxBound, this._parameters.ElementKind, this._evaluateAction);
			}
			this.EvaluationData = default(AvoidElementBehaviour.ElementEvaluationData);
			this.ApplyLastEvaluation(steeringBot, result);
		}

		private void EvaluateElement(IAIStaticSceneElement element)
		{
			if (element.AffectedTeam != TeamKind.Zero && element.AffectedTeam != this.EvaluationData.BotContext.BotSubject.Combat.Team)
			{
				return;
			}
			for (int i = 0; i < this._steeringParameters.DirectionCount; i++)
			{
				Vector3 direction = this._steeringParameters.GetDirection(i);
				Ray ray;
				ray..ctor(this.EvaluationData.Origin, direction);
				RaycastHit raycastHit;
				if (element.ElementCollider.Raycast(ray, ref raycastHit, this._parameters.Range))
				{
					float num = this._parameters.DangerForce * (this._parameters.Range - raycastHit.distance) / this._parameters.Range;
					this._lastDangerEvaluation[i] = num;
				}
			}
		}

		public void ApplyLastEvaluation(ISteeringBotContext steeringBot, ISteeringContextResult result)
		{
			for (int i = 0; i < this._steeringParameters.DirectionCount; i++)
			{
				result.DangerMap[i] = Mathf.Max(result.DangerMap[i], this._lastDangerEvaluation[i]);
				result.InterestMap[i] = Mathf.Max(result.InterestMap[i], this._lastInterestEvaluation[i]);
			}
		}

		public void Format(ISteeringContextFormater formater)
		{
			formater.FormatBehaviour(this, this._lastInterestEvaluation, this._lastDangerEvaluation);
		}

		private ISteeringContextParameters _steeringParameters;

		private IAvoidElementParameters _parameters;

		private IAIStaticSceneElementIterator _aiStaticScene;

		private float[] _lastInterestEvaluation;

		private float[] _lastDangerEvaluation;

		private AvoidElementBehaviour.ElementEvaluationData EvaluationData;

		private Action<IAIStaticSceneElement> _evaluateAction;

		private struct ElementEvaluationData
		{
			public Vector3 Heading;

			public Vector3 Origin;

			public Vector2 MinBound;

			public Vector2 MaxBound;

			public ISteeringBotContext BotContext;
		}
	}
}
