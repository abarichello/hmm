using System;
using HeavyMetalMachines.ExpirableStorage;
using Hoplon.Logging;
using Hoplon.Time;
using UniRx;

namespace HeavyMetalMachines.Social.Avatar.Infra
{
	public class AvatarStorageHandler : IAvatarStorageHandler
	{
		public AvatarStorageHandler(IAvatarStorage avatarStorage, ICurrentTime currentTime, IPlayerAvatarProvider playerAvatarProvider, ILogger<ExpirableStorageHandler<long, Guid>> logger)
		{
			this._expirableStorage = new ExpirableStorageHandler<long, Guid>(avatarStorage, new Func<long, IObservable<Guid>>(playerAvatarProvider.GetAvatarItemIdFromPlayerId), currentTime, logger);
		}

		public IObservable<Guid> GetValue(long playerId)
		{
			return this._expirableStorage.GetValue(playerId);
		}

		private readonly ExpirableStorageHandler<long, Guid> _expirableStorage;
	}
}
