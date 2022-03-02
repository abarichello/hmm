using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using UniRx;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class ObserveGroupInvitesCooldown : IObserveGroupInvitesCooldown
	{
		public ObserveGroupInvitesCooldown(GroupManager groupManager)
		{
			this._groupManager = groupManager;
		}

		public IObservable<Unit> Observe(IPlayer player)
		{
			return this.ObserveCooldown(player.UniversalId);
		}

		private IObservable<Unit> ObserveCooldown(string universalId)
		{
			return Observable.AsUnitObservable<long>(Observable.First<long>(Observable.EveryUpdate(), (long _) => !this._groupManager.IsUserBlockedToInvite(universalId)));
		}

		private readonly GroupManager _groupManager;
	}
}
