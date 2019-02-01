using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class IncreaseHpOnEnemyDeath : GadgetBehaviour
	{
		private IncreaseHpOnEnemyDeathInfo MyInfo
		{
			get
			{
				return base.Info as IncreaseHpOnEnemyDeathInfo;
			}
		}

		private void Start()
		{
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.CheckPlayerDeathEvent;
		}

		public override void SetInfo(GadgetInfo gadget)
		{
			base.SetInfo(gadget);
			this._upgrades = ModifierData.CreateData(this.MyInfo.UpgradeModifiers, this.MyInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgrades.SetLevel(upgradeName, level);
			this.RefreshMaxHpModifier();
		}

		public override void Activate()
		{
			base.Activate();
			this.RefreshMaxHpModifier();
		}

		private void CheckPlayerDeathEvent(PlayerEvent data)
		{
			float rangeSqr = this.GetRangeSqr();
			float num = Vector3.SqrMagnitude(this.Combat.Transform.position - data.Location);
			PlayerData playerByObjectId = GameHubBehaviour.Hub.Players.GetPlayerByObjectId(data.TargetId);
			TeamKind team = this.Combat.Player.Team;
			if (playerByObjectId == null || playerByObjectId.Team == team || num > rangeSqr)
			{
				return;
			}
			this._kills++;
			this.RefreshMaxHpModifier();
		}

		private void RefreshMaxHpModifier()
		{
			if (this._kills == 0)
			{
				return;
			}
			if (!base.Activated)
			{
				return;
			}
			ModifierData[] array = ModifierData.CreateConvoluted(this._upgrades, (float)this._kills);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].BuffCharges = this._kills;
			}
			this.Combat.Controller.AddPassiveModifiers(array, this.Combat, -1);
		}

		protected ModifierData[] _upgrades;

		private int _kills;
	}
}
