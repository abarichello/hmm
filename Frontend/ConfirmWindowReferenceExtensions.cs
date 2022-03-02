using System;
using Hoplon.Assertions;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public static class ConfirmWindowReferenceExtensions
	{
		public static IObservable<ConfirmWindowResult> OpenConfirmWindowAsync(this ConfirmWindowReference confirmWindowReference, ConfirmWindowProperties properties)
		{
			return Observable.Create<ConfirmWindowResult>(delegate(IObserver<ConfirmWindowResult> observer)
			{
				Assert.IsTrue(properties.OnOk == null, "Callback OnOk cannot be used when opening the confirm window asynchronously.");
				Assert.IsTrue(properties.OnTimeOut == null, "Callback OnTimeOut cannot be used when opening the confirm window asynchronously.");
				bool isDisposed = false;
				properties.Guid = Guid.NewGuid();
				properties.OnOk = delegate()
				{
					ConfirmWindowReferenceExtensions.CloseDialog(confirmWindowReference, properties.Guid, observer, isDisposed);
				};
				properties.OnTimeOut = delegate()
				{
					ConfirmWindowReferenceExtensions.CloseDialog(confirmWindowReference, properties.Guid, observer, isDisposed);
				};
				Debug.Log("Dialog shown");
				confirmWindowReference.OpenConfirmWindow(properties);
				return Disposable.Create(delegate()
				{
					confirmWindowReference.HideConfirmWindow(properties.Guid);
					isDisposed = true;
				});
			});
		}

		private static void CloseDialog(ConfirmWindowReference confirmWindowReference, Guid dialogGuid, IObserver<ConfirmWindowResult> observer, bool isDisposed)
		{
			if (isDisposed)
			{
				return;
			}
			confirmWindowReference.HideConfirmWindow(dialogGuid);
			observer.OnNext(new ConfirmWindowResult
			{
				CheckboxValue = confirmWindowReference.Checkbox.value
			});
			observer.OnCompleted();
		}
	}
}
