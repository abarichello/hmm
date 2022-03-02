using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using ClientAPI;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Infra.Quiz;
using HeavyMetalMachines.OpenUrl.Infra;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Swordfish;
using Hoplon.Serialization;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class UIProgressionRating : GameHubBehaviour
	{
		public void Start()
		{
			this._skipSwordfish = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish);
			this._clickInQuestionButton = 0;
			this._questionId = 1;
			if (SpectatorController.IsSpectating)
			{
				base.gameObject.SetActive(false);
				return;
			}
			if (!this._skipSwordfish)
			{
				this._getQuizTypeForPlayerDisposable = ObservableExtensions.Subscribe<QuizBag>(this.GetQuizTypeForPlayer());
			}
		}

		public void OnQuestionButtonClick()
		{
			Guid matchId = Guid.Empty;
			if (!this._skipSwordfish)
			{
				matchId = GameHubBehaviour.Hub.Swordfish.Msg.ClientMatchId;
			}
			string url = this._quizUrlFileProvider.GetQuizUrl(matchId);
			ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(Observable.First<bool>(this._getUgcRestrictionIsEnabled.OfferToChangeGlobalRestriction(), (bool isRestricted) => !isRestricted), delegate(bool _)
			{
				this._openUrlService.OpenUrl(url);
			}));
			this._quizUrlFileProvider.TryDeleteQuizUrlFile();
			this._clickInQuestionButton = 1;
		}

		public void CommitSelection()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			string msg = string.Format("SteamID={0} QuestionID={1} Answer={2}", GameHubBehaviour.Hub.User.UniversalId, this._questionId, this._clickInQuestionButton);
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMatchMsg(58, msg, false);
		}

		private IObservable<QuizBag> GetQuizTypeForPlayer()
		{
			return Observable.DoOnError<QuizBag>(Observable.Do<QuizBag>(Observable.Select<NetResult, QuizBag>(this.GetPlayerEligibleForRookieQuizFromCustomWs(), (NetResult netResult) => (QuizBag)((JsonSerializeable<!0>)netResult.Msg)), new Action<QuizBag>(this.OnPlayerEligibleSuccess)), new Action<Exception>(this.OnGetPlayerEligibleFailure));
		}

		private void OnGetPlayerEligibleFailure(Exception obj)
		{
			UIProgressionRating.Log.ErrorFormat("Failed to get PlayerEligibleForRookieQuiz. Exception: {0}", new object[]
			{
				obj
			});
		}

		private void OnPlayerEligibleSuccess(QuizBag quizBag)
		{
			this._isPlayerEligibleForRookieQuiz = quizBag.IsPlayerEligibleForRookieQuiz;
			if (this._isPlayerEligibleForRookieQuiz)
			{
				this._glowIdleAnimation.Play();
				return;
			}
			base.transform.localScale = new Vector3(this._scaleForVeteranQuiz, this._scaleForVeteranQuiz, 0f);
		}

		private IObservable<NetResult> GetPlayerEligibleForRookieQuizFromCustomWs()
		{
			return SwordfishObservable.FromStringSwordfishCall<NetResult>(delegate(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
			{
				PlayerCustomWS.IsPlayerEligibleForRookieQuiz(GameHubBehaviour.Hub.User.UniversalId, onSuccess, onError);
			});
		}

		private void OnDestroy()
		{
			if (this._getQuizTypeForPlayerDisposable != null)
			{
				this._getQuizTypeForPlayerDisposable.Dispose();
				this._getQuizTypeForPlayerDisposable = null;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(UIProgressionRating));

		[InjectOnClient]
		private IQuizUrlFileProvider _quizUrlFileProvider;

		[InjectOnClient]
		private IGetUGCRestrictionIsEnabled _getUgcRestrictionIsEnabled;

		[InjectOnClient]
		private IUGCRestrictionDialogPresenter _ugcRestrictionDialogPresenter;

		[InjectOnClient]
		private IOpenUrlService _openUrlService;

		[SerializeField]
		private Animation _glowIdleAnimation;

		[SerializeField]
		private float _scaleForVeteranQuiz = 0.8f;

		private bool _skipSwordfish;

		private bool _isPlayerEligibleForRookieQuiz;

		private int _clickInQuestionButton;

		private int _questionId;

		private IDisposable _getQuizTypeForPlayerDisposable;
	}
}
