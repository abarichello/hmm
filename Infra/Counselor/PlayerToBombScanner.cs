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
			if (this.AssertIsNotNull(context, "context"))
			{
				return;
			}
			if (this.AssertIsNotNull(this._player, "_player"))
			{
				return;
			}
			if (this.AssertIsNotNull(GameHubObject.Hub, "Hub"))
			{
				return;
			}
			if (this.AssertIsNotNull(GameHubObject.Hub.BombManager, "Hub.BombManager"))
			{
				return;
			}
			if (this.AssertIsNotNull(GameHubObject.Hub.BombManager.BombMovement, "Hub.BombManager.BombMovement"))
			{
				return;
			}
			if (this.AssertIsNotNull(GameHubObject.Hub.BombManager.BombMovement.Combat, "bombCombat"))
			{
				return;
			}
			if (this.AssertIsNotNull(this._player.transform, "_player.transform"))
			{
				return;
			}
			if (this.AssertIsNotNull(GameHubObject.Hub.BombManager.BombMovement.Combat.transform, "bombCombat.transform"))
			{
				return;
			}
			if (this.AssertIsNotNull(this._player.Id, "_player.Id"))
			{
				return;
			}
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

		private bool AssertIsNotNull(object obj, string name)
		{
			if (obj == null)
			{
				PlayerToBombScanner._logger.ErrorFormat("{0} is null!", new object[]
				{
					name
				});
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return string.Format("PlayerToBombScanner {0} _sqrDistanceParameter {1},_isCarryingBombParameterName {2}", this._player.Id.ObjId, this._sqrDistanceParameter, this._isCarryingBombParameterName);
		}

		public void Reset()
		{
		}

		private static readonly BitLogger _logger = new BitLogger(typeof(PlayerToBombScanner));

		private int _sqrDistanceParameter;

		private CombatObject _player;

		private int _isCarryingBombParameterName;
	}
}
