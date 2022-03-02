using System;
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
			GameHubBehaviour.Hub.Options.Audio.OnAnnouncerIndexChanged += this.OnOptionsAnnouncerIndexChanged;
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
			GameHubBehaviour.Hub.Options.Audio.OnAnnouncerIndexChanged -= this.OnOptionsAnnouncerIndexChanged;
		}
	}
}
