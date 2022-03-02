using System;
using System.Collections.Generic;
using ClientAPI.Objects;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class MatchTeams : KeyStateParser, IMatchTeamsServer, IMatchTeams, IMatchTeamsDispatcher, IKeyStateParser
	{
		public void AddTeam(string universalId, Team team)
		{
			if (team == null)
			{
				return;
			}
			Team team2 = this._teams.Find((Team t) => t.Id == team.Id);
			bool flag = team2 == null;
			if (flag)
			{
				this._teams.Add(team);
				team2 = team;
			}
			this._userTeams[universalId] = team2;
			MatchTeams.Log.DebugFormat("Added team={0} user={1} notFound={2}", new object[]
			{
				team2.WriteToString(),
				universalId,
				flag
			});
		}

		public void SetGroupTeam(TeamKind group, Team team)
		{
			if (team == null)
			{
				return;
			}
			Team team2 = this._teams.Find((Team t) => t.Id == team.Id);
			this._groupTeams[group] = team2;
			MatchTeams.Log.DebugFormat("Added group={0} team={1}", new object[]
			{
				group,
				team2.WriteToString()
			});
		}

		public string GetPlayerTag(string universalId)
		{
			Team team;
			if (this._userTeams.TryGetValue(universalId, out team))
			{
				return team.Tag;
			}
			return null;
		}

		public Team GetPlayerTeam(string universalId)
		{
			Team result;
			if (this._userTeams.TryGetValue(universalId, out result))
			{
				return result;
			}
			return null;
		}

		public Team GetGroupTeam(TeamKind group)
		{
			Team result;
			if (this._groupTeams.TryGetValue(group, out result))
			{
				return result;
			}
			return null;
		}

		public override StateType Type
		{
			get
			{
				return StateType.Teams;
			}
		}

		public override void Update(BitStream stream)
		{
			this._serial = stream.ReadCompressedInt();
			int num = stream.ReadCompressedInt();
			this._teams.Clear();
			for (int i = 0; i < num; i++)
			{
				Team team = new Team();
				team.Name = stream.ReadString();
				team.Tag = stream.ReadString();
				team.ImageUrl = stream.ReadString();
				team.Bag = stream.ReadString();
				team.Id = stream.ReadGuid();
				team.CurrentUgmUserUniversalId = stream.ReadString();
				team.CurrentUgmUserPlayerId = stream.ReadLong();
				this._teams.Add(team);
			}
			int num2 = stream.ReadCompressedInt();
			this._userTeams.Clear();
			for (int j = 0; j < num2; j++)
			{
				string key = stream.ReadString();
				int index = stream.ReadCompressedInt();
				this._userTeams[key] = this._teams[index];
			}
			int num3 = stream.ReadCompressedInt();
			this._groupTeams.Clear();
			for (int k = 0; k < num3; k++)
			{
				TeamKind key2 = (TeamKind)stream.ReadByte();
				int index2 = stream.ReadCompressedInt();
				this._groupTeams[key2] = this._teams[index2];
			}
			MatchTeams.Log.DebugFormat("Received teams={0} count={1}", new object[]
			{
				this._serial,
				num
			});
		}

		public void UpdateTeams()
		{
			this.SendTeams(1);
		}

		public void SendTeams(byte to)
		{
			this._serial++;
			MatchTeams.Log.DebugFormat("Sending teams={0} to={1}", new object[]
			{
				this._serial,
				to
			});
			BitStream stream = base.GetStream();
			stream.WriteCompressedInt(this._serial);
			stream.WriteCompressedInt(this._teams.Count);
			for (int i = 0; i < this._teams.Count; i++)
			{
				Team team = this._teams[i];
				stream.WriteString(team.Name);
				stream.WriteString(team.Tag);
				stream.WriteString(team.ImageUrl);
				stream.WriteString(team.Bag);
				stream.WriteGuid(team.Id);
				stream.WriteString(team.CurrentUgmUserUniversalId);
				stream.WriteLong(team.CurrentUgmUserPlayerId);
			}
			stream.WriteCompressedInt(this._userTeams.Count);
			foreach (KeyValuePair<string, Team> keyValuePair in this._userTeams)
			{
				stream.WriteString(keyValuePair.Key);
				stream.WriteCompressedInt(this._teams.IndexOf(keyValuePair.Value));
			}
			stream.WriteCompressedInt(this._groupTeams.Count);
			foreach (KeyValuePair<TeamKind, Team> keyValuePair2 in this._groupTeams)
			{
				stream.WriteByte((byte)keyValuePair2.Key);
				stream.WriteCompressedInt(this._teams.IndexOf(keyValuePair2.Value));
			}
			this._serverDispatcher.SendSnapshot(to, this.Type.Convert(), this._serverDispatcher.GetNextFrameId(), -1, this._gameTime.GetPlaybackTime(), stream.ToArray());
		}

		public void OnCleanup()
		{
			this._groupTeams.Clear();
			this._userTeams.Clear();
			this._teams.Clear();
		}

		public static BitLogger Log = new BitLogger(typeof(MatchTeams));

		private List<Team> _teams = new List<Team>();

		private Dictionary<string, Team> _userTeams = new Dictionary<string, Team>();

		private Dictionary<TeamKind, Team> _groupTeams = new Dictionary<TeamKind, Team>();

		private int _serial;
	}
}
