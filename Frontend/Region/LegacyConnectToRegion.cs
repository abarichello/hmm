using System;
using System.Collections.Generic;
using HeavyMetalMachines.Regions.Business;
using HeavyMetalMachines.Regions.Infra;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.Frontend.Region
{
	public class LegacyConnectToRegion : IConnectToRegion
	{
		public LegacyConnectToRegion(IRegionService regionService, SwordfishLog swordfishLog, ISwordfishWsService swordfishWsService, List<IInitializeOnSwordfishConnected> swordfishInitializations)
		{
			this._regionService = regionService;
			this._swordfishLog = swordfishLog;
			this._swordfishWsService = swordfishWsService;
			this._swordfishInitializations = swordfishInitializations;
		}

		public IObservable<Unit> Connect()
		{
			return Observable.AsUnitObservable<Unit>(Observable.TakeUntil<Unit, Unit>(this._regionService.Initialize(), this._swordfishWsService.OnErrorTimeout()));
		}

		private readonly IRegionService _regionService;

		private readonly SwordfishLog _swordfishLog;

		private readonly ISwordfishWsService _swordfishWsService;

		private readonly IEnumerable<IInitializeOnSwordfishConnected> _swordfishInitializations;
	}
}
