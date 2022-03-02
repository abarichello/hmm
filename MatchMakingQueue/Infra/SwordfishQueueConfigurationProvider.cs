using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.DataTransferObjects.QueueConfigurations;
using HeavyMetalMachines.Matchmaking.Configuration;
using HeavyMetalMachines.MatchMakingQueue.Configuration.Exceptions;
using HeavyMetalMachines.Swordfish;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public class SwordfishQueueConfigurationProvider : IQueueConfigurationProvider
	{
		public SwordfishQueueConfigurationProvider(IMatchmakingQueueConfig matchmakingQueueConfig, CollectionScriptableObject collectionScriptableObject, GameArenaConfig gameArenaConfig)
		{
			this._matchmakingQueueConfig = matchmakingQueueConfig;
			this._collectionScriptableObject = collectionScriptableObject;
			this._gameArenaConfig = gameArenaConfig;
		}

		public IObservable<QueueConfiguration> Get(string queueName, string regionName)
		{
			return Observable.Select<MatchmakingQueueConfig, QueueConfiguration>(Observable.Do<MatchmakingQueueConfig>(SwordfishObservable.FromSwordfishCall<MatchmakingQueueConfig>(delegate(SwordfishClientApi.ParameterizedCallback<MatchmakingQueueConfig> success, SwordfishClientApi.ErrorCallback error)
			{
				this._matchmakingQueueConfig.GetQueueConfigByQueueNameAndRegion(null, queueName, regionName, success, error);
			}), delegate(MatchmakingQueueConfig queueConfiguration)
			{
				this.AssertNotNullQueueConfiguration(queueConfiguration, queueName, regionName);
			}), new Func<MatchmakingQueueConfig, QueueConfiguration>(this.ConvertToQueueConfiguration));
		}

		private void AssertNotNullQueueConfiguration(MatchmakingQueueConfig queueConfiguration, string queueName, string regionName)
		{
			if (queueConfiguration == null)
			{
				throw new QueueConfigurationNotFoundException(queueName, regionName);
			}
		}

		private QueueConfiguration ConvertToQueueConfiguration(MatchmakingQueueConfig matchmakingQueueConfig)
		{
			MatchmakingQueueConfigBag matchmakingQueueConfigBag = (MatchmakingQueueConfigBag)((JsonSerializeable<!0>)matchmakingQueueConfig.Config);
			QueueConfiguration queueConfiguration = new QueueConfiguration();
			queueConfiguration.RegionName = matchmakingQueueConfig.Region;
			queueConfiguration.QueueName = matchmakingQueueConfig.QueueName;
			QueueConfiguration queueConfiguration2 = queueConfiguration;
			IEnumerable<MatchmakingQueuePeriod> periods = matchmakingQueueConfig.Periods;
			if (SwordfishQueueConfigurationProvider.<>f__mg$cache0 == null)
			{
				SwordfishQueueConfigurationProvider.<>f__mg$cache0 = new Func<MatchmakingQueuePeriod, QueuePeriod>(SwordfishQueueConfigurationProvider.ConvertToQueuePeriod);
			}
			queueConfiguration2.QueuePeriods = (from period in periods.Select(SwordfishQueueConfigurationProvider.<>f__mg$cache0)
			orderby period.OpenDateTimeUtc
			select period).ToArray<QueuePeriod>();
			queueConfiguration.LockedCharacters = matchmakingQueueConfigBag.LockedCharacters.Select(new Func<int, QueueConfigCharacterData>(this.CreateCharacterData)).ToArray<QueueConfigCharacterData>();
			queueConfiguration.AvailableArenas = matchmakingQueueConfigBag.AvailableArenas.Select(new Func<int, QueueConfigArenaData>(this.CreateArenaData)).ToArray<QueueConfigArenaData>();
			return queueConfiguration;
		}

		private QueueConfigCharacterData CreateCharacterData(int id)
		{
			ItemTypeComponent itemTypeComponent;
			this._collectionScriptableObject.AllCharactersByCharacterId[id].GetComponentByEnum(ItemTypeComponent.Type.Character, out itemTypeComponent);
			CharacterItemTypeComponent characterItemTypeComponent = itemTypeComponent as CharacterItemTypeComponent;
			string nameDraft = characterItemTypeComponent.NameDraft;
			return new QueueConfigCharacterData
			{
				Id = id,
				NameDraft = nameDraft
			};
		}

		private QueueConfigArenaData CreateArenaData(int id)
		{
			string arenaDraftName = this._gameArenaConfig.GetArenaDraftName(id);
			return new QueueConfigArenaData
			{
				Id = id,
				NameDraft = arenaDraftName
			};
		}

		private static QueuePeriod ConvertToQueuePeriod(MatchmakingQueuePeriod matchmakingQueuePeriod)
		{
			return new QueuePeriod
			{
				OpenDateTimeUtc = matchmakingQueuePeriod.Start,
				CloseDateTimeUtc = matchmakingQueuePeriod.End
			};
		}

		private readonly IMatchmakingQueueConfig _matchmakingQueueConfig;

		private readonly CollectionScriptableObject _collectionScriptableObject;

		private readonly GameArenaConfig _gameArenaConfig;

		[CompilerGenerated]
		private static Func<MatchmakingQueuePeriod, QueuePeriod> <>f__mg$cache0;
	}
}
