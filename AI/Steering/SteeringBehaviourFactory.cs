using System;
using System.Collections.Generic;
using HeavyMetalMachines.Utils;
using Zenject;

namespace HeavyMetalMachines.AI.Steering
{
	public class SteeringBehaviourFactory : ISteeringBehaviourFactory
	{
		public SteeringBehaviourFactory(DiContainer container)
		{
			this._container = container;
			this._creators = new Dictionary<SteeringBehaviourKind, Func<ISteeringBehaviourParameters, ISteeringBehaviour>>();
			this._creators[SteeringBehaviourKind.GoToDirection] = new Func<ISteeringBehaviourParameters, ISteeringBehaviour>(this.CreateGoToDirection);
			this._creators[SteeringBehaviourKind.AvoidElement] = new Func<ISteeringBehaviourParameters, ISteeringBehaviour>(this.CreateAvoidElement);
		}

		public ISteeringBehaviour CreateBehaviour(ISteeringBehaviourParameters parameters)
		{
			Func<ISteeringBehaviourParameters, ISteeringBehaviour> func;
			if (this._creators.TryGetValue(parameters.Kind, out func))
			{
				return func(parameters);
			}
			throw new NotImplementedException("Behaviour of kind " + parameters.Kind + " is not implemented");
		}

		private ISteeringBehaviour CreateGoToDirection(ISteeringBehaviourParameters parameters)
		{
			return this._container.Instantiate<GoToDirectionBehaviour>(parameters.Enumerate<object>());
		}

		private ISteeringBehaviour CreateAvoidElement(ISteeringBehaviourParameters parameters)
		{
			return this._container.Instantiate<AvoidElementBehaviour>(parameters.Enumerate<object>());
		}

		private Dictionary<SteeringBehaviourKind, Func<ISteeringBehaviourParameters, ISteeringBehaviour>> _creators;

		private DiContainer _container;
	}
}
