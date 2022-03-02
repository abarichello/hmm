using System;
using HeavyMetalMachines.Options;

namespace HeavyMetalMachines.Swordfish
{
	public class ClientAudioBiLogger : IClientAudioBiLogger
	{
		public ClientAudioBiLogger(IClientBILogger logger)
		{
			this._logger = logger;
		}

		public void LogAudioOptions(AudioOptions audioOptions)
		{
			AudioBiLog audioBiLog = new AudioBiLog();
			audioBiLog.Master = this.ConvertFloatVolume(audioOptions.MasterVolume);
			audioBiLog.Music = this.ConvertFloatVolume(audioOptions.MusicVolume);
			audioBiLog.GameplaySfx = this.ConvertFloatVolume(audioOptions.SfxGameplayVolume);
			audioBiLog.AmbientSfx = this.ConvertFloatVolume(audioOptions.SfxGameplayVolume);
			audioBiLog.VoiceOver = this.ConvertFloatVolume(audioOptions.VoiceOverVolume);
			audioBiLog.VoiceChat = this.ConvertFloatVolume(audioOptions.VoiceChatVolume);
			audioBiLog.Announcer = this.ConvertFloatVolume(audioOptions.AnnouncerVolume);
			audioBiLog.AnnouncerId = audioOptions.AnnouncerIndex;
			this._logger.BILogClientMsg(89, audioBiLog.ToString(), false);
		}

		private int ConvertFloatVolume(float volume)
		{
			return (int)(volume * 100f);
		}

		private IClientBILogger _logger;
	}
}
