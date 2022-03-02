using System;
using System.Linq;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class ClientStateMachineInstaller : MonoInstaller<ClientStateMachineInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.ParentContainers.First<DiContainer>().BindInstance<StateMachine>(this._stateMachine);
			base.Container.ParentContainers.First<DiContainer>().BindInstance<LoadingState>(this._loadingState);
		}

		[SerializeField]
		private StateMachine _stateMachine;

		[SerializeField]
		private LoadingState _loadingState;
	}
}
