using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class HORTAState : GameState
	{
		protected override void OnMyLevelLoaded()
		{
			GameHubBehaviour.Hub.Config.SetValue(ConfigAccess.SkipSwordfish, true.ToString());
			this._login = new Login(GameHubBehaviour.Hub, new Action<bool>(this.LoginEnd), new Action<ConfirmWindowProperties>(this.BogusConfirm));
			this._login.ConnectSteam();
		}

		private void BogusConfirm(ConfirmWindowProperties obj)
		{
		}

		private void LoginEnd(bool obj)
		{
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
		}

		private void ListenToMatchReady()
		{
			base.GoToState(this.Loading, false);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTAState));

		public LoadingState Loading;

		public HORTAComponent Component;

		private Login _login;
	}
}
