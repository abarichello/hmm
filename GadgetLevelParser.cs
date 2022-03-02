using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines
{
	public class GadgetLevelParser : KeyFrameParser, IGadgetLevelDispatcher
	{
		public override KeyFrameType Type
		{
			get
			{
				return KeyFrameType.GadgetLevel;
			}
		}

		public override void Process(BitStream stream)
		{
			int num = stream.ReadCompressedInt();
			int num2 = stream.ReadCompressedInt();
			Identifiable @object = this._objectCollection.GetObject(num);
			if (@object == null)
			{
				GadgetLevelParser.Log.ErrorFormat("Failed to find object id={0} to read {1} upgrades for.", new object[]
				{
					num,
					num2
				});
				return;
			}
			CombatObject bitComponent = @object.GetBitComponent<CombatObject>();
			if (bitComponent == null)
			{
				GadgetLevelParser.Log.ErrorFormat("Failed to find combat for object id={0} to read {1} upgrades for.", new object[]
				{
					num,
					num2
				});
				return;
			}
			GadgetLevelData gadgetLevelData = default(GadgetLevelData);
			for (int i = 0; i < num2; i++)
			{
				gadgetLevelData.ReadFromStream(stream);
				GadgetBehaviour gadget = bitComponent.GetGadget(gadgetLevelData.Slot);
				gadget.ClientSetLevel(gadgetLevelData.UpgradeName, gadgetLevelData.Level);
			}
		}

		public override bool RewindProcess(IFrame frame)
		{
			return false;
		}

		public void Update(int objectId, GadgetSlot slot, string upgradeName, int level)
		{
			BitStream stream = base.GetStream();
			stream.WriteCompressedInt(objectId);
			stream.WriteCompressedInt(1);
			new GadgetLevelData
			{
				Slot = slot,
				Level = level,
				UpgradeName = upgradeName
			}.WriteToStream(stream);
			this.LastFrameId = -1;
			this.SendKeyframe(stream.ToArray());
		}

		public void SendFullData(byte address)
		{
			List<PlayerData> playersAndBots = this._players.PlayersAndBots;
			for (int i = 0; i < playersAndBots.Count; i++)
			{
				PlayerData playerData = playersAndBots[i];
				CombatObject bitComponent = playerData.CharacterInstance.GetBitComponent<CombatObject>();
				List<GadgetLevelData> list = new List<GadgetLevelData>(16);
				this.AddGadget(bitComponent.CustomGadget0, list);
				this.AddGadget(bitComponent.CustomGadget1, list);
				this.AddGadget(bitComponent.CustomGadget2, list);
				this.AddGadget(bitComponent.BoostGadget, list);
				this.AddGadget(bitComponent.PassiveGadget, list);
				this.AddGadget(bitComponent.TrailGadget, list);
				this.AddGadget(bitComponent.OutOfCombatGadget, list);
				this.AddGadget(bitComponent.DmgUpgrade, list);
				this.AddGadget(bitComponent.HPUpgrade, list);
				this.AddGadget(bitComponent.EPUpgrade, list);
				this.AddGadget(bitComponent.BombGadget, list);
				this.AddGadget(bitComponent.GenericGadget, list);
				BitStream stream = base.GetStream();
				stream.WriteCompressedInt(bitComponent.Id.ObjId);
				stream.WriteCompressedInt(list.Count);
				for (int j = 0; j < list.Count; j++)
				{
					list[j].WriteToStream(stream);
				}
				this.SendFullFrame(address, stream.ToArray());
			}
		}

		private void AddGadget(GadgetBehaviour gadget, List<GadgetLevelData> list)
		{
			if (gadget == null)
			{
				return;
			}
			for (int i = 0; i < gadget.Upgrades.Length; i++)
			{
				GadgetLevelData item = new GadgetLevelData
				{
					Slot = gadget.Slot,
					Level = gadget.Upgrades[i].Level,
					UpgradeName = gadget.Upgrades[i].Info.Name
				};
				list.Add(item);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GadgetLevelParser));

		[Inject]
		private IIdentifiableCollection _objectCollection;

		[Inject]
		private IMatchPlayers _players;
	}
}
