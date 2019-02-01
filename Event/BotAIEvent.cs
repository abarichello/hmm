using System;
using Pocketverse;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class BotAIEvent : PlayerEvent, IBitStreamSerializable
	{
		public override EventScopeKind GetKind()
		{
			return EventScopeKind.Bot;
		}
	}
}
