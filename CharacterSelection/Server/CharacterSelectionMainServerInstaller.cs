using System;
using HeavyMetalMachines.Server;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Server
{
	public class CharacterSelectionMainServerInstaller : MonoInstaller<CharacterSelectionMainServerInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.BindInstance<PickModeServerSetup>(this._legacyServerPickState);
			base.Container.BindInstance<CharacterSelectionServerState>(this._characterSelectionServerState);
			base.Container.Bind<IProceedToServerCharacterSelectionState>().To<ProceedToServerCharacterSelectionState>().AsTransient();
		}

		[SerializeField]
		private PickModeServerSetup _legacyServerPickState;

		[SerializeField]
		private CharacterSelectionServerState _characterSelectionServerState;
	}
}
