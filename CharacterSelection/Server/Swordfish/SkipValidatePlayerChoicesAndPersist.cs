using System;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using UniRx;

namespace HeavyMetalMachines.CharacterSelection.Server.Swordfish
{
	public class SkipValidatePlayerChoicesAndPersist : IValidatePlayerChoicesAndPersist
	{
		public IObservable<CharacterSelectionResult> Execute(CharacterSelectionResult currentResult)
		{
			return Observable.Return<CharacterSelectionResult>(currentResult);
		}
	}
}
