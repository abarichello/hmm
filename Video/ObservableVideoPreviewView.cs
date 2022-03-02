using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Zenject;

namespace HeavyMetalMachines.Video
{
	[DisallowMultipleComponent]
	public class ObservableVideoPreviewView : MonoBehaviour, IVideoView
	{
		public ICanvasGroup Canvas
		{
			get
			{
				return this._canvas;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<IVideoView>(this, null);
			this._player = base.gameObject.AddComponent<VideoPlayer>();
			this._player.audioOutputMode = 0;
			this._player.renderMode = 4;
			this._player.isLooping = true;
			this._player.skipOnDrop = true;
			this._player.prepareCompleted += new VideoPlayer.EventHandler(this.OnPrepareComplete);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IVideoView>(null);
			this._player.prepareCompleted -= new VideoPlayer.EventHandler(this.OnPrepareComplete);
			Object.Destroy(this._player);
			this._player = null;
		}

		private void OnPrepareComplete(VideoPlayer source)
		{
			this.SetupTexture(source.texture);
		}

		private void SetupTexture(Texture texture)
		{
			if (texture)
			{
				this._image.enabled = true;
				this._image.texture = texture;
			}
			else
			{
				this._image.enabled = false;
			}
		}

		private void OnValidate()
		{
			this._image = base.GetComponent<RawImage>();
		}

		public void StopPlayer()
		{
			this._player.Stop();
		}

		public void StartPlayer(VideoClip clip)
		{
			this._player.clip = clip;
			this._player.isLooping = true;
			this._player.Play();
		}

		[SerializeField]
		private UnityCanvasGroup _canvas;

		[SerializeField]
		[HideInInspector]
		private RawImage _image;

		private VideoPlayer _player;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
