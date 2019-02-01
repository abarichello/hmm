using System;
using System.Collections;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudFeedbacksController : GameHubBehaviour
	{
		public void Awake()
		{
			this._animators = new Animator[]
			{
				this.DisputeYellowAnimator,
				this.DisputeRedAnimator,
				this.DisputeBlueAnimator
			};
			for (int i = 0; i < this._animators.Length; i++)
			{
				this._animators[i].gameObject.SetActive(false);
			}
			for (int j = 0; j < this.ExitEvents.Length; j++)
			{
				this.ExitEvents[j].OnExitEvent += this.OnFeedbackExitEvent;
			}
			this._centerFeedback = false;
		}

		public void Start()
		{
			GameHubBehaviour.Hub.BombManager.OnDisputeStarted += this.OnDisputeStarted;
			GameHubBehaviour.Hub.BombManager.OnDisputeFinished += this.OnDisputeFinished;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.PlayersOnListenToObjectUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.PlayersOnListenToObjectSpawn;
		}

		public void OnDestroy()
		{
			for (int i = 0; i < this.ExitEvents.Length; i++)
			{
				this.ExitEvents[i].OnExitEvent -= this.OnFeedbackExitEvent;
			}
			GameHubBehaviour.Hub.BombManager.OnDisputeStarted -= this.OnDisputeStarted;
			GameHubBehaviour.Hub.BombManager.OnDisputeFinished -= this.OnDisputeFinished;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.PlayersOnListenToObjectUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.PlayersOnListenToObjectSpawn;
			GameHubBehaviour.Hub.Announcer.IsHudBusy = false;
		}

		private void PlayersOnListenToObjectSpawn(PlayerEvent data)
		{
			if (data.TargetId == GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
			{
				this._playerRespawning = false;
			}
		}

		private void PlayersOnListenToObjectUnspawn(PlayerEvent data)
		{
			if (data.TargetId == GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
			{
				this._playerRespawning = true;
			}
		}

		private bool CanShow()
		{
			return !this._playerRespawning;
		}

		private void OnDisputeStarted()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				HudFeedbacksController.Log.Warn("trying to call OnDisputeStarted but gameObject is not active. Are you using winHack?");
				return;
			}
			if (this.CanShow())
			{
				base.StartCoroutine(this.ShowDisputeAnimation(GameHubBehaviour.Hub.Players.CurrentPlayerTeam, TeamKind.Neutral));
			}
		}

		private void OnDisputeFinished(TeamKind obj)
		{
			if (!base.gameObject.activeInHierarchy)
			{
				HudFeedbacksController.Log.Warn("trying to call OnDisputeFinishedWithAWinner but gameObject is not active. Are you using winHack?");
				return;
			}
			if (obj != TeamKind.Zero && this.CanShow())
			{
				base.StartCoroutine(this.ShowDisputeAnimation(GameHubBehaviour.Hub.Players.CurrentPlayerTeam, obj));
			}
		}

		private void OnFeedbackExitEvent(HudFeedbackExitEvent feedbackExitEvent)
		{
			base.StartCoroutine(this.DisableFeedbackEventObject(feedbackExitEvent));
		}

		private IEnumerator DisableFeedbackEventObject(HudFeedbackExitEvent feedbackExitEvent)
		{
			yield return UnityUtils.WaitForEndOfFrame;
			feedbackExitEvent.gameObject.SetActive(false);
			HudFeedbackExitEvent.FeedbackType type = feedbackExitEvent.Type;
			if (type == HudFeedbackExitEvent.FeedbackType.Center)
			{
				this._centerFeedback = false;
			}
			yield break;
		}

		private void SetFeedbackAnimatorActive(Animator animator, bool isActive)
		{
			animator.SetBool("active", isActive);
		}

		private void EnableAnimator(Animator animator)
		{
			animator.gameObject.SetActive(true);
			this.SetFeedbackAnimatorActive(animator, false);
		}

		private IEnumerator ShowDisputeAnimation(TeamKind clientOwnerTeamKind, TeamKind disputeWinner)
		{
			while (this._centerFeedback)
			{
				yield return UnityUtils.WaitForEndOfFrame;
			}
			this._centerFeedback = true;
			Animator disputeAnimator = null;
			float timeInSec = 0f;
			if (clientOwnerTeamKind == disputeWinner)
			{
				disputeAnimator = this.DisputeBlueAnimator;
				timeInSec = this.HudFeedbacksSettings.BombPickDisputeFinishedFeedbackDurationInSec;
			}
			else if (disputeWinner == TeamKind.Blue || disputeWinner == TeamKind.Red)
			{
				disputeAnimator = this.DisputeRedAnimator;
				timeInSec = this.HudFeedbacksSettings.BombPickDisputeFinishedFeedbackDurationInSec;
			}
			else if (disputeWinner == TeamKind.Neutral)
			{
				disputeAnimator = this.DisputeYellowAnimator;
				timeInSec = this.HudFeedbacksSettings.BombPickDisputeStartedFeedbackDurationInSec;
			}
			if (disputeAnimator)
			{
				if (!disputeAnimator.gameObject.activeSelf)
				{
					this.EnableAnimator(disputeAnimator);
				}
				this.SetFeedbackAnimatorActive(disputeAnimator, true);
				yield return new WaitForSeconds(timeInSec);
				this.SetFeedbackAnimatorActive(disputeAnimator, false);
			}
			else
			{
				this._centerFeedback = false;
			}
			GameHubBehaviour.Hub.Announcer.IsHudBusy = false;
			yield break;
		}

		public void Update()
		{
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudFeedbacksController));

		private const string AnimatorActivePropertyName = "active";

		public HudFeedbacksSettings HudFeedbacksSettings;

		public Animator DisputeYellowAnimator;

		public Animator DisputeBlueAnimator;

		public Animator DisputeRedAnimator;

		public HudFeedbackExitEvent[] ExitEvents;

		private Animator[] _animators;

		private bool _centerFeedback;

		private bool _playerRespawning;

		public enum FeedbackKind
		{
			Generic
		}
	}
}
