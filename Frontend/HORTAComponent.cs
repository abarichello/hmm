using System;
using System.Diagnostics;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HORTAComponent : GameHubScriptableObject
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string, string> OnVersionMismatched;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<Exception> OnReadFileException;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<IMatchInformation> OnMatchFileLoaded;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnMatchReadyToPlay;

		public IMatchInformation MatchInfo { get; private set; }

		public void LoadFile(string path)
		{
			IMatchInformation matchInformation;
			try
			{
				MatchFile.ReadFile(path, out matchInformation);
			}
			catch (Exception obj)
			{
				if (this.OnReadFileException != null)
				{
					this.OnReadFileException(obj);
				}
				return;
			}
			if (string.Compare(matchInformation.Version, "2.07.972", StringComparison.InvariantCulture) != 0 && this.OnVersionMismatched != null)
			{
				this.OnVersionMismatched(matchInformation.Version, "2.07.972");
			}
			if (this.OnMatchFileLoaded != null)
			{
				this.OnMatchFileLoaded(matchInformation);
			}
			this.MatchInfo = matchInformation;
		}

		public bool Play()
		{
			if (this.MatchInfo == null || this.OnMatchReadyToPlay == null)
			{
				return false;
			}
			this.Behaviour = GameHubScriptableObject.Hub.gameObject.AddComponent<HORTABehaviour>();
			this.StatePlayback = GameHubScriptableObject.Hub.gameObject.AddComponent<HORTAStatePlayback>();
			this.Net = GameHubScriptableObject.Hub.gameObject.AddComponent<HORTANetwork>();
			this.HORTAClock = new HORTATime();
			this._oldNet = GameHubScriptableObject.Hub.Net;
			GameHubScriptableObject.Hub.Net = this.Net;
			GameHubScriptableObject.Hub.SetGameTimer(this.HORTAClock);
			this.Behaviour.Init(this, this.StatePlayback, (HORTAState)GameHubScriptableObject.Hub.State.Current, GameHubScriptableObject.Hub.Match.State);
			GameHubScriptableObject.Hub.Match = this.MatchInfo.Data;
			GameHubScriptableObject.Hub.Match.State = MatchData.MatchState.PreMatch;
			GameHubScriptableObject.Hub.playbackSystem.SetBuffer(this.MatchInfo.KeyFrames);
			this.StatePlayback.Init(this, this.MatchInfo.States);
			GameHubScriptableObject.Hub.Players.AddNarrator(new PlayerData(GameHubScriptableObject.Hub.User.UserSF.PublisherUserId, 0, (!GameHubScriptableObject.Hub.Match.LevelIsTutorial()) ? TeamKind.Blue : TeamKind.Red, this.Net.GetMyAddress(), 0, false, true, new BattlepassProgress()));
			GameHubScriptableObject.Hub.User.ConnectToHORTA(GameHubScriptableObject.Hub.Match);
			this.StatePlayback.RunInitialStates();
			this.OnMatchReadyToPlay();
			return true;
		}

		public void EndGame()
		{
			this.Behaviour.CallEndGame();
		}

		public void CleanUp()
		{
			this.MatchInfo = null;
			UnityEngine.Object.Destroy(this.Behaviour);
			UnityEngine.Object.Destroy(this.StatePlayback);
			UnityEngine.Object.Destroy(this.Net);
			this.HORTAClock.ToggleOffAny();
			this.HORTAClock = null;
			GameHubScriptableObject.Hub.SetGameTimer(null);
			GameHubScriptableObject.Hub.Net = this._oldNet;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTAComponent));

		[NonSerialized]
		public HORTATime HORTAClock;

		[NonSerialized]
		public HORTAStatePlayback StatePlayback;

		[NonSerialized]
		public HORTABehaviour Behaviour;

		[NonSerialized]
		public HORTANetwork Net;

		[NonSerialized]
		private Pocketverse.Network _oldNet;
	}
}
