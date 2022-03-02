using System;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Players.Business;
using HeavyMetaMachines.Audio;

namespace HeavyMetalMachines.Audio
{
	public class PlaySocialVoiceOver : IPlaySocialVoiceOver
	{
		public PlaySocialVoiceOver(IIsPlayerRestrictedByTextChat isPlayerRestrictedByTextChat, IPlayVoiceOver playVoiceOver)
		{
			this._isPlayerRestrictedByTextChat = isPlayerRestrictedByTextChat;
			this._playVoiceOver = playVoiceOver;
		}

		public void PlayMessage(VoiceOverEventGroup voiceOverEventGroup, MatchClient sourceClient)
		{
			if (!sourceClient.IsBot && this._isPlayerRestrictedByTextChat.IsPlayerRestricted(sourceClient.PlayerId))
			{
				return;
			}
			this._playVoiceOver.Play(voiceOverEventGroup, sourceClient);
		}

		private readonly IIsPlayerRestrictedByTextChat _isPlayerRestrictedByTextChat;

		private readonly IPlayVoiceOver _playVoiceOver;
	}
}
