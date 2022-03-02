using System;
using System.Collections.Generic;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Server.Pick.Rules.Apis
{
	public interface IPriorityRolesProvider
	{
		List<DriverRoleKind> GetPriorityRoles(TeamKind team);
	}
}
