using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionRpcCommunicationInstaller : MonoInstaller<CharacterSelectionRpcCommunicationInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IInitializeClientsCommunication>().To<InitializeRpcClientsCommunication>().AsTransient();
			base.Container.Bind<CharacterSelectionRpcStorage>().AsSingle();
			base.Container.BindInstance<CharacterSelectionRpcPrefabs>(this._rpcPrefabs);
		}

		[SerializeField]
		private CharacterSelectionRpcPrefabs _rpcPrefabs;
	}
}
