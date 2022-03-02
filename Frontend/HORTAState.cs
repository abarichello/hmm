using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class HORTAState : GameState
	{
		protected override void OnMyLevelLoaded()
		{
			GameHubBehaviour.Hub.Config.SetValue(ConfigAccess.SkipSwordfish, true.ToString());
			GameHubBehaviour.Hub.PlayerPrefs.SkipSwordfishLoad();
		}

		private void BogusConfirm(ConfirmWindowProperties obj)
		{
			HORTAState.Log.DebugFormat("Bogus Confirm Window={0} Question={1}", new object[]
			{
				obj.TileText,
				obj.QuestionText
			});
		}

		private void LoginEnd(bool obj)
		{
			HORTAState.Log.DebugFormat("Steam login ended={0}", new object[]
			{
				obj
			});
		}

		protected override void OnStateEnabled()
		{
			this.Component.OnMatchFileLoaded += this.ListenToMatchFileLoaded;
			this.Component.OnMatchReadyToPlay += this.ListenToMatchReady;
		}

		protected override void OnStateDisabled()
		{
			this.Component.OnMatchFileLoaded -= this.ListenToMatchFileLoaded;
			this.Component.OnMatchReadyToPlay -= this.ListenToMatchReady;
		}

		private void ListenToMatchFileLoaded(IMatchInformation match)
		{
			HORTAState.Log.DebugFormat("Match file loaded={0}", new object[]
			{
				match.Data
			});
		}

		private void ListenToMatchReady()
		{
			base.GoToState(this.Loading, false);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTAState));

		public LoadingState Loading;

		[InjectOnClient]
		private HORTAComponent Component;
	}
}
