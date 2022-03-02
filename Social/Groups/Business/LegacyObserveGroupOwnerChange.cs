using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using Assets.Standard_Assets.Scripts.HMM.Swordfish.Services;
using UniRx;

namespace HeavyMetalMachines.Social.Groups.Business
{
	public class LegacyObserveGroupOwnerChange : IObserverGroupOwnerChange
	{
		public LegacyObserveGroupOwnerChange(IGroupManager groupManager)
		{
			this._groupManager = groupManager;
		}

		public IObservable<Unit> Observe()
		{
			return Observable.AsUnitObservable<GroupEventArgs>(Observable.FromEvent<GroupEventArgs>(delegate(Action<GroupEventArgs> handler)
			{
				this._groupManager.EvtGroupOwnerChanged += handler;
			}, delegate(Action<GroupEventArgs> handler)
			{
				this._groupManager.EvtGroupOwnerChanged -= handler;
			}));
		}

		private readonly IGroupManager _groupManager;
	}
}
