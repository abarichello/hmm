using System;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.ParentalControl
{
	public class DefaultUGCRestrictionDialogPresenter : IUGCRestrictionDialogPresenter
	{
		public IObservable<bool> ShowAndReturnIsRestricted()
		{
			return Observable.Select<Unit, bool>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit error)
			{
				Debug.Log("default parental control service ugc dialog error " + error);
			}), (Unit _) => false);
		}
	}
}
