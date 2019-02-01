using System;
using HeavyMetalMachines.Options;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class AnnouncerTextSelector : AnimatedTextSelector
	{
		protected override void Initialize()
		{
			this.selectionStrings = new string[GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers.Length];
			for (int i = 0; i < GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers.Length; i++)
			{
				this.selectionStrings[i] = GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers[i].draftName;
			}
			this.currentIndex = GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex;
			AudioOptions audio = GameHubBehaviour.Hub.Options.Audio;
			audio.OnAnnouncerIndexChanged = (Action)Delegate.Combine(audio.OnAnnouncerIndexChanged, new Action(this.OnOptionsAnnouncerIndexChanged));
		}

		private void OnOptionsAnnouncerIndexChanged()
		{
			if (GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex == this.currentIndex)
			{
				return;
			}
			this.currentIndex = GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex;
			base.UpdateLabels();
		}

		protected override void OnCurrentIndexChanged()
		{
			base.OnCurrentIndexChanged();
			GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex = this.currentIndex;
			GameHubBehaviour.Hub.Options.Audio.Apply();
		}

		private void OnDestroy()
		{
			AudioOptions audio = GameHubBehaviour.Hub.Options.Audio;
			audio.OnAnnouncerIndexChanged = (Action)Delegate.Remove(audio.OnAnnouncerIndexChanged, new Action(this.OnOptionsAnnouncerIndexChanged));
		}
	}
}
