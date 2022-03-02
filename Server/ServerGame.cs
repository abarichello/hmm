using System;
using System.Collections;
using System.Collections.Generic;
using HeavyMetalMachines.Arena.Business;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Combat.Business;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Match.Infra;
using HeavyMetalMachines.Matches.DataTransferObjects;
using Hoplon.Unity.Loading;
using Pocketverse;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Server
{
	[Serializable]
	public class ServerGame : GameState, MatchController.GameOverMessage.IGameOverListener
	{
		protected override void OnStateEnabled()
		{
			GameArenaConfig arenaConfig = GameHubBehaviour.Hub.ArenaConfig;
			int arenaIndex = GameHubBehaviour.Hub.Match.ArenaIndex;
			string sceneName = arenaConfig.GetSceneName(arenaIndex);
			ServerGame.Log.DebugFormat("LOADING: server LoadLevel {0}", new object[]
			{
				sceneName
			});
			this._arenaToken = new LoadingToken(base.GetType());
			this._arenaToken.AddLoadable(new SceneLoadable(sceneName, true));
			base.StartCoroutine(this.LoadMatchSceneCoroutine());
			GameHubBehaviour.Hub.Server.ServerSetNotReadyLocal();
		}

		private IEnumerator LoadMatchSceneCoroutine()
		{
			AsyncRequest<LoadingResult> request = Loading.Engine.LoadToken(this._arenaToken);
			yield return request;
			if (LoadStatusExtensions.IsError(request.Result.Status))
			{
				ServerGame.Log.ErrorFormat("Loading failed with status: {0}", new object[]
				{
					request.Result.Status
				});
				yield return LoadingFailedHandler.HandleFailure(request.Result);
				yield break;
			}
			base.LoadingToken.InheritData(this._arenaToken);
			this._arenaToken = null;
			GameHubBehaviour.Hub.GameTime.SetTimeZero();
			GameHubBehaviour.Hub.UpdateManager.SetRunning(true);
			GameHubBehaviour.Hub.MatchHistory.WriteMatchStartInfo();
			this.MarkMatchAsStartedForPlayers();
			this._combatControllerStorageInitializer.Initialize();
			this._arenaModifierInitializer.InitializeModifierApplier();
			yield break;
		}

		protected override void OnStateDisabled()
		{
			this._arenaModifierInitializer.Dispose();
			GameHubBehaviour.Hub.UpdateManager.SetRunning(false);
			((NetworkServer)GameHubBehaviour.Hub.Net).LogPacketSentStatistics();
		}

		public void OnGameOver(MatchController.GameOverMessage msg)
		{
			this.GameOver.State = msg.State;
			this.GameOver.LoadingToken.InheritData(base.LoadingToken);
			GameHubBehaviour.Hub.State.GotoState(this.GameOver, false);
			MatchLogWriter.WriteAfk(this._afkManager);
			this.MarkMatchAsDoneForPlayers();
		}

		private void MarkMatchAsStartedForPlayers()
		{
			MatchKind kind = GameHubBehaviour.Hub.Match.Kind;
			if (kind != 2 && kind != 6)
			{
				ServerGame.Log.InfoFormat("No need to mark start MatchKind = {0}", new object[]
				{
					kind
				});
				return;
			}
			List<PlayerData> players = GameHubBehaviour.Hub.Players.Players;
			for (int i = 0; i < players.Count; i++)
			{
				ObservableExtensions.Subscribe<Unit>(this._markMatchService.MarkPlayerHasStarted(players[i].PlayerId, GameHubBehaviour.Hub.Match.Kind));
			}
		}

		private void MarkMatchAsDoneForPlayers()
		{
			MatchKind kind = GameHubBehaviour.Hub.Match.Kind;
			if (kind != 2 && kind != 6)
			{
				ServerGame.Log.InfoFormat("No need to mark done MatchKind = {0}", new object[]
				{
					kind
				});
				return;
			}
			List<PlayerData> players = GameHubBehaviour.Hub.Players.Players;
			for (int i = 0; i < players.Count; i++)
			{
				ObservableExtensions.Subscribe<Unit>(this._markMatchService.MarkPlayerHasDone(players[i].PlayerId, GameHubBehaviour.Hub.Match.Kind));
			}
		}

		private static BitLogger Log = new BitLogger(typeof(ServerGame));

		[Inject]
		private ICombatControllerStorageInitializer _combatControllerStorageInitializer;

		[Inject]
		private IArenaModifierInitializer _arenaModifierInitializer;

		[Inject]
		private IMarkMatchService _markMatchService;

		[InjectOnServer]
		private IAFKManager _afkManager;

		public ServerFinish GameOver;

		private LoadingToken _arenaToken;
	}
}
