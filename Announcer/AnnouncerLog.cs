using System;
using HeavyMetalMachines.Localization;

namespace HeavyMetalMachines.Announcer
{
	[Serializable]
	public class AnnouncerLog
	{
		public string LocalizedText
		{
			get
			{
				return (!string.IsNullOrEmpty(this.Name)) ? Language.Get(this.Name, TranslationContext.Announcer) : string.Empty;
			}
		}

		public string Name;

		public AnnouncerLog.AnnouncerEventKinds AnnouncerEventKind;

		[ScriptId]
		public int Index;

		public AudioEventAsset Audio;

		public bool clientOnlyAudio;

		public AnnouncerLog.AnnouncerEventKinds TopAnnouncerEventKind;

		public AnnouncerLog.AnnouncerEventKinds LogAnnouncerEventKind;

		public bool Exclusive;

		public enum AnnouncerEventKinds
		{
			None,
			PlayerReconnected,
			PlayerDisconnected,
			FirstBlood,
			DoubleKill,
			TripleKill,
			QuadKill,
			UltraKill,
			BluVictory,
			RedVictory,
			BluWipe,
			RedWipe,
			KillStreak03,
			KillStreak06,
			KillStreak09,
			KillStreak12,
			KillStreak15,
			BeginningWarmup,
			Beginning30,
			Beginning10,
			Beginning05,
			Beginning00,
			PlayerKilledByPlayer,
			PlayerKilledByPlayerWithAssists,
			PlayerKilledByEnvironment,
			PlayerKilledByEnvironmentWithAssists,
			PlayerEndedAKillStreak,
			PlayerEndedAKillStreakWithAssists,
			AFKGeneric,
			KillNemesisDomination,
			KillNemesisRevenge,
			BombDelivery,
			LeaverGeneric,
			AFKEnd,
			AFKWarning,
			BotControllerActivated,
			BotControllerDeactivated,
			LeaverModifierWarning,
			BombPicked,
			BombPickedNearGoal,
			BombShootingNearGoal,
			SpectatorConnected
		}
	}
}
