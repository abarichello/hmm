using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using ClientAPI.Objects;
using Pocketverse;
using UniRx;
using UniRx.InternalUtil;

namespace HeavyMetalMachines.Storyteller
{
	public class StorytellerMatchConnectionService : GameHubObject, IStorytellerMatchConnectionService
	{
		public IObservable<bool> ConnectToServer(GameServerRunningInfo info)
		{
			StorytellerMatchConnectionService.<ConnectToServer>c__AnonStorey0 <ConnectToServer>c__AnonStorey = new StorytellerMatchConnectionService.<ConnectToServer>c__AnonStorey0();
			<ConnectToServer>c__AnonStorey.info = info;
			<ConnectToServer>c__AnonStorey.$this = this;
			return Observable.Create<bool>(delegate(IObserver<bool> observer)
			{
				StorytellerMatchConnectionService.Log.DebugFormat("ConnectToMatchServer IP: {0} Port: {1} Bag: {2}", new object[]
				{
					<ConnectToServer>c__AnonStorey.info.Ip,
					<ConnectToServer>c__AnonStorey.info.Port,
					<ConnectToServer>c__AnonStorey.info.GameServerStatus
				});
				if (<ConnectToServer>c__AnonStorey.info.Port == null)
				{
					StorytellerMatchConnectionService.Log.ErrorFormat("Cannot connect to server={0}, server port not set status={1}", new object[]
					{
						<ConnectToServer>c__AnonStorey.info.Ip,
						<ConnectToServer>c__AnonStorey.info.GameServerStatus
					});
					observer.OnCompleted();
					return Disposable.Create(delegate()
					{
						observer = EmptyObserver<bool>.Instance;
					});
				}
				GameHubObject.Hub.Server.ServerIp = <ConnectToServer>c__AnonStorey.info.Ip;
				GameHubObject.Hub.Server.ServerPort = <ConnectToServer>c__AnonStorey.info.Port.Value;
				SingletonMonoBehaviour<SpectatorController>.Instance.CurrentSpectatorRole = SpectatorRole.Spectator;
				GameHubObject.Hub.User.ConnectNarratorToServer(true, delegate
				{
					StorytellerMatchConnectionService.Log.Debug("Connection failed");
					GameHubObject.Hub.State.GotoState(GameHubObject.Hub.State.Current, false);
					SingletonMonoBehaviour<SpectatorController>.Instance.CurrentSpectatorRole = SpectatorRole.None;
					<ConnectToServer>c__AnonStorey.CompleteConnectionAction(observer, false);
				}, delegate
				{
					StorytellerMatchConnectionService.Log.Debug("Connection succesful");
					<ConnectToServer>c__AnonStorey.CompleteConnectionAction(observer, true);
				});
				return Disposable.Create(delegate()
				{
					observer = EmptyObserver<bool>.Instance;
				});
			});
		}

		private void CompleteConnectionAction(IObserver<bool> observer, bool nextValue)
		{
			observer.OnNext(nextValue);
			observer.OnCompleted();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(StorytellerMatchConnectionService));
	}
}
