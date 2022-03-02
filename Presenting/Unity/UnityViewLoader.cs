using System;
using System.Collections;
using HeavyMetalMachines.Presenting.Exceptions;
using Hoplon.Logging;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Presenting.Unity
{
	public class UnityViewLoader : IViewLoader
	{
		public UnityViewLoader(ILogger<UnityViewLoader> logger)
		{
			this._logger = logger;
		}

		public IObservable<Unit> LoadView(string sceneName)
		{
			return Observable.FromCoroutine<Unit>((IObserver<Unit> observer, CancellationToken cancellationToken) => this.PerformSceneLoad(sceneName, observer));
		}

		public IObservable<Unit> UnloadView(string sceneName)
		{
			return Observable.FromCoroutine<Unit>((IObserver<Unit> observer, CancellationToken cancellationToken) => this.PerformSceneUnload(sceneName, observer));
		}

		private IEnumerator PerformSceneLoad(string sceneName, IObserver<Unit> observer)
		{
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, 1);
			if (!UnityViewLoader.SceneExists(asyncOperation))
			{
				observer.OnError(new Exception(string.Format("Could not load scene. The scene with name {0} does not exists.", sceneName)));
				yield break;
			}
			this._logger.InfoFormat("Load of scene {0} started.", new object[]
			{
				sceneName
			});
			yield return UnityViewLoader.ObserveAsyncOperation(asyncOperation, observer);
			this._logger.InfoFormat("Load of scene {0} finished.", new object[]
			{
				sceneName
			});
			yield break;
		}

		private IEnumerator PerformSceneUnload(string sceneName, IObserver<Unit> observer)
		{
			if (!SceneManager.GetSceneByName(sceneName).IsValid())
			{
				UnityViewLoader.UnloadSceneError(observer, sceneName);
				yield break;
			}
			AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneName);
			if (!UnityViewLoader.SceneExists(asyncOperation))
			{
				UnityViewLoader.UnloadSceneError(observer, sceneName);
				yield break;
			}
			this._logger.InfoFormat("Unload of scene {0} started.", new object[]
			{
				sceneName
			});
			yield return UnityViewLoader.ObserveAsyncOperation(asyncOperation, observer);
			this._logger.InfoFormat("Unload of scene {0} finished.", new object[]
			{
				sceneName
			});
			yield break;
		}

		private static void UnloadSceneError(IObserver<Unit> observer, string sceneName)
		{
			observer.OnError(new UnloadSceneException(string.Format("Could not unload scene. The scene with name {0} does not exists.", sceneName)));
		}

		private static IEnumerator ObserveAsyncOperation(AsyncOperation asyncOperation, IObserver<Unit> observer)
		{
			while (!asyncOperation.isDone)
			{
				yield return null;
			}
			observer.OnNext(Unit.Default);
			observer.OnCompleted();
			yield break;
		}

		private static bool SceneExists(AsyncOperation sceneOperation)
		{
			try
			{
				bool isDone = sceneOperation.isDone;
			}
			catch (NullReferenceException ex)
			{
				return false;
			}
			return true;
		}

		private readonly ILogger<UnityViewLoader> _logger;
	}
}
