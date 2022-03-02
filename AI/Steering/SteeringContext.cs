using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.AI.Steering
{
	public class SteeringContext : ISteeringContext, IAITask
	{
		public ISteering Steering
		{
			get
			{
				return this._steering;
			}
		}

		public void SetBotContext(ISteeringBotContext botContext)
		{
			this._botContext = botContext;
			foreach (ISteeringBehaviourParameters parameters in this._botContext.BotParameters.SteeringBehaviours)
			{
				ISteeringBehaviour item = this._behaviourFactory.CreateBehaviour(parameters);
				this._behaviours.Add(item);
			}
		}

		public void Update(float deltaTime)
		{
			if (!this._botContext.IsBotControlled)
			{
				return;
			}
			this._result.Clear();
			foreach (ISteeringBehaviour steeringBehaviour in this._behaviours)
			{
				steeringBehaviour.Evaluate(this._botContext, this._result);
			}
			Vector3 zero = Vector3.zero;
			foreach (ISteeringContextMapEvaluator steeringContextMapEvaluator in this._evaluators)
			{
				if (steeringContextMapEvaluator.EvaluateDirection(this._result, out zero))
				{
					this._lastResult = steeringContextMapEvaluator.LastResult;
					break;
				}
			}
			if (zero == Vector3.zero)
			{
				this.Steering.Update(this._botContext);
				return;
			}
			Vector3 position = this._botContext.BotSubject.SubjectTransform.position + zero * 50f;
			if (this._botContext.DesiredDestination != null)
			{
				this.Steering.SteerToPosition(this._botContext, position);
			}
			else
			{
				this.Steering.StopMoving(this._botContext);
			}
			this.Steering.Update(this._botContext);
		}

		public void Format(ISteeringContextFormater formater)
		{
			if (this._lastResult == null)
			{
				return;
			}
			formater.FormatDangers(this._result.DangerMap);
			formater.FormatInterests(this._result.InterestMap);
			formater.FormatResults(this._lastResult);
			for (int i = 0; i < this._behaviours.Count; i++)
			{
				this._behaviours[i].Format(formater);
			}
		}

		[Inject]
		private ISteering _steering;

		[Inject]
		private ISteeringContextResult _result;

		[Inject]
		private List<ISteeringContextMapEvaluator> _evaluators;

		[Inject]
		private ISteeringBehaviourFactory _behaviourFactory;

		private List<ISteeringBehaviour> _behaviours = new List<ISteeringBehaviour>();

		private ISteeringBotContext _botContext;

		private float[] _lastResult;
	}
}
