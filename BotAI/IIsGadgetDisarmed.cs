using System;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.BotAI
{
	public interface IIsGadgetDisarmed
	{
		bool Check(ICombatObject combat);
	}
}
