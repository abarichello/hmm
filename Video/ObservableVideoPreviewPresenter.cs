using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Logging;
using Hoplon.Unity.Loading;
using SharedUtils.Loading;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

namespace HeavyMetalMachines.Video
{
	public class ObservableVideoPreviewPresenter : IDynamicAssetListener<Object>
	{
		public ObservableVideoPreviewPresenter(IViewProvider viewProvider, ILogger<ObservableVideoPreviewPresenter> logger)
		{
			this._viewProvider = viewProvider;
			this._logger = logger;
		}

		public void Initialize()
		{
			this._view = this._viewProvider.Provide<IVideoView>(null);
			this.Hide();
		}

		public IObservable<Unit> LoadVideo(string clipName)
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (string.IsNullOrEmpty(clipName))
				{
					return Observable.ReturnUnit();
				}
				if (string.Equals(clipName, this._currentClipName))
				{
					this._view.Canvas.Alpha = 1f;
					return Observable.ReturnUnit();
				}
				this._currentClipName = clipName;
				this._loadingObservation = new Subject<Unit>();
				return Observable.Do<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					this.ListenToLoading(),
					this.CallLoading()
				}), delegate(Unit _)
				{
					this._view.Canvas.Alpha = 1f;
				});
			});
		}

		public void Hide()
		{
			this._view.Canvas.Alpha = 0f;
		}

		private IObservable<Unit> ListenToLoading()
		{
			return this._loadingObservation;
		}

		private IObservable<Unit> CallLoading()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.StopPlayer();
				if (!Loading.GenericAssetManager.GetAssetAsync(this._currentClipName, this))
				{
					this._logger.Error(string.Format("VideoClip not found: {0}", this._currentClipName));
				}
				return Observable.ReturnUnit();
			});
		}

		public void OnAssetLoaded(string assetName, Object asset)
		{
			if (asset == null)
			{
				this._loadingObservation.OnCompleted();
				return;
			}
			if (!string.Equals(assetName, this._currentClipName))
			{
				this._loadingObservation.OnCompleted();
				return;
			}
			VideoClip videoClip = asset as VideoClip;
			if (videoClip == null)
			{
				this._loadingObservation.OnCompleted();
				return;
			}
			this._view.StartPlayer(videoClip);
			this._loadingObservation.OnCompleted();
		}

		private string _currentClipName;

		private IViewProvider _viewProvider;

		private readonly ILogger<ObservableVideoPreviewPresenter> _logger;

		private IVideoView _view;

		private Subject<Unit> _loadingObservation;
	}
}
