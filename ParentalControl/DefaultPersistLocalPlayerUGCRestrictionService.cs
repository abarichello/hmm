using System;
using UniRx;

namespace HeavyMetalMachines.ParentalControl
{
	public class DefaultPersistLocalPlayerUGCRestrictionService : IPersistLocalPlayerUGCRestrictionService
	{
		public IObservable<Unit> Persist(bool ugcEnabled)
		{
			return Observable.ReturnUnit();
		}
	}
}
