using System;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Server;
using HeavyMetalMachines.Swordfish;
using Hoplon.Serialization;
using UniRx;
using UniRx.InternalUtil;

namespace HeavyMetalMachines.Storyteller
{
	public class StorytellerGameserverSearchService : IStorytellerGameserverSearchService
	{
		public IObservable<GameServerRunningInfo[]> SearchServer(MatchSearchBag search)
		{
			return Observable.Create<GameServerRunningInfo[]>(delegate(IObserver<GameServerRunningInfo[]> observer)
			{
				StorytellerCustomWS.GetMatches(null, search, delegate(object _, string res)
				{
					observer.OnNext(((GameServerInfoListBag)((JsonSerializeable<!0>)res)).Servers);
					observer.OnCompleted();
				}, delegate(object _, Exception error)
				{
					observer.OnError(error);
				});
				return Disposable.Create(delegate()
				{
					observer = EmptyObserver<GameServerRunningInfo[]>.Instance;
				});
			});
		}
	}
}
