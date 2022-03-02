using System;
using ClientAPI.Service.Interfaces;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.Platforms
{
	public class PlatformNotifications : IPlatformNotifications
	{
		public PlatformNotifications(ILogin swordfishLoginService, ILogger<PlatformNotifications> logger)
		{
			this._swordfishLoginService = swordfishLoginService;
			this._logger = logger;
		}

		public IObservable<Unit> OnResumedFromSuspension
		{
			get
			{
				return Platform.Current.OnResumedFromSuspension;
			}
		}

		public IObservable<Unit> OnUserSignedOut
		{
			get
			{
				return Observable.AsUnitObservable<EventArgs>(Observable.Do<EventArgs>(Observable.DoOnCancel<EventArgs>(Observable.DoOnSubscribe<EventArgs>(Observable.FromEvent<EventHandler<EventArgs>, EventArgs>((Action<EventArgs> h) => delegate(object sender, EventArgs e)
				{
					h(e);
				}, delegate(EventHandler<EventArgs> handler)
				{
					this._swordfishLoginService.OnPublisherUserSignOut += handler;
				}, delegate(EventHandler<EventArgs> handler)
				{
					this._swordfishLoginService.OnPublisherUserSignOut -= handler;
				}), delegate()
				{
					this._logger.Info("Someone started listening to OnPublisherUserSignOut notifications.");
				}), delegate()
				{
					this._logger.Info("Someone stopped listening to OnPublisherUserSignOut notifications.");
				}), delegate(EventArgs _)
				{
					this._logger.Info("User signed out of the platform.");
				}));
			}
		}

		private readonly ILogin _swordfishLoginService;

		private readonly ILogger<PlatformNotifications> _logger;
	}
}
