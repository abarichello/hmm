using System;
using HeavyMetalMachines.Matches;
using Hoplon.Input.Infra;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.Input.NoInputDetection.Business
{
	public class NoInputDetectedNotifier : INoInputDetectedNotifier
	{
		public NoInputDetectedNotifier(ILogger<NoInputDetectedNotifier> logger, INoInputDetectedRpc noInputDetectedRpc, IGetCurrentMatch getCurrentMatch, IInputJoystickDetectionService inputJoystickDetectionService, IInputGetAnyButtonService inputGetAnyButtonService)
		{
			this._logger = logger;
			this._noInputDetectedRpc = noInputDetectedRpc;
			this._getCurrentMatch = getCurrentMatch;
			this._inputJoystickDetectionService = inputJoystickDetectionService;
			this._inputGetAnyButtonService = inputGetAnyButtonService;
		}

		public IObservable<bool> OnNotification
		{
			get
			{
				return this._subject;
			}
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Repeat<Unit>(Observable.First<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.First<Unit>(this.ObserveJoystickDisconnection()), this.ObserveAnyInput())));
		}

		private IObservable<Unit> ObserveAnyInput()
		{
			return Observable.Do<Unit>(this._inputGetAnyButtonService.ObserveAnyInput(), delegate(Unit _)
			{
				this._subject.OnNext(false);
			});
		}

		private IObservable<Unit> ObserveJoystickDisconnection()
		{
			return Observable.AsUnitObservable<Unit>(Observable.Do<Unit>(this._inputJoystickDetectionService.ObserveJoystickDisconnection(), delegate(Unit _)
			{
				this.HandleJoystickDisconnection();
			}));
		}

		private void HandleJoystickDisconnection()
		{
			if (this._getCurrentMatch.GetIfExisting() != null)
			{
				this._noInputDetectedRpc.SendNoInputDetectedMessage();
			}
			this._subject.OnNext(true);
		}

		private readonly INoInputDetectedRpc _noInputDetectedRpc;

		private readonly IGetCurrentMatch _getCurrentMatch;

		private readonly IInputJoystickDetectionService _inputJoystickDetectionService;

		private readonly IInputGetAnyButtonService _inputGetAnyButtonService;

		private readonly ILogger<NoInputDetectedNotifier> _logger;

		private readonly Subject<bool> _subject = new Subject<bool>();
	}
}
