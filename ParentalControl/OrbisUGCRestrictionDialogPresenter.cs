using System;
using HeavyMetalMachines.Orbis;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.ParentalControl
{
	public class OrbisUGCRestrictionDialogPresenter : IUGCRestrictionDialogPresenter
	{
		private OrbisPlatform OrbisPlatform
		{
			get
			{
				return Platform.Current as OrbisPlatform;
			}
		}

		public IObservable<bool> ShowAndReturnIsRestricted()
		{
			return Observable.Select<long, bool>(Observable.Do<long>(this.OrbisPlatform.ShowUGCRestrictionDialog(), delegate(long error)
			{
				Debug.Log("orbis parental control service chat dialog error " + error);
			}), (long _) => true);
		}
	}
}
