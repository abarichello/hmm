using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	public class BombDeliveryAnimator : GameHubBehaviour
	{
		private void OnEnable()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
			this._animator = base.GetComponent<Animator>();
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.ListenToBombDelivery;
			this._gameState = (GameHubBehaviour.Hub.State.Current as Game);
			if (this._gameState != null)
			{
				this._gameState.OnGameOver += this.GameOver;
			}
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.ListenToBombDelivery;
		}

		private void ListenToBombDelivery(int causerid, TeamKind scoredTeam, Vector3 deliveryPosition)
		{
			TeamKind targetTeam = this.TargetTeam;
			if (targetTeam != TeamKind.Blue)
			{
				if (targetTeam == TeamKind.Red)
				{
					this.SetAnimatorParam(GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed);
				}
			}
			else
			{
				this.SetAnimatorParam(GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue);
			}
		}

		private void SetAnimatorParam(int score)
		{
			this._animator.SetInteger(this.AnimatorParameterName, score);
		}

		private void GameOver(MatchData.MatchState matchWinner)
		{
			if (this._gameState != null)
			{
				this._gameState.OnGameOver -= this.GameOver;
			}
			if (matchWinner != MatchData.MatchState.MatchOverRedWins)
			{
				if (matchWinner == MatchData.MatchState.MatchOverBluWins)
				{
					if (this.TargetTeam == TeamKind.Blue)
					{
						this.SetAnimatorParam(GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue + 1);
					}
				}
			}
			else if (this.TargetTeam == TeamKind.Red)
			{
				this.SetAnimatorParam(GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed + 1);
			}
		}

		public string AnimatorParameterName = "score";

		public TeamKind TargetTeam;

		private Animator _animator;

		private Game _gameState;
	}
}
