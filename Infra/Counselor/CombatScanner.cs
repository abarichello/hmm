using System;
using HeavyMetalMachines.Combat;
using Hoplon.SensorSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Counselor
{
	public class CombatScanner : GameHubObject, IScanner
	{
		public CombatScanner(SensorController controller, string hpParameterName, string healedParameterName, string heavyDmgParameterName, string dropBrokenLinkParameterName, string dropYellowParameterName, string dropDeathParameterName, string deathsParameterName, string spawnStateParameterName, string roleParameterName, string notMovingName, CombatObject player)
		{
			this._hpId = controller.GetHash(hpParameterName);
			this._healedId = controller.GetHash(healedParameterName);
			this._heavyDamageId = controller.GetHash(heavyDmgParameterName);
			this._dropBrokenLinkId = controller.GetHash(dropBrokenLinkParameterName);
			this._dropYellowId = controller.GetHash(dropYellowParameterName);
			this._dropDeathId = controller.GetHash(dropDeathParameterName);
			this._deathsId = controller.GetHash(deathsParameterName);
			this._spawnStateId = controller.GetHash(spawnStateParameterName);
			this._roleId = controller.GetHash(roleParameterName);
			this._notMovingTimeId = controller.GetHash(notMovingName);
			this._player = player;
			this._hasHealed = false;
			this._role = (int)player.Player.Character.Role;
			this._player.OnRepairDealt += this._player_OnRepairDealt;
			CombatController.OnInstantModifierApplied += this.OnInstantModifierApplied;
			GameHubObject.Hub.BombManager.ListenToBombDrop += this.ListenToBombDrop;
			this.Reset();
		}

		private void OnInstantModifierApplied(ModifierInstance mod, CombatObject causer, CombatObject target, float amount, int eventId)
		{
			if (target.Id.ObjId != this._player.Id.ObjId)
			{
				return;
			}
			if (mod.Info.Effect == EffectKind.HPHeavyDamage)
			{
				this._receivedHeavyDamaged = 1f;
			}
		}

		private void ListenToBombDrop(BombInstance bombInstance, SpawnReason reason, int causer)
		{
			if (causer == this._player.Id.ObjId)
			{
				if (reason != SpawnReason.Death)
				{
					if (reason != SpawnReason.TriggerDrop)
					{
						if (reason == SpawnReason.BrokenLink)
						{
							this._droppedBombBrokenLink = 1f;
						}
					}
					else
					{
						this._droppedBombYellow = 1f;
					}
				}
				else
				{
					this._droppedBombDeath = 1f;
				}
			}
		}

		~CombatScanner()
		{
			if (this._player != null)
			{
				this._player.OnRepairDealt -= this._player_OnRepairDealt;
				CombatController.OnInstantModifierApplied -= this.OnInstantModifierApplied;
				this._player = null;
			}
			CombatController.OnInstantModifierApplied -= this.OnInstantModifierApplied;
		}

		private void _player_OnRepairDealt(float arg1, int arg2)
		{
			this._hasHealed = true;
		}

		public void UpdateContext(SensorController context)
		{
			float num;
			context.GetParameter(context.MainClockId, out num);
			float num2;
			context.GetParameter(context.DeltaTimeId, out num2);
			context.SetParameter(this._healedId, (!this._hasHealed) ? 0f : 1f);
			this._hasHealed = false;
			context.SetParameter(this._heavyDamageId, this._receivedHeavyDamaged);
			this._receivedHeavyDamaged -= num2;
			context.SetParameter(this._dropBrokenLinkId, this._droppedBombBrokenLink);
			this._droppedBombBrokenLink -= num2;
			context.SetParameter(this._dropYellowId, this._droppedBombYellow);
			this._droppedBombYellow -= num2;
			context.SetParameter(this._dropDeathId, this._droppedBombDeath);
			this._droppedBombDeath -= num2;
			context.SetParameter(this._roleId, (float)this._role);
			context.SetParameter(this._spawnStateId, (float)this._player.SpawnController.State);
			context.SetParameter(this._deathsId, (float)this._player.Stats.Deaths);
			if (this._player.IsAlive() && this._player.Combat.Movement.CanMove)
			{
				context.SetParameter(this._hpId, this._player.Data.HP / (float)this._player.Data.HPMax);
				if (Mathf.Abs(this._player.CarInput.TargetV) > 0.01f)
				{
					this._lastTimeMoved = num;
				}
				context.SetParameter(this._notMovingTimeId, num - this._lastTimeMoved);
			}
			else
			{
				context.SetParameter(this._hpId, 0f);
				context.SetParameter(this._notMovingTimeId, 0f);
			}
		}

		public override string ToString()
		{
			return string.Format("CombatScanner {0} _hpId {1},_healedId {2},_heavyDamageId {3},_dropYellowId {4},_dropDeathId {5},_deathsId {6},_spawnStateId {7},_roleId {8}", new object[]
			{
				this._player.Id.ObjId,
				this._hpId,
				this._healedId,
				this._heavyDamageId,
				this._dropYellowId,
				this._dropDeathId,
				this._deathsId,
				this._spawnStateId,
				this._roleId
			});
		}

		public void Reset()
		{
			this._hasHealed = false;
			this._receivedHeavyDamaged = 0f;
			this._droppedBombBrokenLink = 0f;
			this._droppedBombYellow = 0f;
			this._droppedBombDeath = 0f;
		}

		private int _hpId;

		private int _healedId;

		private int _heavyDamageId;

		private int _dropBrokenLinkId;

		private int _dropYellowId;

		private int _dropDeathId;

		private int _deathsId;

		private int _spawnStateId;

		private int _roleId;

		private int _notMovingTimeId;

		private bool _hasHealed;

		private float _receivedHeavyDamaged;

		private float _droppedBombBrokenLink;

		private float _droppedBombYellow;

		private float _droppedBombDeath;

		private float _lastTimeMoved;

		private int _role;

		private CombatObject _player;
	}
}
