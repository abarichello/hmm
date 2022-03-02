using System;
using ClientAPI.Publisher3rdp.Contracts;
using HeavyMetalMachines.Crossplay.DataTransferObjects;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Publishing
{
	public class PublisherGroupService : IPublisherGroupService
	{
		public PublisherGroupService(IPublisherGroup publisherGroupService)
		{
			this._publisherGroupService = publisherGroupService;
		}

		public IObservable<PublisherGroupData> OnInviteReceived
		{
			get
			{
				return Observable.Select<EventPattern<PublisherGroupEvent>, PublisherGroupData>(Observable.FromEventPattern<EventHandler<PublisherGroupEvent>, PublisherGroupEvent>((EventHandler<PublisherGroupEvent> h) => new EventHandler<PublisherGroupEvent>(h.Invoke), delegate(EventHandler<PublisherGroupEvent> h)
				{
					this._publisherGroupService.InviteReceived += h;
				}, delegate(EventHandler<PublisherGroupEvent> h)
				{
					this._publisherGroupService.InviteReceived -= h;
				}), (EventPattern<PublisherGroupEvent> e) => this.ConvertToModel(e.EventArgs));
			}
		}

		private PublisherGroupData ConvertToModel(PublisherGroupEvent publisherGroupEvent)
		{
			SerializableCrossplay serializableCrossplay = JsonSerializeable<SerializableCrossplay>.Deserialize(publisherGroupEvent.Bag);
			if (serializableCrossplay == null)
			{
				serializableCrossplay = new SerializableCrossplay
				{
					Enable = false,
					Publisher = string.Empty
				};
			}
			PublisherGroupData result = default(PublisherGroupData);
			result.GroupId = publisherGroupEvent.GroupId;
			result.IsOwnerCrossplayEnabled = serializableCrossplay.Enable;
			result.OwnerPublisher = Publishers.GetPublisherByName(serializableCrossplay.Publisher);
			return result;
		}

		private readonly IPublisherGroup _publisherGroupService;
	}
}
