using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.BI.GameServer;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Bomb/LogBombBI")]
	public class LogBombBIBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsServer)
			{
				MatchLogWriter.BombEvent(this._causer.GetValue<ICombatObject>(gadgetContext).Identifiable.ObjId, this._type, this._holdTime.GetValue<float>(gadgetContext), this._position.GetValue<Vector3>(gadgetContext), this._bombAngle.GetValue<float>(gadgetContext), this._isMeteor.GetValue<bool>(gadgetContext));
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private BaseParameter _causer;

		[SerializeField]
		private GameServerBombEvent.EventKind _type;

		[SerializeField]
		private BaseParameter _holdTime;

		[SerializeField]
		private BaseParameter _position;

		[SerializeField]
		private BaseParameter _bombAngle;

		[SerializeField]
		private BaseParameter _isMeteor;
	}
}
