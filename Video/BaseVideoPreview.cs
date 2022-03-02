using System;
using System.Collections;
using System.Diagnostics;
using Hoplon.Unity.Loading;
using Pocketverse;
using SharedUtils.Loading;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

namespace HeavyMetalMachines.Video
{
	public abstract class BaseVideoPreview : BaseMonoBehaviour, IDynamicAssetListener<Object>, IEnumerator
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BaseVideoPreview.OnVideoLoadedDelegate OnVideoLoaded;

		protected virtual void Awake()
		{
			this._player = base.gameObject.AddComponent<VideoPlayer>();
			this._player.audioOutputMode = 0;
			this._player.renderMode = 4;
			this._player.isLooping = true;
			this._player.skipOnDrop = true;
			this._player.prepareCompleted += new VideoPlayer.EventHandler(this.OnPrepareComplete);
			BaseVideoPreview.Log.Info(string.Format("VideoPlayer created {0}", this._player.GetInstanceID()));
		}

		protected virtual void OnDestroy()
		{
			this._player.prepareCompleted -= new VideoPlayer.EventHandler(this.OnPrepareComplete);
			int instanceID = this._player.GetInstanceID();
			BaseVideoPreview.Log.Info(string.Format("VideoPlayer destroying {0}", instanceID));
			Object.Destroy(this._player);
			this._player = null;
			BaseVideoPreview.Log.Info(string.Format("VideoPlayer destroyed {0}", instanceID));
		}

		protected virtual void OnEnable()
		{
			if (this._player.isPrepared)
			{
				this._player.Play();
			}
		}

		protected virtual void OnDisable()
		{
			if (this._player)
			{
				this._player.Stop();
			}
			base.StopAllCoroutines();
			this._currentSwap = null;
			this._nextClip = null;
		}

		private void OnPrepareComplete(VideoPlayer source)
		{
			if (this._player.isLooping)
			{
				this.SetupTexture(source.texture);
			}
		}

		public void Stop()
		{
			if (this._player)
			{
				this._player.isLooping = false;
			}
			this._currentClipName = string.Empty;
		}

		public string VideoClipName
		{
			set
			{
				if (string.Equals(value, this._currentClipName))
				{
					return;
				}
				this._currentClipName = value;
				this._swapCooldown = 1f;
				this.SetupTexture(null);
				if (!Loading.GenericAssetManager.GetAssetAsync(value, this))
				{
					BaseVideoPreview.Log.Error(string.Format("VideoClip not found: {0}", value));
				}
			}
		}

		public IObservable<Unit> LoadVideo(string clipName)
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (string.IsNullOrEmpty(clipName))
				{
					BaseVideoPreview.Log.Warn("Asset name is null. Ignoring.");
					return Observable.ReturnUnit();
				}
				if (string.Equals(clipName, this._currentClipName))
				{
					return Observable.ReturnUnit();
				}
				this._currentClipName = clipName;
				this._swapCooldown = 1f;
				this.SetupTexture(null);
				this._loadingObservation = new Subject<Unit>();
				return Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					this.ListenToLoading(),
					this.CallLoading()
				});
			});
		}

		private IObservable<Unit> CallLoading()
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (!Loading.GenericAssetManager.GetAssetAsync(this._currentClipName, this))
				{
					BaseVideoPreview.Log.Error(string.Format("VideoClip not found: {0}", this._currentClipName));
				}
				return Observable.ReturnUnit();
			});
		}

		private IObservable<Unit> ListenToLoading()
		{
			return this._loadingObservation;
		}

		private IEnumerator VideoClipSwapCoroutine()
		{
			this._player.Stop();
			yield return this;
			this.InvokeOnVideoLoaded();
			this._player.clip = this._nextClip;
			this._player.isLooping = true;
			this._player.Play();
			this._nextClip = null;
			this._currentSwap = null;
			this._swapCooldown = 1f;
			yield break;
		}

		protected abstract void SetupTexture(Texture texture);

		protected virtual void InvokeOnVideoLoaded()
		{
			if (this.OnVideoLoaded != null)
			{
				this.OnVideoLoaded();
			}
			this.InvokeLoadActionEnded();
		}

		private void InvokeLoadActionEnded()
		{
			if (this._loadingObservation != null)
			{
				this._loadingObservation.OnCompleted();
				this._loadingObservation = null;
			}
		}

		public void OnAssetLoaded(string assetName, Object asset)
		{
			if (asset == null)
			{
				this.InvokeLoadActionEnded();
				return;
			}
			if (!string.Equals(assetName, this._currentClipName))
			{
				this.InvokeLoadActionEnded();
				return;
			}
			VideoClip videoClip = asset as VideoClip;
			if (videoClip == null)
			{
				this.InvokeLoadActionEnded();
				return;
			}
			this._nextClip = videoClip;
			if (this._currentSwap == null)
			{
				this._currentSwap = base.StartCoroutine(this.VideoClipSwapCoroutine());
			}
		}

		public bool MoveNext()
		{
			this._swapCooldown -= Time.deltaTime;
			return this._swapCooldown > 0f;
		}

		public void Reset()
		{
		}

		public object Current
		{
			get
			{
				return null;
			}
		}

		private const float VideoSwapCooldownValue = 1f;

		protected static readonly BitLogger Log = new BitLogger(typeof(BaseVideoPreview));

		[NonSerialized]
		private string _currentClipName;

		[NonSerialized]
		protected VideoPlayer _player;

		[NonSerialized]
		private Coroutine _currentSwap;

		[NonSerialized]
		private VideoClip _nextClip;

		[NonSerialized]
		private float _swapCooldown;

		private Subject<Unit> _loadingObservation;

		public delegate void OnVideoLoadedDelegate();
	}
}
