using System;
using HeavyMetalMachines.Orbis;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.ParentalControl
{
	public class OrbisChatRestrictionDialogPresenter : IChatRestrictionDialogPresenter
	{
		private OrbisPlatform OrbisPlatform
		{
			get
			{
				return Platform.Current as OrbisPlatform;
			}
		}

		public IObservable<Unit> Show()
		{
			return Observable.AsUnitObservable<long>(Observable.Do<long>(this.OrbisPlatform.ShowChatRestrictionDialog(), delegate(long error)
			{
				Debug.Log("orbis parental control service chat dialog error " + error);
			}));
		}
	}
}
