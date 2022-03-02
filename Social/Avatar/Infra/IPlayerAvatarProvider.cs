using System;
using UniRx;

namespace HeavyMetalMachines.Social.Avatar.Infra
{
	public interface IPlayerAvatarProvider
	{
		IObservable<Guid> GetAvatarItemIdFromPlayerId(long playerId);
	}
}
