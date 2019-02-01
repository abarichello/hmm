using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Server.Pick.Rules.Apis
{
	public interface IPriorityRolesProvider
	{
		List<CharacterInfo.DriverRoleKind> GetPriorityRoles(TeamKind team);
	}
}
