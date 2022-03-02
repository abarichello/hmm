using System;
using ClientAPI;
using HeavyMetalMachines.Tournaments.DataTransferObjects;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class TeamCustomWS : GameHubObject
	{
		public static void UpdateTournamentTeamsSkills(TeamTournamentSkillsDTO teamTournamentSkillsDTO, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			TeamCustomWS.Log.DebugFormat("UpdateTournamentTeamsSkills Info={0}", new object[]
			{
				teamTournamentSkillsDTO
			});
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "UpdateTournamentTeamsSkills", teamTournamentSkillsDTO.ToString(), onSuccess, onError);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(TeamCustomWS));
	}
}
