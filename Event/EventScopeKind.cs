using System;

namespace HeavyMetalMachines.Event
{
	public enum EventScopeKind
	{
		Unset,
		None,
		Player,
		Scene = 4,
		Effect,
		Announcer,
		Pickup,
		Bot,
		Ping
	}
}
