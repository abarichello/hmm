using System;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.ReportSystem.DataTransferObjects;
using HeavyMetalMachines.Swordfish;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.ReportSystem
{
	public class ReportCreator : IReportCreator
	{
		public ReportCreator(ICustomWS customWs, ILogger<ReportCreator> log)
		{
			this._log = log;
			this._customWS = customWs;
		}

		public IObservable<bool> CreateReport(ReportRequestBag bag)
		{
			return this.CreateReportCustomWsCall("CreateReport", bag);
		}

		private IObservable<bool> CreateReportCustomWsCall(string methodName, ReportRequestBag bag)
		{
			return Observable.Select<NetResult, bool>(this._customWS.ExecuteAsObservable(methodName, bag.Serialize()), delegate(NetResult netResult)
			{
				if (!netResult.Success)
				{
					this._log.ErrorFormat("Failed report creation code={1} msg={0}", new object[]
					{
						netResult.Msg,
						netResult.Error
					});
				}
				return netResult.Success;
			});
		}

		private readonly ICustomWS _customWS;

		private ILogger<ReportCreator> _log;
	}
}
