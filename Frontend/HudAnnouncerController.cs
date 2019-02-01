using System;
using System.Collections;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudAnnouncerController : GameHubBehaviour
	{
		private void Start()
		{
			this.Reset();
			GameHubBehaviour.Hub.Announcer.ListenToEvent += this.OnHudAnnounceTriggered;
			this.waitForDisconnectionMsgTime = new WaitForSeconds(this.playerDisconnectionMsgTime);
			this.waitForWipeMsgTime = new WaitForSeconds(this.wipeMsgTime);
			this.waitForKillPlayerMsgTime = new WaitForSeconds(this.killPlayerMsgTime);
			this.waitForFirstBloodMsgTime = new WaitForSeconds(this.firstBloodMsgTime);
			this.waitForKillingSpreeMsgTime = new WaitForSeconds(this.killingSpreeMsgTime);
			this.waitForKillStreakMsgTime = new WaitForSeconds(this.killStreakMsgTime);
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.Announcer.ListenToEvent -= this.OnHudAnnounceTriggered;
		}

		private void OnHudAnnounceTriggered(AnnouncerManager.QueuedAnnouncerLog announce)
		{
			if (GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.IsWindowVisible())
			{
				return;
			}
			AnnouncerEvent announcerEvent = announce.AnnouncerEvent;
			AnnouncerLog announcerLog = announce.AnnouncerLog;
			switch (announcerLog.TopAnnouncerEventKind)
			{
			case AnnouncerLog.AnnouncerEventKinds.PlayerReconnected:
			case AnnouncerLog.AnnouncerEventKinds.PlayerDisconnected:
				GameHubBehaviour.Hub.Announcer.IsHudBusy = true;
				base.StartCoroutine(this.ShowPlayerDisconnectedAnnounce(announcerEvent, announcerLog));
				break;
			case AnnouncerLog.AnnouncerEventKinds.FirstBlood:
				GameHubBehaviour.Hub.Announcer.IsHudBusy = true;
				base.StartCoroutine(this.ShowFirstBloodAnnounce(announcerEvent, announcerLog));
				break;
			case AnnouncerLog.AnnouncerEventKinds.DoubleKill:
			case AnnouncerLog.AnnouncerEventKinds.TripleKill:
			case AnnouncerLog.AnnouncerEventKinds.QuadKill:
			case AnnouncerLog.AnnouncerEventKinds.UltraKill:
				GameHubBehaviour.Hub.Announcer.IsHudBusy = true;
				base.StartCoroutine(this.ShowKillingSpreeAnnounce(announcerEvent, announcerLog));
				break;
			case AnnouncerLog.AnnouncerEventKinds.BluWipe:
			case AnnouncerLog.AnnouncerEventKinds.RedWipe:
				GameHubBehaviour.Hub.Announcer.IsHudBusy = true;
				base.StartCoroutine(this.ShowWipeAnnounce(announcerEvent, announcerLog));
				break;
			case AnnouncerLog.AnnouncerEventKinds.KillStreak03:
			case AnnouncerLog.AnnouncerEventKinds.KillStreak06:
			case AnnouncerLog.AnnouncerEventKinds.KillStreak09:
			case AnnouncerLog.AnnouncerEventKinds.KillStreak12:
			case AnnouncerLog.AnnouncerEventKinds.KillStreak15:
			case AnnouncerLog.AnnouncerEventKinds.PlayerEndedAKillStreak:
			case AnnouncerLog.AnnouncerEventKinds.PlayerEndedAKillStreakWithAssists:
				GameHubBehaviour.Hub.Announcer.IsHudBusy = true;
				base.StartCoroutine(this.ShowKillStreakAnnounce(announcerEvent, announcerLog));
				break;
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayer:
			case AnnouncerLog.AnnouncerEventKinds.PlayerKilledByPlayerWithAssists:
				GameHubBehaviour.Hub.Announcer.IsHudBusy = true;
				base.StartCoroutine(this.ShowKillByPlayerAnnounce(announcerEvent, announcerLog));
				break;
			}
		}

		private IEnumerator ShowPlayerDisconnectedAnnounce(AnnouncerEvent announcerEvent, AnnouncerLog announce)
		{
			this.KillbyPlayerLabelAnnounce.text = string.Format(announce.LocalizedText, GameHubBehaviour.Hub.Announcer.GetColoredPlayerName(announcerEvent.Killer));
			this.KillbyPlayerAnnounceKillerIcon.spriteName = "HudStatsThumbTexture";
			this.KillbyPlayerAnnounceKillerColor.color = GameHubBehaviour.Hub.Announcer.GetColorByPlayerId(announcerEvent.Killer);
			this.KillbyPlayerAnnounceVictimGroup.SetActive(false);
			this.KillbyPlayerAnnounceKillerDisconnectedIcon.SetActive(announce.TopAnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.PlayerDisconnected);
			this.KillbyPlayerAnnouncePanel.alpha = 1f;
			this.KillbyPlayer_Background.fillAmount = 0f;
			while (this.KillbyPlayer_Background.fillAmount < 1f)
			{
				this.KillbyPlayer_Background.fillAmount += Time.deltaTime * 2f;
				yield return null;
			}
			yield return this.waitForDisconnectionMsgTime;
			bool coroutineCanceled = false;
			this.KillbyPlayerAnnouncePanel.alpha = 0.99f;
			while (this.KillbyPlayerAnnouncePanel.alpha > 0f)
			{
				if (this.KillbyPlayerAnnouncePanel.alpha >= 1f)
				{
					coroutineCanceled = true;
					break;
				}
				this.KillbyPlayerAnnouncePanel.alpha -= Time.deltaTime * 2f;
				yield return null;
			}
			if (!coroutineCanceled)
			{
				this.KillbyPlayerAnnounceKillerDisconnectedIcon.SetActive(announce.TopAnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.PlayerDisconnected);
			}
			GameHubBehaviour.Hub.Announcer.IsHudBusy = false;
			yield break;
		}

		private IEnumerator ShowWipeAnnounce(AnnouncerEvent announcerEvent, AnnouncerLog announce)
		{
			this.TeamLabel.text = string.Format(announce.LocalizedText, GameHubBehaviour.Hub.Announcer.GetColoredVictimTeamName(announce.TopAnnouncerEventKind));
			this.TeamIcon.spriteName = "2_skull_icon";
			this.TeamBackground.fillAmount = 0f;
			this.TeamPanel.alpha = 1f;
			while (this.TeamBackground.fillAmount < 1f)
			{
				this.TeamBackground.fillAmount += Time.deltaTime * 2f;
				yield return null;
			}
			yield return this.waitForWipeMsgTime;
			this.TeamPanel.alpha = 0.99f;
			while (this.TeamPanel.alpha > 0f && this.TeamPanel.alpha < 1f)
			{
				this.TeamPanel.alpha -= Time.deltaTime * 2f;
				yield return null;
			}
			GameHubBehaviour.Hub.Announcer.IsHudBusy = false;
			yield break;
		}

		private IEnumerator ShowKillByPlayerAnnounce(AnnouncerEvent announcerEvent, AnnouncerLog announce)
		{
			this.KillbyPlayerLabelAnnounce.text = string.Format(Language.Get("PLAYERKILLED_BYPLAYER_TITLE", TranslationSheets.Announcer), GameHubBehaviour.Hub.Announcer.GetColoredPlayerName(announcerEvent.Killer), GameHubBehaviour.Hub.Announcer.GetColoredPlayerName(announcerEvent.Victim));
			this.KillbyPlayerAnnounceKillerColor.color = GameHubBehaviour.Hub.Announcer.GetColorByPlayerId(announcerEvent.Killer);
			this.KillbyPlayerAnnounceVictimColor.color = GameHubBehaviour.Hub.Announcer.GetColorByPlayerId(announcerEvent.Victim);
			this.KillbyPlayerAnnounceKillerIcon.spriteName = "HudStatsThumbTexture";
			this.KillbyPlayerAnnounceVictimIcon.spriteName = "HudStatsThumbTexture";
			this.KillbyPlayerAnnounceVictimGroup.SetActive(true);
			this.KillbyPlayerAnnounceKillerDisconnectedIcon.SetActive(false);
			this.KillbyPlayerAnnouncePanel.alpha = 1f;
			this.KillbyPlayer_Background.fillAmount = 0f;
			while (this.KillbyPlayer_Background.fillAmount < 1f)
			{
				this.KillbyPlayer_Background.fillAmount += Time.deltaTime * 2f;
				yield return null;
			}
			yield return this.waitForKillPlayerMsgTime;
			this.KillbyPlayerAnnouncePanel.alpha = 0.99f;
			while (this.KillbyPlayerAnnouncePanel.alpha > 0f && this.KillbyPlayerAnnouncePanel.alpha < 1f)
			{
				this.KillbyPlayerAnnouncePanel.alpha -= Time.deltaTime * 2f;
				yield return null;
			}
			GameHubBehaviour.Hub.Announcer.IsHudBusy = false;
			yield break;
		}

		private IEnumerator ShowFirstBloodAnnounce(AnnouncerEvent announcerEvent, AnnouncerLog announce)
		{
			this.KillSpreeLabelAnnounce.text = string.Format(announce.LocalizedText, GameHubBehaviour.Hub.Announcer.GetColoredPlayerName(announcerEvent.Killer));
			this.KillSpreeTitleAnnounce.text = Language.Get("FIRSTBLOOD_TITLE", TranslationSheets.Announcer);
			this.KillSpreeAnnounceKillerColor.color = GameHubBehaviour.Hub.Announcer.GetColorByPlayerId(announcerEvent.Killer);
			this.KillSpreeAnnounceKillerIcon.spriteName = "HudStatsThumbTexture";
			this.KillSpreeAnnouncePanel.alpha = 1f;
			this.KillSpree_Background.fillAmount = 0f;
			while (this.KillSpree_Background.fillAmount < 1f)
			{
				this.KillSpree_Background.fillAmount += Time.deltaTime * 2f;
				yield return null;
			}
			yield return this.waitForFirstBloodMsgTime;
			this.KillSpreeAnnouncePanel.alpha = 0.99f;
			while (this.KillSpreeAnnouncePanel.alpha > 0f && this.KillSpreeAnnouncePanel.alpha < 1f)
			{
				this.KillSpreeAnnouncePanel.alpha -= Time.deltaTime * 2f;
				yield return null;
			}
			GameHubBehaviour.Hub.Announcer.IsHudBusy = false;
			yield break;
		}

		private IEnumerator ShowKillingSpreeAnnounce(AnnouncerEvent announcerEvent, AnnouncerLog announce)
		{
			Animation anim = null;
			switch (announcerEvent.AnnouncerEventKind)
			{
			case AnnouncerLog.AnnouncerEventKinds.DoubleKill:
				this.KillSpreeTitleAnnounce.text = Language.Get("KILLINGSPREE_DOUBLEKILL_TITLE", TranslationSheets.Announcer);
				anim = this.KillSpree_doublekill_bullets;
				break;
			case AnnouncerLog.AnnouncerEventKinds.TripleKill:
				this.KillSpreeTitleAnnounce.text = Language.Get("KILLINGSPREE_TRIPLEKILL_TITLE", TranslationSheets.Announcer);
				anim = this.KillSpree_triplekill_bullets;
				break;
			case AnnouncerLog.AnnouncerEventKinds.QuadKill:
				this.KillSpreeTitleAnnounce.text = Language.Get("KILLINGSPREE_QUADKILL_TITLE", TranslationSheets.Announcer);
				anim = this.KillSpree_quadrakill_bullets;
				break;
			case AnnouncerLog.AnnouncerEventKinds.UltraKill:
				this.KillSpreeTitleAnnounce.text = Language.Get("KILLINGSPREE_ULTRAKILL_TITLE", TranslationSheets.Announcer);
				anim = this.KillSpree_pentakill_bullets;
				break;
			}
			this.KillSpreeLabelAnnounce.text = string.Format(announce.LocalizedText, GameHubBehaviour.Hub.Announcer.GetColoredPlayerName(announcerEvent.Killer));
			this.KillSpreeAnnounceKillerColor.color = GameHubBehaviour.Hub.Announcer.GetColorByPlayerId(announcerEvent.Killer);
			this.KillSpreeAnnounceKillerIcon.spriteName = "HudStatsThumbTexture";
			if (anim != null)
			{
				anim.gameObject.SetActive(true);
				anim.Rewind();
				anim.Play();
			}
			this.KillSpreeAnnouncePanel.alpha = 1f;
			this.KillSpree_Background.fillAmount = 0f;
			while (this.KillSpree_Background.fillAmount < 1f)
			{
				this.KillSpree_Background.fillAmount += Time.deltaTime * 2f;
				yield return null;
			}
			if (anim != null)
			{
				while (anim.isPlaying)
				{
					yield return null;
				}
			}
			yield return this.waitForKillingSpreeMsgTime;
			this.KillSpreeAnnouncePanel.alpha = 0.99f;
			while (this.KillSpreeAnnouncePanel.alpha > 0f && this.KillSpreeAnnouncePanel.alpha < 1f)
			{
				this.KillSpreeAnnouncePanel.alpha -= Time.deltaTime * 2f;
				yield return null;
			}
			if (anim != null)
			{
				anim.gameObject.SetActive(false);
			}
			GameHubBehaviour.Hub.Announcer.IsHudBusy = false;
			yield break;
		}

		private IEnumerator ShowKillStreakAnnounce(AnnouncerEvent announcerEvent, AnnouncerLog announce)
		{
			if (announcerEvent.AnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.PlayerEndedAKillStreak || announcerEvent.AnnouncerEventKind == AnnouncerLog.AnnouncerEventKinds.PlayerEndedAKillStreakWithAssists)
			{
				this.KillStreakAnnounceKillerColor.color = GameHubBehaviour.Hub.Announcer.GetColorByPlayerId(announcerEvent.Victim);
				this.KillStreakAnnounceKillerIcon.spriteName = "HudStatsThumbTexture";
				this.KillStreakLabelAnnounce.text = GameHubBehaviour.Hub.Announcer.GetColoredPlayerName(announcerEvent.Victim);
				this.KillStreakAnnounceIsoverGroup.SetActive(true);
				this.KillStreakAnnounceIsover.text = Language.Get("KILLSTREAK_ENDED", TranslationSheets.Announcer);
				switch (announcerEvent.CurrentKillStreak)
				{
				case 0:
				case 1:
				case 2:
					yield break;
				case 3:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_03_TITLE", TranslationSheets.Announcer);
					break;
				case 4:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_04_TITLE", TranslationSheets.Announcer);
					break;
				case 5:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_05_TITLE", TranslationSheets.Announcer);
					break;
				case 6:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_06_TITLE", TranslationSheets.Announcer);
					break;
				case 7:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_07_TITLE", TranslationSheets.Announcer);
					break;
				case 8:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_08_TITLE", TranslationSheets.Announcer);
					break;
				case 9:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_09_TITLE", TranslationSheets.Announcer);
					break;
				case 10:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_10_TITLE", TranslationSheets.Announcer);
					break;
				case 15:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_15_TITLE", TranslationSheets.Announcer);
					break;
				}
			}
			else
			{
				this.KillStreakAnnounceKillerColor.color = GameHubBehaviour.Hub.Announcer.GetColorByPlayerId(announcerEvent.Killer);
				this.KillStreakAnnounceKillerIcon.spriteName = "HudStatsThumbTexture";
				this.KillStreakLabelAnnounce.text = string.Format(announce.LocalizedText, GameHubBehaviour.Hub.Announcer.GetColoredPlayerName(announcerEvent.Killer));
				this.KillStreakAnnounceIsoverGroup.SetActive(false);
				switch (announcerEvent.AnnouncerEventKind)
				{
				case AnnouncerLog.AnnouncerEventKinds.KillStreak03:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_03_TITLE", TranslationSheets.Announcer);
					break;
				case AnnouncerLog.AnnouncerEventKinds.KillStreak06:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_06_TITLE", TranslationSheets.Announcer);
					break;
				case AnnouncerLog.AnnouncerEventKinds.KillStreak09:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_09_TITLE", TranslationSheets.Announcer);
					break;
				case AnnouncerLog.AnnouncerEventKinds.KillStreak12:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_12_TITLE", TranslationSheets.Announcer);
					break;
				case AnnouncerLog.AnnouncerEventKinds.KillStreak15:
					this.KillStreakTitleAnnounce.text = Language.Get("KILLSTREAK_15_TITLE", TranslationSheets.Announcer);
					break;
				}
			}
			this.KillStreakAnnouncePanel.alpha = 1f;
			this.KillStreak_Background.fillAmount = 0f;
			while (this.KillStreak_Background.fillAmount < 1f)
			{
				this.KillStreak_Background.fillAmount += Time.deltaTime * 2f;
				yield return null;
			}
			yield return this.waitForKillStreakMsgTime;
			this.KillStreakAnnouncePanel.alpha = 0.99f;
			while (this.KillStreakAnnouncePanel.alpha > 0f && this.KillStreakAnnouncePanel.alpha < 1f)
			{
				this.KillStreakAnnouncePanel.alpha -= Time.deltaTime * 2f;
				yield return null;
			}
			GameHubBehaviour.Hub.Announcer.IsHudBusy = false;
			yield break;
		}

		private void Reset()
		{
			this.KillbyPlayerAnnouncePanel.alpha = 0f;
			this.TeamPanel.alpha = 0f;
			this.KillSpreeAnnouncePanel.alpha = 0f;
			this.KillStreakAnnouncePanel.alpha = 0f;
			GameHubBehaviour.Hub.Announcer.IsHudBusy = false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudAnnouncerController));

		public UILabel TeamLabel;

		public UISprite TeamBackground;

		public UISprite TeamIcon;

		public UIPanel TeamPanel;

		public UISprite KillbyPlayer_Background;

		public UILabel KillbyPlayerLabelAnnounce;

		public UISprite KillbyPlayerAnnounceKillerColor;

		public UISprite KillbyPlayerAnnounceVictimColor;

		public UISprite KillbyPlayerAnnounceKillerIcon;

		public UISprite KillbyPlayerAnnounceVictimIcon;

		public UIPanel KillbyPlayerAnnouncePanel;

		public GameObject KillbyPlayerAnnounceVictimGroup;

		public GameObject KillbyPlayerAnnounceKillerDisconnectedIcon;

		public UISprite KillSpree_Background;

		public UILabel KillSpreeLabelAnnounce;

		public UILabel KillSpreeTitleAnnounce;

		public UISprite KillSpreeAnnounceKillerColor;

		public UISprite KillSpreeAnnounceKillerIcon;

		public UIPanel KillSpreeAnnouncePanel;

		public Animation KillSpree_doublekill_bullets;

		public Animation KillSpree_triplekill_bullets;

		public Animation KillSpree_quadrakill_bullets;

		public Animation KillSpree_pentakill_bullets;

		public UISprite KillStreak_Background;

		public UILabel KillStreakLabelAnnounce;

		public UILabel KillStreakTitleAnnounce;

		public UISprite KillStreakAnnounceKillerColor;

		public UISprite KillStreakAnnounceKillerIcon;

		public UIPanel KillStreakAnnouncePanel;

		public GameObject KillStreakAnnounceIsoverGroup;

		public UILabel KillStreakAnnounceIsover;

		private WaitForSeconds waitForDisconnectionMsgTime;

		private WaitForSeconds waitForWipeMsgTime;

		private WaitForSeconds waitForKillPlayerMsgTime;

		private WaitForSeconds waitForFirstBloodMsgTime;

		private WaitForSeconds waitForKillingSpreeMsgTime;

		private WaitForSeconds waitForKillStreakMsgTime;

		public float playerDisconnectionMsgTime = 2f;

		public float wipeMsgTime = 2f;

		public float killPlayerMsgTime = 2f;

		public float firstBloodMsgTime = 2f;

		public float killingSpreeMsgTime = 1f;

		public float killStreakMsgTime = 2f;
	}
}
