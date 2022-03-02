using System;
using System.Collections.Generic;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Tournament;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Teams
{
	public class ITeamAccess
	{
		private static Dictionary<string, Team> UserTeams
		{
			get
			{
				return (ITeamAccess._userTeams != null) ? ITeamAccess._userTeams : (ITeamAccess._userTeams = new Dictionary<string, Team>(16));
			}
		}

		public static void ClearCache()
		{
			ITeamAccess.UserTeams.Clear();
		}

		public static void GetGroupTeamAsync(HMMHub hub, TeamKind teamKind, ITeamAccess.GetGroupTeamDelegate onSuccess, ITeamAccess.TeamErrorDelegate onError)
		{
			if (hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				if (teamKind != TeamKind.Red)
				{
					onSuccess(null);
				}
				else
				{
					onSuccess(new Team
					{
						Bag = new TeamBag
						{
							goalsMade = 666,
							goalsTaken = 3,
							matchsParticipated = 666
						}.ToString(),
						ImageUrl = "team_image_meme_02",
						Name = "SkipSFName",
						Id = Guid.NewGuid(),
						Tag = "RED"
					});
				}
				return;
			}
			List<PlayerData> players = hub.Players.Players;
			List<string> list = new List<string>(4);
			for (int i = 0; i < players.Count; i++)
			{
				PlayerData playerData = players[i];
				if (playerData.Team == teamKind && !playerData.IsBot)
				{
					list.Add(playerData.UserId);
				}
			}
			ITeamAccess.GetGroupTeamAsync(hub, list, onSuccess, onError);
		}

		public static void GetGroupTeamAsync(HMMHub hub, List<string> universalIds, ITeamAccess.GetGroupTeamDelegate onSuccess, ITeamAccess.TeamErrorDelegate onError)
		{
			if (universalIds.Count != 4 || hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				onSuccess(null);
				return;
			}
			int sameTeamMemberCount = 0;
			int notSameTeamMemberCount = 0;
			Guid teamId = Guid.Empty;
			bool hasError = false;
			for (int i = 0; i < universalIds.Count; i++)
			{
				string universalId = universalIds[i];
				ITeamAccess.GetUserTeamAsync(hub, universalId, delegate(Team team)
				{
					if (team == null)
					{
						notSameTeamMemberCount++;
						return;
					}
					if (teamId == Guid.Empty)
					{
						teamId = team.Id;
					}
					if (teamId == team.Id)
					{
						sameTeamMemberCount++;
					}
					else
					{
						notSameTeamMemberCount++;
					}
					if (!hasError && sameTeamMemberCount + notSameTeamMemberCount == 4)
					{
						if (notSameTeamMemberCount == 0)
						{
							onSuccess(team);
						}
						else
						{
							onSuccess(null);
						}
					}
				}, delegate(Exception exception)
				{
					hasError = true;
					onError(exception);
				});
			}
		}

		public static void GetUserTagAsync(HMMHub hub, string universalId, ITeamAccess.GetUserTagDelegate onResult, ITeamAccess.TeamErrorDelegate onError)
		{
			if (hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				onResult(ITeamAccess.GetTagColoredEncoded("SKIP", hub.GuiScripts.GUIColors.TeamTagColor));
				return;
			}
			ITeamAccess.GetUserTeamAsync(hub, universalId, delegate(Team team)
			{
				if (team == null)
				{
					onResult(string.Empty);
					return;
				}
				onResult(ITeamAccess.GetTagColoredEncoded(team.Tag, hub.GuiScripts.GUIColors.TeamTagColor));
			}, onError);
		}

		public static void GetUserTeamAsync(HMMHub hub, string universalId, ITeamAccess.GetUserTeamDelegate onGetUser, ITeamAccess.TeamErrorDelegate onError)
		{
			if (hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				onGetUser(null);
				return;
			}
			Team tag;
			if (ITeamAccess.UserTeams.TryGetValue(universalId, out tag))
			{
				onGetUser(tag);
				return;
			}
			if (!hub.ClientApi.IsLogged)
			{
				ITeamAccess.Log.ErrorFormat("[Async] someone is trying to GetTeamByUniversalId but is not logged. stack{0}", new object[]
				{
					StackTraceUtility.ExtractStackTrace()
				});
				return;
			}
			hub.ClientApi.team.GetTeamByUniversalId(hub.State.Current.StateKind, universalId, delegate(object o, Team team)
			{
				GameState.GameStateKind gameStateKind = (GameState.GameStateKind)o;
				if (gameStateKind == hub.State.Current.StateKind)
				{
					ITeamAccess.UserTeams[universalId] = team;
					onGetUser(team);
					TeamController.ResetTimer();
				}
				else
				{
					ITeamAccess.Log.WarnFormat("GetTeamByUniversalId on wrong state. Current = {0} Old = {1}", new object[]
					{
						hub.State.Current.StateKind,
						gameStateKind
					});
				}
			}, delegate(object o, Exception exception)
			{
				onError(exception);
			});
		}

		public static string GetTagColoredEncoded(string teamTag, Color textColor)
		{
			return string.Format("[{0}][[-][{0}]{1}[-][{0}]][-]", HudUtils.RGBToHex(textColor), NGUIText.EscapeSymbols(teamTag));
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ITeamAccess));

		private static Dictionary<string, Team> _userTeams;

		public delegate void GetGroupTeamDelegate(Team team);

		public delegate void TeamErrorDelegate(Exception exception);

		public delegate void GetUserTagDelegate(string tag);

		public delegate void GetUserTeamDelegate(Team tag);
	}
}
