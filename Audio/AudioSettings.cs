using System;
using FMod;
using HeavyMetalMachines.Audio.Music;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	public class AudioSettings : GameHubScriptableObject
	{
		public float GetVolume(Identifiable id)
		{
			if (id == null || id.IsOwner || GameHubScriptableObject.Hub == null || GameHubScriptableObject.Hub.Match == null || GameHubScriptableObject.Hub.Players.CurrentPlayerData == null)
			{
				return 1f;
			}
			PlayerData playerOrBotsByObjectId = GameHubScriptableObject.Hub.Players.GetPlayerOrBotsByObjectId(id.ObjId);
			if (playerOrBotsByObjectId.Team == GameHubScriptableObject.Hub.Players.CurrentPlayerTeam)
			{
				return this.AlliesVolume;
			}
			return this.EnemiesVolume;
		}

		[Range(0f, 1f)]
		[Tooltip("Percentage of HPMax where Massive Damage Audio is triggered")]
		public float MassiveDamageThreshold = 0.5f;

		[Range(0f, 1f)]
		[Tooltip("Percentage of HPMax where Near Death Audio is triggered")]
		public float NearDeathThreshold = 0.2f;

		[Tooltip("Time in seconds when acumulative damage for massive damage trigger is reset")]
		public float MassiveDamageCheckInterval = 1f;

		[Tooltip("Any damage bellow this thresholder will be ignored for audio purposes")]
		public float IgnoreAudioDamageThreshold = 10f;

		[Tooltip("Any repair bellow this thresholder will be ignored for audio purposes")]
		public float IgnoreAudioRepairThreshold = 9f;

		[Tooltip("Any impulse bellow this thresholder will be ignored for audio purposes")]
		public float IgnoreAudioImpulseThreshold = 20f;

		[Tooltip("Any voice over where cooldown is not configured in FMOD will be set to this value")]
		public int DefaultAudioCooldown = 5;

		[Tooltip("Any voice over where priority is not configured in FMOD will be set to this value")]
		public int DefaultAudioPriority = 10;

		[Tooltip("Any voice over where timeout is not configured in FMOD will be set to this value")]
		public float DefaultAudioTimeout = 100f;

		[Tooltip("Percentage where the enemy sfx and V.O. are listened")]
		public float EnemiesVolume = 0.95f;

		[Tooltip("Percentage where the allies sfx and V.O. are listened")]
		public float AlliesVolume = 0.85f;

		[Tooltip("How much time a engine will stay in it's state")]
		public float EngineStateHisteria = 0.2f;

		[Header("Snapshots")]
		public AudioEventAsset MuteSnapshot;

		public AudioEventAsset DeathSnapshot;

		public AudioEventAsset BombDeliverySnapshot;

		public AudioEventAsset MatchEndSnapshot;

		public AudioEventAsset loadingSnapshot;

		[Header("Banks")]
		public string[] SyncronousStartingBanks;

		public string[] AsyncronousStartingBanks;

		public string AsyncronousCounselorBankName;

		[Header("Music and Ambience")]
		public MusicAndAmbience Welcome;

		public MusicAndAmbience CreateProfile;

		public MusicAndAmbience MainMenu;

		public MusicAndAmbience CharacterPick;

		public MusicAndAmbience InGame;

		public MusicAndAmbience EndMatch;

		public MusicAndAmbience RankedDrafter;

		public MusicAndAmbience TournamentDrafter;

		[Header("Global SFXs")]
		public AudioEventAsset PickScreenCharacterEnterSFX;

		public CrowdConfiguration defaultCrowd;

		public AnnouncerVoiceOverInfo[] AnnouncerVoiceOvers;

		[EnumArray(typeof(VoiceOverEventGroup), typeof(FMODAudioManager.SourceTypes))]
		public FMODAudioManager.SourceTypes[] VoiceOverEventsConfig;
	}
}
