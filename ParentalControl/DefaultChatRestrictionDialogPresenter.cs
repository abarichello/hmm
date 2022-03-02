using System;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.ParentalControl
{
	public class DefaultChatRestrictionDialogPresenter : IChatRestrictionDialogPresenter
	{
		public IObservable<Unit> Show()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit error)
			{
				Debug.Log("default parental control service chat dialog error " + error);
			});
		}
	}
}
