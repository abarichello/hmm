using System;
using Hoplon.Serialization;

namespace HeavyMetalMachines.Chat
{
	public class ChatMessageBag : JsonSerializeable<ChatMessageBag>
	{
		public int PublisherId { get; set; }

		public string PublisherUserName { get; set; }
	}
}
