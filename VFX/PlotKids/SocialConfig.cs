using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX.PlotKids
{
	public class SocialConfig : GameHubScriptableObject
	{
		public int SpamMessageCountThreshold
		{
			get
			{
				return this._spamMessageCountThreshold;
			}
		}

		public int SpamBlockedChatDuration
		{
			get
			{
				return this._spamBlockedChatDuration;
			}
		}

		[Header("Friend State Colors")]
		public Color OnlineColor;

		public Color OfflineColor;

		public Color AwayColor;

		public Color PlayingHmmColor;

		public Color PlayingHmmMatchColor;

		public Color PlayingAnotherGameColor;

		[Header("Friend Founder Sprites")]
		public Sprite FounderBronzeSprite;

		public Sprite FounderSilverSprite;

		public Sprite FounderGoldSprite;

		[Header("Friend Fallback Sprite")]
		public Texture2D FallbackPlayerIcon;

		[Header("Chat Spam Filter")]
		[SerializeField]
		[Tooltip("Allows X repeated messages.")]
		[Range(1f, 10f)]
		private int _spamMessageCountThreshold = 2;

		[SerializeField]
		private int _spamBlockedChatDuration = 5;
	}
}
