using System;
using System.Collections;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Infra.AsyncOperation
{
	public static class AsyncOperationExtensions
	{
		public static IObservable<float> ToObservable(this AsyncOperation asyncOperation)
		{
			if (asyncOperation == null)
			{
				throw new ArgumentNullException("asyncOperation");
			}
			return Observable.FromCoroutine<float>((IObserver<float> observer, CancellationToken cancellationToken) => AsyncOperationExtensions.RunAsyncOperation(asyncOperation, observer, cancellationToken));
		}

		private static IEnumerator RunAsyncOperation(AsyncOperation asyncOperation, IObserver<float> observer, CancellationToken cancellationToken)
		{
			while (!asyncOperation.isDone && !cancellationToken.IsCancellationRequested)
			{
				observer.OnNext(asyncOperation.progress);
				yield return null;
			}
			if (!cancellationToken.IsCancellationRequested)
			{
				observer.OnNext(asyncOperation.progress);
				observer.OnCompleted();
			}
			yield break;
		}
	}
}
