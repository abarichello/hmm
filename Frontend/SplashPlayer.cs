using System;
using System.Collections.Generic;
using FMod;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class SplashPlayer : GameHubBehaviour
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSplashPlayer) && this.VideoTexture != null)
			{
				this.VideoTexture.enabled = false;
				base.enabled = false;
				return;
			}
			for (int i = 0; i < this.Videos.Count; i++)
			{
				GameHubBehaviour gameHubBehaviour = this.Videos[i];
			}
		}

		private void OnOggPlayerError(string error)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get(error, "Help"),
				OkButtonText = Language.Get("Ok", "GUI"),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					this.FinishSplashes();
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSplashPlayer) && this.VideoTexture != null)
			{
				return;
			}
			for (int i = 0; i < this.Videos.Count; i++)
			{
				GameHubBehaviour gameHubBehaviour = this.Videos[i];
			}
			if (this.muteSnapshotAudio != null && !this.muteSnapshotAudio.IsInvalidated())
			{
				this.muteSnapshotAudio.Stop();
			}
		}

		public void PlaySplashes(Action splashesEndedCallback)
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSplashPlayer))
			{
				splashesEndedCallback();
				return;
			}
			this._splashesEndedCallback = splashesEndedCallback;
			this.Rewind();
			if (this.muteSnapshotAudio != null)
			{
				this.muteSnapshotAudio.Stop();
			}
			this.muteSnapshotAudio = FMODAudioManager.PlayAt(GameHubBehaviour.Hub.AudioSettings.MuteSnapshot, base.transform);
		}

		public void Rewind()
		{
			this._finishedSplashes = false;
			this._splashIndex = -1;
			this.PlayNextSplash();
		}

		public void TrySkipCurrentSplash()
		{
			if (!this._finishedSplashes && this._splashState != SplashPlayer.SplashState.Stopping)
			{
				this._splashState = SplashPlayer.SplashState.Stopping;
			}
		}

		public void SetVideoFileName(string fileName, int targetIndex)
		{
			if (this.Videos.Count < targetIndex)
			{
				SplashPlayer.Log.ErrorFormat("Error trying to set video file name. Wrong index. Videos Count:{0}, TargetIndex:{1}", new object[]
				{
					this.Videos.Count,
					targetIndex
				});
				return;
			}
		}

		public void SetVideoTexture(UITexture poNewVideoTexture)
		{
			for (int i = 0; i < this.Videos.Count; i++)
			{
				GameHubBehaviour gameHubBehaviour = this.Videos[i];
			}
			this.VideoTexture = poNewVideoTexture;
		}

		private void PlayNextSplash()
		{
			this._splashIndex++;
			if (this._splashIndex >= this.Videos.Count)
			{
				this.FinishSplashes();
				return;
			}
			this._splashState = SplashPlayer.SplashState.Starting;
			this._alpha = 0f;
		}

		private void FinishSplashes()
		{
			if (!this._finishedSplashes)
			{
				this._splashState = SplashPlayer.SplashState.FinishedAll;
				this._finishedSplashes = true;
				if (this._splashesEndedCallback != null)
				{
					this._splashesEndedCallback();
				}
				if (this.muteSnapshotAudio != null && !this.muteSnapshotAudio.IsInvalidated())
				{
					this.muteSnapshotAudio.Stop();
				}
			}
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() || GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSplashPlayer))
			{
				return;
			}
			if (this._skipable && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.JoystickButton7)))
			{
				this.TrySkipCurrentSplash();
			}
			switch (this._splashState)
			{
			case SplashPlayer.SplashState.Starting:
				if (this._fadeInSeconds > 0f)
				{
					this._alpha = Mathf.Min(1f, this._alpha + Time.deltaTime * 1f / this._fadeInSeconds);
				}
				if (this._fadeInSeconds <= 0f || this._alpha >= 1f)
				{
					this._splashState = SplashPlayer.SplashState.Playing;
				}
				break;
			case SplashPlayer.SplashState.Playing:
				if (this._splashIndex < 0)
				{
					this.PlayNextSplash();
				}
				break;
			case SplashPlayer.SplashState.Stopping:
				if (this._fadeOutSeconds > 0f)
				{
					this._alpha = Mathf.Max(0f, this._alpha - Time.deltaTime * 1f / this._fadeOutSeconds);
				}
				if (this._fadeOutSeconds <= 0f || this._alpha <= 0f)
				{
					if (this._splashIndex < 0 || this._splashIndex < this.Videos.Count)
					{
					}
					this.PlayNextSplash();
				}
				break;
			}
		}

		private bool IsLastVideo()
		{
			return this._splashIndex == this.Videos.Count - 1;
		}

		public bool HasFinished()
		{
			return GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSplashPlayer) || this._splashState == SplashPlayer.SplashState.FinishedAll;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SplashPlayer));

		[SerializeField]
		private UITexture VideoTexture;

		[SerializeField]
		private List<GameHubBehaviour> Videos = new List<GameHubBehaviour>(2);

		[SerializeField]
		private bool _skipable = true;

		[SerializeField]
		private float _fadeInSeconds = 0.2f;

		[SerializeField]
		private float _fadeOutSeconds = 0.5f;

		private SplashPlayer.SplashState _splashState;

		private Action _splashesEndedCallback;

		private int _splashIndex = -1;

		private bool _finishedSplashes;

		private FMODAudioManager.FMODAudio muteSnapshotAudio;

		private float _alpha;

		private enum SplashState
		{
			Starting,
			Playing,
			Stopping,
			FinishedAll
		}
	}
}
