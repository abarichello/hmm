using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class MultipleTargetsCannon : BasicCannon
	{
		public MultipleTargetsCannonInfo MyInfo
		{
			get
			{
				return base.Info as MultipleTargetsCannonInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.TargetsKind = new Upgradeable(this.MyInfo.TargetsUpgrade, (float)this.MyInfo.Targets, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.TargetsKind.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			this.GetTargets();
			for (int i = 0; i < this._targets.Count; i++)
			{
				Identifiable identifiable = this._targets[i];
				EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
				effectEvent.MoveSpeed = this._moveSpeed.Get();
				effectEvent.Range = this.GetRange();
				effectEvent.Origin = this.DummyPosition();
				effectEvent.Target = identifiable.transform.position;
				effectEvent.TargetId = identifiable.Id.ObjId;
				effectEvent.LifeTime = ((base.LifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.LifeTime);
				effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
				effectEvent.Modifiers = ModifierData.CopyData(this._damage);
				effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
				this._createdEffectsIds.Push(GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent));
			}
			this._targets.Clear();
			if (this._createdEffectsIds.Count <= 0)
			{
				MultipleTargetsCannon.Log.WarnFormat("Something wrong with MultipleTargetsCannon. Effect Ids Count is 0", new object[0]);
				return -1;
			}
			while (this._createdEffectsIds.Count > 1)
			{
				base.ExistingFiredEffectsAdd(this._createdEffectsIds.Pop());
			}
			return this._createdEffectsIds.Pop();
		}

		private void GetTargets()
		{
			switch (this.TargetsKind.IntGet())
			{
			case 1:
				for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
				{
					PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[i];
					this._targets.Add(playerData.CharacterInstance);
				}
				break;
			case 2:
				for (int j = 0; j < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; j++)
				{
					PlayerData playerData2 = GameHubBehaviour.Hub.Players.PlayersAndBots[j];
					if (playerData2.Team == this.Combat.Team)
					{
						this._targets.Add(playerData2.CharacterInstance);
					}
				}
				break;
			case 3:
				for (int k = 0; k < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; k++)
				{
					PlayerData playerData3 = GameHubBehaviour.Hub.Players.PlayersAndBots[k];
					if (playerData3.Team != this.Combat.Team)
					{
						this._targets.Add(playerData3.CharacterInstance);
					}
				}
				break;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MultipleTargetsCannon));

		protected Upgradeable TargetsKind;

		private readonly List<Identifiable> _targets = new List<Identifiable>(8);

		private readonly Stack<int> _createdEffectsIds = new Stack<int>(8);
	}
}
