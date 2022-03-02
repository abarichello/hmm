using System;
using System.Collections.Generic;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.BI.Matches;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/BI/LogCombatGadgetBI")]
	internal class LogCombatGadgetBIBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsClient)
			{
				return this._nextBlock;
			}
			ICombatObject combatObject = (ICombatObject)ihmmgadgetContext.Owner;
			LogCombatGadgetBIBlock.GadgetActionLog gadgetActionLog = new LogCombatGadgetBIBlock.GadgetActionLog
			{
				Action = this._action,
				SteamID = combatObject.PlayerData.User.UniversalID,
				CharacterGUID = combatObject.PlayerData.Character.CharacterItemTypeGuid.ToString(),
				GadgetSlot = (int)((CombatGadget)ihmmgadgetContext).Slot,
				Parameters = new Dictionary<string, float>()
			};
			for (int i = 0; i < this._parameters.Count; i++)
			{
				IParameterTomate<float> parameterTomate = this._parameters[i].parameter.ParameterTomate as IParameterTomate<float>;
				gadgetActionLog.Parameters.Add(this._parameters[i].name, parameterTomate.GetValue(gadgetContext));
			}
			MatchLogWriter.LogGadgetAction(gadgetActionLog);
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private string _action;

		[SerializeField]
		private List<LogCombatGadgetBIBlock.NamedFloatParameter> _parameters;

		private struct GadgetActionLog : IMatchLog, IBaseLog
		{
			public string MatchId { get; set; }

			public string EventAt { get; set; }

			public int Round { get; set; }

			public int MatchTime { get; set; }

			public int OvertimeTime { get; set; }

			public string Action { get; set; }

			public string SteamID { get; set; }

			public string CharacterGUID { get; set; }

			public int GadgetSlot { get; set; }

			public Dictionary<string, float> Parameters { get; set; }
		}

		[Serializable]
		private struct NamedFloatParameter
		{
			public string name;

			public BaseParameter parameter;
		}
	}
}
