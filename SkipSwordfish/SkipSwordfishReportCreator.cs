using System;
using HeavyMetalMachines.ReportSystem;
using HeavyMetalMachines.ReportSystem.DataTransferObjects;
using UniRx;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishReportCreator : IReportCreator
	{
		public IObservable<bool> CreateReport(ReportRequestBag bag)
		{
			return Observable.Select<long, bool>(Observable.Timer(TimeSpan.FromSeconds(1.0)), delegate(long _)
			{
				this._result = !this._result;
				return this._result;
			});
		}

		private bool _result;
	}
}
