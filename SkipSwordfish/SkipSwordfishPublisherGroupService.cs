using System;
using HeavyMetalMachines.Publishing;
using UniRx;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishPublisherGroupService : IPublisherGroupService
	{
		public IObservable<Unit> Initialize()
		{
			return Observable.ReturnUnit();
		}

		public IObservable<PublisherGroupData> OnInviteReceived
		{
			get
			{
				return Observable.Never<PublisherGroupData>(default(PublisherGroupData));
			}
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.ReturnUnit();
		}
	}
}
