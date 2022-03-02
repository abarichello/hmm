using System;
using HeavyMetalMachines.HostingPlatforms;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.Publishing
{
	public class PublisherInitializer : IPublisherInitializer
	{
		public PublisherInitializer(INotifyPublisherSessionInvitation notifyPlatformSessionInvitation, IObservePublisherGroupInvite observePublisherGroupInvite, IPublisherGroupInviteReceivedHandler publisherGroupInviteReceivedHandler, IGetHostPlatform getHostPlatform, ILogger<PublisherInitializer> logger)
		{
			this._notifyPlatformSessionInvitation = notifyPlatformSessionInvitation;
			this._observePublisherGroupInvite = observePublisherGroupInvite;
			this._publisherGroupInviteReceivedHandler = publisherGroupInviteReceivedHandler;
			this._getHostPlatform = getHostPlatform;
			this._logger = logger;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this.SubscribeToSessionInvitation();
				this.SubscribePublisherGroupInvite();
				return Observable.ReturnUnit();
			});
		}

		private void SubscribeToSessionInvitation()
		{
			if (!this.ShouldListenForSessionInvitations())
			{
				return;
			}
			ObservableExtensions.Subscribe<SessionInvitationData>(Observable.Do<SessionInvitationData>(Platform.Current.OnSessionInvitation, delegate(SessionInvitationData sessionInvitationData)
			{
				this._notifyPlatformSessionInvitation.Notify(sessionInvitationData);
			}));
			this._logger.Info("Started listening for session invitations from platform.");
		}

		private bool ShouldListenForSessionInvitations()
		{
			HostPlatform current = this._getHostPlatform.GetCurrent();
			return current == 1 || current == 2;
		}

		private void SubscribePublisherGroupInvite()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<PublisherGroupData, Unit>(this._observePublisherGroupInvite.Observe(), (PublisherGroupData data) => this._publisherGroupInviteReceivedHandler.Handle(data.GroupId, data.IsOwnerCrossplayEnabled, data.OwnerPublisher.SwordfishUniqueName)));
		}

		private readonly INotifyPublisherSessionInvitation _notifyPlatformSessionInvitation;

		private readonly IObservePublisherGroupInvite _observePublisherGroupInvite;

		private readonly IPublisherGroupInviteReceivedHandler _publisherGroupInviteReceivedHandler;

		private readonly IGetHostPlatform _getHostPlatform;

		private readonly ILogger<PublisherInitializer> _logger;
	}
}
