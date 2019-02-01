using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class ModifierEventParser : KeyFrameParser
	{
		public override KeyFrameType Type
		{
			get
			{
				return KeyFrameType.ModifierEvent;
			}
		}

		public override void Process(BitStream stream)
		{
			while (stream.ReadBool())
			{
				int num = stream.ReadCompressedInt();
				CombatController combatController;
				CombatObject combatObject;
				ModifierEventHolder modifierEventHolder;
				if (!GameHubObject.Hub.Stream.ControllersById.TryGetValue(num, out combatController))
				{
					combatObject = null;
					modifierEventHolder = new ModifierEventHolder();
				}
				else
				{
					modifierEventHolder = combatController.ModifierEvents;
					combatObject = combatController.Combat;
				}
				modifierEventHolder.ReadFromBitStream(stream);
				bool flag = combatObject != null;
				for (int i = 0; i < modifierEventHolder._events.Count; i++)
				{
					ModifierEventHolder.EventData eventData = modifierEventHolder._events[i];
					int otherId = eventData.OtherId;
					GadgetSlot slot = eventData.Slot;
					Identifiable @object = GameHubObject.Hub.ObjectCollection.GetObject(otherId);
					if (!(combatObject == null) || !(@object == null))
					{
						CombatObject combatObject2 = (!(@object != null)) ? null : @object.GetBitComponent<CombatObject>();
						bool flag2 = combatObject2 != null;
						ModifierEvent e = new ModifierEvent
						{
							ObjId = num
						};
						for (int j = 0; j < eventData.Count; j++)
						{
							eventData.GetEvent(ref e, j);
							if (flag)
							{
								combatObject.OnModifierReceived(e);
							}
							if (flag2)
							{
								combatObject2.OnModifierDealt(e);
							}
						}
						eventData.Clear();
					}
				}
			}
		}

		public void SendEvents()
		{
			bool flag = false;
			BitStream stream = base.GetStream();
			for (int i = 0; i < GameHubObject.Hub.Stream.ControllersList.Count; i++)
			{
				CombatController combatController = GameHubObject.Hub.Stream.ControllersList[i];
				ModifierEventHolder modifierEvents = combatController.ModifierEvents;
				if (modifierEvents.HasRecords)
				{
					flag = true;
					stream.WriteBool(true);
					stream.WriteCompressedInt(combatController.Id.ObjId);
					modifierEvents.WriteToBitStream(stream);
				}
				modifierEvents.Clear();
			}
			if (!flag)
			{
				return;
			}
			stream.WriteBool(false);
			int frameId = GameHubObject.Hub.PlaybackManager.NextId();
			GameHubObject.Hub.PlaybackManager.SendKeyFrame(this.Type, false, frameId, -1, stream.ToArray());
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ModifierEventParser));
	}
}
