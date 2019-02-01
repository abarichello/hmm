using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Infra.Counselor;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Counselor
{
	public class HudCounselorCursorController : GameHubBehaviour
	{
		private void Reset()
		{
			this.state = HudCounselorCursorController.State.Off;
			this._anim.Stop();
			this._anim.Sample();
			this._anim.Play(this.animationOffName);
		}

		private void OnEnable()
		{
			this.Reset();
			if (!SpectatorController.IsSpectating)
			{
				GameHubBehaviour.Hub.ClientCounselorController.OnAudioPlayingChanged += this.OnAudioPlayingChanged;
				GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned += this.ListenToAllPlayersSpawned;
				GameHubBehaviour.Hub.Events.Bots.ListenToAllPlayersSpawned += this.ListenToAllPlayersSpawned;
				GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
			}
		}

		private void OnPhaseChange(BombScoreBoard.State bombScoreBoardState)
		{
			if (this.state != HudCounselorCursorController.State.Off && bombScoreBoardState != BombScoreBoard.State.BombDelivery)
			{
				this.GoToOffState(true);
			}
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.ClientCounselorController.OnAudioPlayingChanged -= this.OnAudioPlayingChanged;
			GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned -= this.ListenToAllPlayersSpawned;
			GameHubBehaviour.Hub.Events.Bots.ListenToAllPlayersSpawned -= this.ListenToAllPlayersSpawned;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			if (this._combat != null)
			{
				this._combat.SpawnController.OnStateChanged -= this.SpawnControllerOnStateChanged;
			}
		}

		private void ListenToAllPlayersSpawned()
		{
			if (!GameHubBehaviour.Hub.Events.Players.CarCreationFinished || !GameHubBehaviour.Hub.Events.Bots.CarCreationFinished)
			{
				return;
			}
			this._combat = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetComponent<CombatObject>();
			this._combat.SpawnController.OnStateChanged += this.SpawnControllerOnStateChanged;
		}

		private void SpawnControllerOnStateChanged(SpawnController.StateType stateType)
		{
			if (this.state != HudCounselorCursorController.State.Off && stateType != SpawnController.StateType.PreSpawned)
			{
				this.GoToOffState(false);
			}
		}

		private void GoToOffState(bool immediate)
		{
			this.StopCurrentAnim();
			if (immediate)
			{
				this._anim.Play(this.animationOffName);
			}
			else
			{
				this._anim.Play(this.animationKeyOutName);
			}
			this.state = HudCounselorCursorController.State.Off;
		}

		private void OnAudioPlayingChanged()
		{
			CounselorConfig.AdvicesConfig currentAdviceConfig = GameHubBehaviour.Hub.ClientCounselorController.CurrentAdviceConfig;
			if (string.IsNullOrEmpty(currentAdviceConfig.CursorText))
			{
				if (this.state != HudCounselorCursorController.State.Off)
				{
					this.GoToOffState(false);
				}
				return;
			}
			if (!GameHubBehaviour.Hub.ClientCounselorController.IsPlaying)
			{
				return;
			}
			this._messageLabel.text = Language.Get(currentAdviceConfig.CursorText, TranslationSheets.Advisor);
			if (this.state == HudCounselorCursorController.State.Off)
			{
				this.state = HudCounselorCursorController.State.In;
				this._anim.Play(this.animationKeyInName);
				if (this.updateInStateCoroutine != null)
				{
					base.StopCoroutine(this.updateInStateCoroutine);
				}
				this.updateInStateCoroutine = base.StartCoroutine(this.UpdateInState());
			}
		}

		private void StopCurrentAnim()
		{
			if (this._anim.clip.wrapMode != WrapMode.Loop)
			{
				return;
			}
			this._anim.Stop(this._anim.clip.name);
			this._anim.Rewind(this._anim.clip.name);
			this._anim.Sample();
		}

		private IEnumerator UpdateInState()
		{
			yield return UnityUtils.WaitForOneSecond;
			if (!this._anim.isPlaying)
			{
				yield return UnityUtils.WaitForEndOfFrame;
			}
			this.StopCurrentAnim();
			this._anim.Play(this.animationKeyIdleName);
			this.state = HudCounselorCursorController.State.Showing;
			yield break;
		}

		private HudCounselorCursorController.State state = HudCounselorCursorController.State.Off;

		[SerializeField]
		private Text _messageLabel;

		[SerializeField]
		private Animation _anim;

		[SerializeField]
		private string animationOffName;

		[SerializeField]
		private string animationKeyInName;

		[SerializeField]
		private string animationKeyIdleName;

		[SerializeField]
		private string animationKeyOutName;

		private Coroutine updateInStateCoroutine;

		private CombatObject _combat;

		public enum State
		{
			Off = 1,
			In,
			Showing,
			ShowingTransition
		}
	}
}
