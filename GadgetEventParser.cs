using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using HeavyMetalMachines.Playback;
using HeavyMetalMachines.Utils;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class GadgetEventParser : KeyFrameParser, IGadgetEventDispatcher
	{
		public GadgetEventParser()
		{
			this._eventBuffer.OnMemberRemoved += this.OnEventRemoved;
		}

		public override KeyFrameType Type
		{
			get
			{
				return KeyFrameType.GadgetEvent;
			}
		}

		private void OnEventRemoved(GadgetEventParser.EventContextHolder ev)
		{
			this._eventsGadgets.Remove(ev.Event);
			GadgetEvent.Free((GadgetEvent)ev.Event);
		}

		public override void Process(BitStream stream)
		{
			int num = stream.ReadInt();
			int num2 = stream.ReadInt();
			int bitReadSize = stream.GetBitReadSize();
			stream.SetReadedSize(bitReadSize);
			Identifiable @object = GameHubObject.Hub.ObjectCollection.GetObject(num);
			if (@object == null)
			{
				GadgetEventParser.Log.ErrorFormat("Owner null id={0} gadget={1}", new object[]
				{
					num,
					num2
				});
				return;
			}
			IGadgetOwner component = @object.GetComponent<IGadgetOwner>();
			IHMMGadgetContext gadgetContext = component.GetGadgetContext(num2);
			GadgetEvent instance = GadgetEvent.GetInstance(0, gadgetContext);
			instance.ReadFromBitStream(stream);
			gadgetContext.TriggerEvent(instance);
		}

		public override bool RewindProcess(IFrame frame)
		{
			BitStream readData = frame.GetReadData();
			int id = readData.ReadInt();
			int id2 = readData.ReadInt();
			Identifiable @object = GameHubObject.Hub.ObjectCollection.GetObject(id);
			IGadgetOwner component = @object.GetComponent<IGadgetOwner>();
			IHMMGadgetContext gadgetContext = component.GetGadgetContext(id2);
			GadgetEvent instance = GadgetEvent.GetInstance(0, gadgetContext);
			instance.ReadFromBitStream(readData);
			GadgetEventParser.Log.DebugFormat("Rewinding frame={0} previousFrameId={1} owner={2} gadget={3} root_block={4}", new object[]
			{
				frame.FrameId,
				frame.PreviousFrameId,
				component,
				gadgetContext,
				BaseBlock.GetBlock(instance.BlockIndex)
			});
			instance.Undo();
			GadgetEvent.Free(instance);
			return frame.PreviousFrameId >= 0;
		}

		public void SendEvent(IHMMGadgetContext gadgetContext, IHMMEventContext eventContext)
		{
			BitStream stream = base.GetStream();
			this.WriteToBitStream(stream, gadgetContext, eventContext);
			GadgetEventParser.EventContextHolder holder = new GadgetEventParser.EventContextHolder(eventContext);
			if (this._eventBuffer.AddMember(holder))
			{
				this._eventsGadgets.Add(eventContext, gadgetContext);
			}
			this._dispatcher.SendFrame(this.Type.Convert(), true, eventContext.Id, eventContext.PreviousEventId, stream.ByteArray);
		}

		public void SendAllEvents(byte address)
		{
			for (int i = 0; i < this._eventBuffer.LiveHolders.Count; i++)
			{
				GadgetEventParser.EventContextHolder eventContextHolder = this._eventBuffer.LiveHolders[i];
				IHMMEventContext @event = eventContextHolder.Event;
				IHMMGadgetContext gadgetContext = this._eventsGadgets[@event];
				BitStream stream = base.GetStream();
				this.WriteToBitStream(stream, gadgetContext, @event);
				this._dispatcher.SendSnapshot(address, this.Type.Convert(), @event.Id, @event.PreviousEventId, @event.CreationTime, stream.ByteArray);
				GadgetEventParser.Log.InfoFormat("Sending full frame Gadget BlockIndex = {0}", new object[]
				{
					@event.BlockIndex
				});
			}
			for (int j = 0; j < GameHubObject.Hub.Players.PlayersAndBots.Count; j++)
			{
				CombatObject combat = CombatRef.GetCombat(GameHubObject.Hub.Players.PlayersAndBots[j].CharacterInstance);
				List<IHMMGadgetContext> customGadgets = combat.CustomGadgets;
				for (int k = 0; k < customGadgets.Count; k++)
				{
					if (customGadgets[k] is CombatGadget)
					{
						GadgetEvent instance = GadgetEvent.GetInstance(-1, customGadgets[k], ((CombatGadget)customGadgets[k]).GetAllUIParameters());
						BitStream stream = base.GetStream();
						this.WriteToBitStream(stream, customGadgets[k], instance);
						this._dispatcher.SendSnapshot(address, this.Type.Convert(), instance.Id, instance.PreviousEventId, instance.CreationTime, stream.ByteArray);
					}
				}
			}
		}

		private void WriteToBitStream(BitStream stream, IHMMGadgetContext gadgetContext, IHMMEventContext eventContext)
		{
			stream.WriteInt(gadgetContext.Owner.Identifiable.ObjId);
			stream.WriteInt(gadgetContext.Id);
			eventContext.WriteToBitStream(stream);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GadgetEventParser));

		private NodeHolderLifeCycle<GadgetEventParser.EventContextHolder> _eventBuffer = new NodeHolderLifeCycle<GadgetEventParser.EventContextHolder>();

		private Dictionary<IHMMEventContext, IHMMGadgetContext> _eventsGadgets = new Dictionary<IHMMEventContext, IHMMGadgetContext>();

		private class EventContextHolder : INodeHolder
		{
			public EventContextHolder(IHMMEventContext evt)
			{
				this.Event = evt;
				List<int> list;
				List<int> list2;
				evt.GetBodies(out list, out list2);
				this.NodeIds = list.ToArray();
				this.NodeRemovals = list2.ToArray();
			}

			public int[] NodeIds { get; private set; }

			public int[] NodeRemovals { get; private set; }

			public IHMMEventContext Event { get; private set; }
		}
	}
}
