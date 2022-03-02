using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting;
using Pocketverse;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class CharacterSelectionClientState : GameState
	{
		protected override IObservable<Unit> OnMyLevelLoadedAsync()
		{
			return Observable.First<Unit>(this._lifetimeNotifications.OnStarted);
		}

		[Inject]
		private LocalCharacterSelectionLifetimeNotifications _lifetimeNotifications;
	}
}
