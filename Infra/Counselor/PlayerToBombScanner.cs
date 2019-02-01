using System;
using HeavyMetalMachines.Combat;
using Hoplon.SensorSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Counselor
{
	public class PlayerToBombScanner : GameHubObject, IScanner
	{
		public PlayerToBombScanner(SensorController controller, string sqrDistanceParameterName, string IsCarryingBombParameterName, CombatObject player)
		{
			this._sqrDistanceParameter = controller.GetHash(sqrDistanceParameterName);
			this._isCarryingBombParameterName = controller.GetHash(IsCarryingBombParameterName);
			this._player = player;
		}

		~PlayerToBombScanner()
		{
			this._player = null;
		}

		public void UpdateContext(SensorController context)
		{
			CombatObject combat = GameHubObject.Hub.BombManager.BombMovement.Combat;
			context.SetParameter(this._sqrDistanceParameter, Vector3.SqrMagnitude(this._player.transform.position - combat.transform.position));
			if (GameHubObject.Hub.BombManager.IsCarryingBomb(this._player.Id.ObjId))
			{
				context.SetParameter(this._isCarryingBombParameterName, 1f);
			}
			else
			{
				context.SetParameter(this._isCarryingBombParameterName, 0f);
			}
		}

		public override string ToString()
		{
			return string.Format("PlayerToBombScanner {0} _sqrDistanceParameter {1},_isCarryingBombParameterName {2}", this._player.Id.ObjId, this._sqrDistanceParameter, this._isCarryingBombParameterName);
		}

		public void Reset()
		{
		}

		private int _sqrDistanceParameter;

		private CombatObject _player;

		private int _isCarryingBombParameterName;
	}
}
