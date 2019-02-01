using System;
using HeavyMetalMachines.Options;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX.PlotKids.VoiceChat
{
	public class VoiceChatAudioSource : GameHubBehaviour
	{
		public string UniversalIdFrom
		{
			get
			{
				return this._universalIdFrom;
			}
		}

		private void OnEnable()
		{
			AudioOptions audio = GameHubBehaviour.Hub.Options.Audio;
			audio.OnMasterVolumeChanged = (Action<float>)Delegate.Combine(audio.OnMasterVolumeChanged, new Action<float>(this.OnVolumeChanged));
			AudioOptions audio2 = GameHubBehaviour.Hub.Options.Audio;
			audio2.OnVoiceChatVolumeChanged = (Action<float>)Delegate.Combine(audio2.OnVoiceChatVolumeChanged, new Action<float>(this.OnVolumeChanged));
			this._audioClip = AudioClip.Create(this._universalIdFrom, 22050, 1, 11025, true, new AudioClip.PCMReaderCallback(this.Pcmreadercallback));
			this._audioSource1.clip = this._audioClip;
		}

		private void OnVolumeChanged(float f)
		{
			this._audioSource1.volume = SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatVolume;
		}

		private void OnDisable()
		{
			UnityEngine.Object.Destroy(this._audioClip);
			AudioOptions audio = GameHubBehaviour.Hub.Options.Audio;
			audio.OnMasterVolumeChanged = (Action<float>)Delegate.Remove(audio.OnMasterVolumeChanged, new Action<float>(this.OnVolumeChanged));
			AudioOptions audio2 = GameHubBehaviour.Hub.Options.Audio;
			audio2.OnVoiceChatVolumeChanged = (Action<float>)Delegate.Remove(audio2.OnVoiceChatVolumeChanged, new Action<float>(this.OnVolumeChanged));
		}

		private void Update()
		{
			if (SingletonMonoBehaviour<VoiceChatController>.Instance.IsUserMuted(this._universalIdFrom))
			{
				this._isVoiceChatEnabled = false;
				return;
			}
			if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatTeamStatus == VoiceChatTeamStatus.Disable)
			{
				this._isVoiceChatEnabled = false;
				return;
			}
			this._isVoiceChatEnabled = true;
			if (this._enableAudioPlay)
			{
				if (!this._audioSource1.isPlaying)
				{
					this._audioSource1.Play();
					VoiceFeedBackGUI.Instance.OnVoiceReceived(this._universalIdFrom);
				}
			}
			else if (this._audioSource1.isPlaying)
			{
				this._audioSource1.Stop();
				VoiceFeedBackGUI.Instance.OnVoiceStopped(this._universalIdFrom);
			}
		}

		private void OnDestroy()
		{
			VoiceFeedBackGUI.Instance.OnVoiceStopped(this._universalIdFrom);
			this._audioSource1.Stop();
		}

		public bool IsSpeaking()
		{
			return this._audioSource1.isPlaying;
		}

		public void Init(string universalidfrom)
		{
			this._universalIdFrom = universalidfrom;
			this._audioSource1.volume = SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatVolume;
			base.gameObject.name = string.Format("{0}_{1}", base.gameObject.name, this._universalIdFrom);
			base.gameObject.gameObject.SetActive(true);
		}

		public void onVoiceReceived(string universalidfrom, byte[] audiodata, DateTime datesent, DateTime datereceived)
		{
			if (this._isVoiceChatEnabled)
			{
				this.WriteBuffer(audiodata);
			}
		}

		public void WriteBuffer(byte[] data)
		{
			object obj = this.bufferWriteMutex;
			lock (obj)
			{
				int num = 0;
				if (this.offset > 0)
				{
					num = this.offset;
					this.offset = 0;
				}
				int num2 = data.Length / 2 - num;
				if (num2 + this.bufferIndex > this.f.Length)
				{
					int num3 = num * 2;
					for (int i = this.bufferIndex; i < this.f.Length; i++)
					{
						this.f[this.bufferIndex] = (float)BitConverter.ToInt16(data, num3) * 3.05175781E-05f;
						num3 += 2;
						this.bufferIndex++;
					}
					this.bufferIndex = 0;
					for (int j = num3; j < data.Length; j += 2)
					{
						this.f[this.bufferIndex] = (float)BitConverter.ToInt16(data, j) * 3.05175781E-05f;
						this.bufferIndex++;
					}
				}
				else
				{
					for (int k = num * 2; k < data.Length; k += 2)
					{
						this.f[this.bufferIndex] = (float)BitConverter.ToInt16(data, k) * 3.05175781E-05f;
						this.bufferIndex++;
					}
				}
				if (this.bufferIndex >= this.f.Length)
				{
					this.bufferIndex = 0;
				}
				this._enableAudioPlay = true;
			}
		}

		private void Pcmreadercallback(float[] data)
		{
			object obj = this.bufferWriteMutex;
			lock (obj)
			{
				for (int i = 0; i < data.Length; i++)
				{
					if (this.readData == this.bufferIndex)
					{
						data[i] = 0f;
						this.offset++;
					}
					else
					{
						data[i] = this.f[this.readData];
						this.readData++;
						if (this.readData >= this.f.Length)
						{
							this.readData = 0;
						}
					}
				}
				if (this.offset > 11025)
				{
					this._enableAudioPlay = false;
					this.bufferIndex = 0;
					this.offset = 0;
					this.readData = 0;
				}
			}
		}

		private void onVoiceChatVolumeChange(float voiceChatVolume)
		{
			this._audioSource1.volume = voiceChatVolume;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(VoiceChatAudioSource));

		private const float bitConversion = 3.05175781E-05f;

		private string _universalIdFrom = string.Empty;

		[SerializeField]
		private AudioSource _audioSource1;

		private const int maxBufferSize = 60000;

		private float[] f = new float[60000];

		private int bufferIndex;

		private int readData;

		private int offset;

		private volatile bool _isVoiceChatEnabled = true;

		private volatile bool _enableAudioPlay;

		private AudioClip _audioClip;

		private object bufferWriteMutex = new object();
	}
}
