using System;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.AI.Steering
{
	public class AvoidHazardsBehaviour : ISteeringBehaviour
	{
		public void Evaluate(ISteeringBotContext steeringBot, ISteeringContextResult result)
		{
			Vector3 normalized = steeringBot.BotSubject.SubjectRigidbody.velocity.normalized;
			Vector3 position = steeringBot.BotSubject.SubjectTransform.position;
			for (int i = 0; i < this._steeringParameters.DirectionCount; i++)
			{
				Vector3 direction = this._steeringParameters.GetDirection(i);
				Ray ray;
				ray..ctor(position, direction);
				int num = Physics.RaycastNonAlloc(ray, this._raycastHits, 50f, (int)LayerManager.GetMask(LayerManager.Layer.PlayerTrigger));
				for (int j = 0; j < num; j++)
				{
					RaycastHit raycastHit = this._raycastHits[j];
					if (raycastHit.transform.GetComponent<HazardArea>())
					{
						float num2 = Vector3.Angle(direction, normalized) * 0.017453292f;
						result.DangerMap[i] = Mathf.Max(result.DangerMap[i], Mathf.Cos(num2) * 1f);
					}
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

		private const float AvoidStrength = 1f;

		private const int ArraySize = 32;

		private RaycastHit[] _raycastHits = new RaycastHit[32];
	}
}
