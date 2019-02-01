using System;
using HeavyMetalMachines.Frontend;
using Pocketverse;

namespace HeavyMetalMachines.Match
{
	public class AutoMatch : GameHubBehaviour
	{
		private void Awake()
		{
			GameHubBehaviour.Hub.Server.OnMatchStateChanged += this.OnMatchStateChanged;
			MainMenuGui.OnMenuDisplayed += this.OnMenuDisplayed;
		}

		private void Start()
		{
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnMatchCanceledEvent += this.OnMatchCanceledEvent;
		}

		private void OnDestroy()
		{
			MainMenuGui.OnMenuDisplayed -= this.OnMenuDisplayed;
			if (GameHubBehaviour.Hub == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Swordfish.Msg != null && GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking != null)
			{
				GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnMatchCanceledEvent -= this.OnMatchCanceledEvent;
			}
			GameHubBehaviour.Hub.Server.OnMatchStateChanged -= this.OnMatchStateChanged;
		}

		private void OnMatchStateChanged(MatchData.MatchState newmatchstate)
		{
			this._lastMatchKind = GameHubBehaviour.Hub.Match.Kind;
		}

		private void OnMenuDisplayed()
		{
			if (this.MustRejoinQueue)
			{
				MainMenuGui stateGuiController = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
				MatchData.MatchKind lastMatchKind = this._lastMatchKind;
				if (lastMatchKind != MatchData.MatchKind.PvE)
				{
					if (lastMatchKind == MatchData.MatchKind.PvP)
					{
						stateGuiController.JoinQueue(GameModeTabs.Normal);
					}
				}
				else
				{
					stateGuiController.JoinQueue(GameModeTabs.CoopVsBots);
				}
				this._automatchStreak++;
				GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(ClientBITags.Automatch, string.Format("MatchKind={0} Streak={1}", this._lastMatchKind, this._automatchStreak), true);
			}
			else
			{
				this._automatchStreak = 0;
			}
			this.MustRejoinQueue = false;
		}

		private void OnMatchCanceledEvent(string[] obj)
		{
			this._automatchStreak = 0;
			this.MustRejoinQueue = false;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(AutoMatch));

		private MatchData.MatchKind _lastMatchKind;

		private int _automatchStreak;

		public bool MustRejoinQueue;
	}
}
